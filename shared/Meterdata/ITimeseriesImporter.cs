namespace Meterdata;

public interface ITimeseriesImporter
{
    Task ImportDataAsync(string fileName, IEnumerable<MeterReading> readings);
}