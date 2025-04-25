using System.Globalization;
using System.Text;
using InfluxDB.Client;
using Meterdata;

namespace InfluxDbWorker;

public class InfluxDbHelper
{
    internal static readonly string Token = "t8mvNAx4qJjk8TXu4ldEZ8QzFwrnPeU4FIJGHPuKSblgWpNL4d5lfng4WQpjRxyoF6lyYiOkLAxqJysKNp3Row==";
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

    public static async Task ShowOrganizationsAsync()
    {
        using var client = new InfluxDBClient("http://localhost:8086", Token);
        var orgClient = client.GetOrganizationsApi();
        var orgs = await orgClient.FindOrganizationsAsync();
        foreach (var org in orgs)
        {
            Console.WriteLine(org.Name);
        }
    }
    
    public static string GetInfluxLine(MeterReading reading)
    {
        var lineProtocol = new StringBuilder();
        foreach (var pointReading in reading.Readings)
        {
            lineProtocol.AppendLine(
                $"consumption,meterid=\"{reading.MeterId}\" value={pointReading.Value.ToString(CultureInfo.InvariantCulture)} {(pointReading.Timestamp - UnixEpoch).TotalNanoseconds}");
        }

        return lineProtocol.ToString();
    }
}