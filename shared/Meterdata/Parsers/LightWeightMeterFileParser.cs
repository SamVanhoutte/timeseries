using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Meterdata.Extensions;
using Alphirk.Simulation;

namespace Meterdata.Parsers;

public class LightWeightMeterfileParser : IMeterfileParser
{
    // This parser will just submit quarter hourly readings for yesterday, for one meter and should be used for easy testing
    public bool SingleFile => true;

    public async Task<(long, long)> ParseMeterDataAsync(string filePath, Func<MeterReading, Task> processData)
    {
        var random = new Random();
        var yesterday = DateTime.Today.AddDays(-1);
        var reading = new MeterReading
        {
            MeterId = "TestMeter2",
            MeasurementType = "consumption",
            Readings = new (DateTime, double)[4 * 24]
        };

        for (int qh = 1; qh <= 4 * 24; qh++)
        {
            reading.Readings[qh - 1] = (yesterday.AddMinutes(15 * qh), Math.Abs(random.NextGaussian() * 5));
        }

        await processData(reading);

        return (24 * 4, 0);
    }
}