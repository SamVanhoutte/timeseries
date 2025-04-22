namespace Meterdata;

public class MeterReading
{
    public string MeterId { get; set; }
    public (DateTime, double)[] Readings { get; set; }
}