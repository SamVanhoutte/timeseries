using Meterdata.Reporting;

namespace ResultParser;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Please give the directory path to the files you want to parse:");
        //string? directoryPath = Console.ReadLine();
        string? directoryPath = "/Users/sam/git/timeseries/shared/TimeseriesWorker";
        bool addHeader = true;
        foreach (var file in Directory.GetFiles(directoryPath, "*.csv"))
        {
            var fileInfo = new FileInfo(file);
            var resultFile = fileInfo.Name;
            // Structure of file name is : {date}-{databaseType}-({batchSize}){('Parallel' :'Serial')}-{comment}.csv
            var runTime = resultFile[..19];
            resultFile = resultFile[20..];
            var databaseType = resultFile.Split('-')[0];
            var batchSize = resultFile.Split('-')[1].Split(')')[0].Replace("(", "");
            var parallel = resultFile.Split('-')[1].Split(')')[1];
            var comment = resultFile.Split($"){parallel}-")[1].Split('.')[0];
            var results = await ReadResultsAsync(file);
            await WriteFullResultSetAsync("../../../../../fullresults.csv", addHeader, results, runTime, batchSize,
                parallel, comment, databaseType);
            addHeader = false;
            Console.WriteLine(runTime + " " + databaseType + " " + batchSize + " " + parallel + " " + comment);
        }
    }

    private static async Task WriteFullResultSetAsync(string outputFile, bool addHeader, List<FileImportResult> results,
        string runTime, string batchSize, string parallel, string comment, string database)
    {
        await using var writer = new StreamWriter(outputFile,
            options: new FileStreamOptions { Access = FileAccess.Write, Mode = FileMode.Append });
        if (addHeader)
        {
            await writer.WriteLineAsync("RunTime,Database,BatchSize,Parallel,Comment,Sequence,FileName,Start,End,Duration,TotalRecords,TotalInifiniteValues");
        }

        foreach (var result in results)
        {
            await writer.WriteLineAsync(
                $"{runTime},{database},{batchSize},{parallel},{comment},{result.Sequence},{result.FileName},{result.Start},{result.End},{result.Duration},{result.TotalRecords},{result.TotalInifiniteValues}");
        }
    }

    private static async Task<List<FileImportResult>> ReadResultsAsync(string fileName)
    {
        var results = new List<FileImportResult>();
        using var reader = new StreamReader(fileName);
        string? line;
        await reader.ReadLineAsync(); // read header line
        while ((line = await reader.ReadLineAsync()) != null)
        {
            var parts = line.Split(',');
            if (parts.Length == 7)
            {
                results.Add(new FileImportResult
                {
                    Sequence = int.Parse(parts[0]),
                    FileName = parts[1],
                    Start = DateTimeOffset.Parse(parts[2]),
                    End = DateTimeOffset.Parse(parts[3]),
                    TotalRecords = long.Parse(parts[5]),
                    TotalInifiniteValues = long.Parse(parts[6])
                });
            }
        }

        return results;
    }
}