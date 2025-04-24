namespace Meterdata.Importers;

public class NullImporter : ITimeseriesImporter
{
    public Task ImportDataAsync(string fileName, IEnumerable<MeterReading> readings)
    {
        return Task.CompletedTask;
    }
}