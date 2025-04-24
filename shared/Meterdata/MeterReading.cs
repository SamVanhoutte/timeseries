namespace Meterdata;

public class MeterReading
{
    public string MeterId { get; set; }
    public (DateTime Timestamp, double Value)[] Readings { get; set; }
}