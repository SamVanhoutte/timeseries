using Cassandra;
using Meterdata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CassandraWorker;

public class CassandraTimeseriesImporter(ILogger<CassandraTimeseriesImporter> logger, IConfiguration configuration) : ITimeseriesImporter
{
    private ISession? session;
    private ICluster? cluster;
    private PreparedStatement? preparedInsertQuery;
    public async Task ImportDataAsync(string fileName, IEnumerable<MeterReading> readings)
    {
        await EnsureConnectionAsync();
        foreach (var rd in readings)
        {
            foreach (var reading in rd.Readings)
            {
                await session!.ExecuteAsync( preparedInsertQuery!.Bind(rd.MeterId, reading.Timestamp, reading.Value, rd.MeasurementType));
            }
        }
    }

    public Task CloseAsync()
    {
        // Optionally close connection
        cluster?.Dispose();
        return Task.CompletedTask;
    }

    private async Task EnsureConnectionAsync()
    {
        // Define Cassandra connection
        cluster ??= Cluster.Builder()
            .AddContactPoint("127.0.0.1") // change to your Cassandra host
            .Build();

        if (session == null)
        {
            
            session ??= await cluster.ConnectAsync();
            session!.ChangeKeyspace("timeseries");
            preparedInsertQuery = await session.PrepareAsync(
                $"INSERT INTO {configuration["ContainerName"]} (meter_id, measure_time, consumption, label) VALUES (?, ?, ?, ?)");

        }
    }
    // CREATE TABLE IF NOT EXISTS meter_readings (
    //     meter_id text,
    //     measure_time timestamp,
    //     consumption double,
    // label text,
    //     PRIMARY KEY (meter_id, measure_time)
    //     ) WITH CLUSTERING ORDER BY (measure_time DESC);
   
}