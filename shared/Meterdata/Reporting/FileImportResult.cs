namespace Meterdata.Reporting;

public class FileImportResult
{
    public string FileName { get; set; }
    public int Sequence { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public long Duration => (int)(End - Start).TotalMilliseconds;
    public long TotalRecords { get; set; }
    public long TotalInifiniteValues { get; set; }
}