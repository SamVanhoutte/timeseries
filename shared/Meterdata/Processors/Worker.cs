using Meterdata.Reporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meterdata.Processors;

public class Worker(
    ILogger<Worker> logger,
    IConfiguration configuration,
    IMeterfileParser meterfileParser,
    Reporter reporter,
    IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var directory = configuration["ImportDirectory"];
        var batchSizeValue = configuration["BatchSize"];
        var destination = configuration["Destination"];
        Console.WriteLine("Please enter a comment for the import:");
        var comment = Console.ReadLine();
        var timeseriesImporter = serviceProvider.GetRequiredKeyedService<ITimeseriesImporter>(destination);
        if (!int.TryParse(batchSizeValue, out int batchSize))
        {
            batchSize = 1000;
        }

        if (meterfileParser.SingleFile)
        {
            try
            {
                await ProcessFileAsync(timeseriesImporter, batchSize);
            }
            finally
            {
                await timeseriesImporter.CloseAsync();
            }
        }
        else
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            logger.LogInformation($"Working to import files from {di.FullName}");
            if (Directory.Exists(directory))
            {
                logger.LogInformation("DataImporter running at: {time}", DateTimeOffset.Now);
                var files = Directory.GetFiles(directory, "*.csv");
                if (files.Any())
                {
                    try
                    {
                        int fileId = 1;
                        //await Parallel.ForEachAsync(files, async (file, token) =>
                        foreach (var file in files)
                        {
                            await ProcessFileAsync(timeseriesImporter, batchSize, file, fileId, files.Length);
                            fileId++;
                        }

                        //);
                    }
                    finally
                    {
                        await timeseriesImporter.CloseAsync();
                    }
                }
            }
            else
            {
                logger.LogWarning("Directory {directory} does not exist", directory);
            }
        }
        await reporter.PersistResultsAsync(destination, batchSize, false, comment);
        logger.LogInformation("DataImporter finished at: {time}", DateTimeOffset.Now);
    }

    private async Task ProcessFileAsync(ITimeseriesImporter timeseriesImporter, int batchSize,
        string file = "", int fileId = 1, int fileCount = 1)
    {
        var startTime = DateTimeOffset.Now;
        logger.LogInformation($"Processing file: {{file}} ({fileId} / {fileCount})", file);
        var readings = new List<MeterReading>();
        var readingCounts = await meterfileParser.ParseMeterDataAsync(file, async (reading) =>
        {
            // logger.LogInformation("Processing reading for meter {meterId}", reading.MeterId);
            readings.Add(reading);
            if (readings.Count >= batchSize)
            {
                await timeseriesImporter.ImportDataAsync($"{file} ({fileId} / {fileCount})",
                    readings);
                readings = new List<MeterReading>();
            }
        });
        await timeseriesImporter.ImportDataAsync($"{file} ({fileId} / {fileCount})", readings);
        var endTime = DateTimeOffset.Now;
        reporter.ReportFile(file, startTime, endTime, readingCounts.TotalRecords,
            readingCounts.TotalEmptyReadings);
    }
}