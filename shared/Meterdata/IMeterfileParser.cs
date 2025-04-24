namespace Meterdata;

public interface IMeterfileParser
{
    Task<(long TotalRecords, long TotalEmptyReadings)> ParseMeterDataAsync(string filePath, Func<MeterReading, Task> processData);
}