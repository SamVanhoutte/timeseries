using Meterdata;

namespace InfluxDbWorker;

public class InfluxTimeseriesImporter(ILogger<InfluxTimeseriesImporter> logger) : ITimeseriesImporter
{
    public Task ImportDataAsync(IEnumerable<MeterReading> readings)
    {
        foreach (var reading in readings)
        {
            logger.LogInformation($"Meter {reading.MeterId} has {reading.Readings.Length} readings on {reading.Readings.First().Item1}");
        }
        return Task.CompletedTask;
    }
}