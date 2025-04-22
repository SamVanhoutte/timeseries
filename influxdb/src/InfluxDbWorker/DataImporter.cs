using Meterdata;

namespace InfluxDbWorker;

public class DataImporter(
    ILogger<DataImporter> logger,
    IConfiguration configuration,
    IMeterfileParser meterfileParser,
    ITimeseriesImporter timeseriesImporter) : BackgroundService
{
    private const int BatchSize = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var directory = configuration.GetValue<string>("ImportDirectory");
        DirectoryInfo di = new DirectoryInfo(directory);
        Console.WriteLine(di.FullName);
        if (Directory.Exists(directory))
        {
            logger.LogInformation("DataImporter running at: {time}", DateTimeOffset.Now);
            var files = Directory.GetFiles(directory, "*.csv");
            if (files.Any())
            {
                var readings = new List<MeterReading>();
                await meterfileParser.ParseMeterDataAsync(files.First(), async (reading) =>
                {
                    logger.LogInformation("Processing reading for meter {meterId}", reading.MeterId);
                    readings.Add(reading);
                    if (readings.Count >= BatchSize)
                    {
                        await timeseriesImporter.ImportDataAsync(readings);
                        readings = new List<MeterReading>();
                    }
                });
                await timeseriesImporter.ImportDataAsync(readings);
            }
        }
        else
        {
            logger.LogInformation("Directory {directory} does not exist", directory);
        }
        logger.LogInformation("DataImporter finished at: {time}", DateTimeOffset.Now);
    }
}