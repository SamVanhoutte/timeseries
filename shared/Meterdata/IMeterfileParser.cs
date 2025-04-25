namespace Meterdata;

public interface IMeterfileParser
{
    bool SingleFile { get; }
    Task<(long TotalRecords, long TotalEmptyReadings)> ParseMeterDataAsync(string filePath, Func<MeterReading, Task> processData);
}