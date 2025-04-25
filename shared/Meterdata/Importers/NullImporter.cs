namespace Meterdata.Importers;

public class NullImporter : ITimeseriesImporter
{
    public Task ImportDataAsync(string fileName, IEnumerable<MeterReading> readings)
    {
        return Task.CompletedTask;
    }

    public Task CloseAsync()
    {
        return Task.CompletedTask;
    }
}