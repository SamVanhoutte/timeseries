namespace Meterdata;

using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

public class LondonMeterfileParser : IMeterfileParser
{
    public async Task ParseMeterDataAsync(string filePath, Func<MeterReading, Task> processData)
    {
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
                    var day = DateTime.ParseExact(dayValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
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

                    await processData(reading);
                }
            }
        }
    }
}