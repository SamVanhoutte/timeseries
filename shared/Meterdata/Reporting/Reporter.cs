using Microsoft.Extensions.Logging;

namespace Meterdata.Reporting;

public class Reporter(ILogger<Reporter> logger)
{
    private List<FileImportResult> results = new List<FileImportResult>();
    public void ReportFile(string fileName, DateTimeOffset start, DateTimeOffset end, long totalRecords, long totalInfiniteValues)
    {
        results.Add(new FileImportResult
        {
            Sequence = results.Count + 1,
            FileName = fileName,
            Start = start,
            End = end,
            TotalRecords = totalRecords,
            TotalInifiniteValues = totalInfiniteValues
        });
        
        logger.LogInformation("{TotalRecords} imported in {Duration}", totalRecords, (end-start));
    }
    
    public async Task PersistResultsAsync(string databaseType, int batchSize, bool parallel, string comment)
    {
        var reportFileName = $"{DateTime.Now:s}-{databaseType}-({batchSize}){(parallel ? 'P' :'S')}-{comment}.csv";
        await using var writer = new StreamWriter(reportFileName);
        await writer.WriteLineAsync("Sequence,FileName,Start,End,Duration,TotalRecords,TotalInifiniteValues");
        foreach (var result in results)
        {
            await writer.WriteLineAsync($"{result.Sequence},{result.FileName},{result.Start},{result.End},{result.Duration},{result.TotalRecords},{result.TotalInifiniteValues}");
        }
    }
}