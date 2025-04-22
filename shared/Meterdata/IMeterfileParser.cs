namespace Meterdata;

public interface IMeterfileParser
{
    Task ParseMeterDataAsync(string filePath, Func<MeterReading, Task> processData);
}