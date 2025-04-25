using System.Globalization;
using System.Text;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Meterdata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InfluxDbWorker;

public class InfluxDbDirectTimeseriesImporter(ILogger<InfluxDbDirectTimeseriesImporter> logger, IConfiguration configuration) : ITimeseriesImporter
{
    private InfluxDBClient? client;
    private WriteApiAsync? writeApi;

    public Task ImportDataAsync(string fileName, IEnumerable<MeterReading> readings)
    {
        try
        {
            EnsureConnection();
            foreach (var meterReading in readings)
            {
                writeApi.WriteRecordAsync(InfluxDbHelper.GetInfluxLine(meterReading), WritePrecision.Ns, configuration["ContainerName"], "docs");
            }

        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while importing data: {Message}", e.Message);
        }

        return Task.CompletedTask;
    }

    public Task CloseAsync()
    {
        client?.Dispose();
        return Task.CompletedTask;
    }

    private WriteApiAsync EnsureConnection()
    {
        if (client == null)
        {
            client = new InfluxDBClient("http://localhost:8086", InfluxDbHelper.Token);
        }

        if (writeApi == null)
        {
            writeApi = client.GetWriteApiAsync();
        }
        return writeApi;
    }
}