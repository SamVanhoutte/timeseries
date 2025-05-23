using System.Globalization;
using System.Text;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Meterdata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InfluxDbWorker;

public class InfluxDbTimeseriesImporter(ILogger<InfluxDbTimeseriesImporter> logger, IConfiguration configuration)
    : ITimeseriesImporter
{
    private InfluxDBClient? client;
    private WriteApi? writeApi;


    public Task ImportDataAsync(string fileName, IEnumerable<MeterReading> readings)
    {
        try
        {
            EnsureConnection();
            foreach (var meterReading in readings)
            {
                writeApi.WritePoints(meterReading.Readings.Select(r => PointData.Measurement(meterReading.MeasurementType)
                    .Tag("meterid", meterReading.MeterId)
                    .Field("value", r.Value)
                    .Timestamp(r.Timestamp, WritePrecision.S)).ToArray(), configuration["ContainerName"], "docs");
                writeApi.Flush();

                // writeApi.WriteRecord(InfluxDbHelper.GetInfluxLine(meterReading), WritePrecision.Ns, configuration["ContainerName"], "docs");
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
        writeApi?.Dispose();
        client?.Dispose();
        return Task.CompletedTask;
    }

    private WriteApi EnsureConnection()
    {
        if (client == null)
        {
            client = new InfluxDBClient("http://localhost:8086", InfluxDbHelper.Token);
        }

        if (writeApi == null)
        {
            writeApi = client.GetWriteApi();
        }

        return writeApi;
    }
}