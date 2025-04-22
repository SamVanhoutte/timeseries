namespace Meterdata;

public interface ITimeseriesImporter
{
    Task ImportDataAsync(IEnumerable<MeterReading> readings);
}