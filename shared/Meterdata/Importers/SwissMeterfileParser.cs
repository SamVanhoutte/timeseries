namespace Meterdata;

using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

public class SwissMeterfileParser : IMeterfileParser
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

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        int rowNumber = 0;

        // Read each record (row) one by one
        while (await csv.ReadAsync())
        {
            rowNumber++;

            if (csv.TryGetField("id", out string? meterId) && csv.TryGetField("month", out string? monthValue))
            {
                if (!string.IsNullOrEmpty(meterId))
                {
                    var reading = new MeterReading { MeterId = meterId, Readings = [] };
                    for (int hr = 0; hr < 24; hr++)
                    {
                        if (csv.TryGetField($"H{hr}", out string? hourValue))
                        {
                            var readingTime = DateTime.ParseExact(hourValue, "yyyyMMdd", CultureInfo.InvariantCulture);
                            reading.Readings[hr] = (readingTime, double.Parse(hourValue, CultureInfo.InvariantCulture));
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