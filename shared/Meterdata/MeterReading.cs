namespace Meterdata;

public class MeterReading
{
    public string MeterId { get; set; }
    public string MeasurementType { get; set; } = "consumption";
    public (DateTime Timestamp, double Value)[] Readings { get; set; }
}