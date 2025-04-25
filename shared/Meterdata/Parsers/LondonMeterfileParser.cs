using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Meterdata.Extensions;

namespace Meterdata.Parsers;

public class LondonMeterfileParser : IMeterfileParser
{
    // Last timestamp of dataset , all times will be shifted towards current day
    private const string FirstTimeStamp = "2011-11-24T00:00:00Z";
    private const string LastTimeStamp = "2013-02-25T00:00:00Z";
    
    public bool SingleFile => false;

    public async Task<(long, long)> ParseMeterDataAsync(string filePath, Func<MeterReading, Task> processData)
    {
        var totalRecords = 0;
        var totalInfiniteValues = 0;
        var daysToShift = (int)DateTime.UtcNow.Subtract( DateTime.ParseExact(LastTimeStamp, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)).TotalDays;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true, // Set to true if your CSV has a header row
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim, // Optional: trims whitespace around fields
            BadDataFound = null // Optional: ignore malformed lines
        };

        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        int rowNumber = 0;
        await csv.ReadAsync();
        csv.ReadHeader();
        // Read each record (row) one by one
        while (await csv.ReadAsync())
        {
            rowNumber++;

            if (csv.TryGetField("LCLid", out string? meterId) && csv.TryGetField("day", out string? dayValue))
            {
                if (!string.IsNullOrEmpty(meterId))
                {
                    var fileDay = DateTime.ParseExact(dayValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    // Shift data here
                    var day = fileDay.AddDays(daysToShift);
                    var reading = new MeterReading { MeterId = meterId, Readings = new (DateTime, double)[48] };
                    for (int hhr = 1; hhr <= 48; hhr++)
                    {
                        if (csv.TryGetField<double>($"hh_{hhr - 1}", out var halfHourValue))
                        {
                            reading.Readings[hhr - 1] = (day.AddMinutes(30 * hhr), halfHourValue);
                        }
                        else
                        {
                            break;
                        }
                    }
                    totalRecords+= reading.Readings.Length;
                    totalInfiniteValues+= reading.Readings.Count(r => r.Timestamp.IsInfiniteOrEmpty());

                    await processData(reading);
                }
            }
        }
        return (totalRecords, totalInfiniteValues);
    }
}