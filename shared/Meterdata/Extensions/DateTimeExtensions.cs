namespace Meterdata.Extensions;

public static class DateTimeExtensions
{
    public static bool IsInfiniteOrEmpty(this DateTime? dateTime)
    {
        return dateTime == null || dateTime.Value.IsInfiniteOrEmpty();
    }
    
    public static bool IsInfiniteOrEmpty(this DateTime dateTime)
    {
        return dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue;
    }
}