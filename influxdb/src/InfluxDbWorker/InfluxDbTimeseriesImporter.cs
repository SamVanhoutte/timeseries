using System.Globalization;
using System.Text;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Meterdata;
using Microsoft.Extensions.Logging;

namespace InfluxDbWorker;

public class InfluxDbTimeseriesImporter(ILogger<InfluxDbTimeseriesImporter> logger) : ITimeseriesImporter
{
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

    public Task ImportDataAsync(string fileName, IEnumerable<MeterReading> readings)
    {
        try
        {
            using var client = new InfluxDBClient("http://localhost:8086", InfluxDbHelper.Token);
            var writeApi = client.GetWriteApiAsync();
            foreach (var meterReading in readings)
            {
                writeApi.WriteRecordAsync(GetInfluxLine(meterReading), WritePrecision.Ns, "meter00", "docs");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while importing data: {Message}", e.Message);
        }

        return Task.CompletedTask;
    }

    private string GetInfluxLine(MeterReading reading)
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