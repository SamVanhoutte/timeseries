using System.Globalization;
using System.Text;
using Meterdata;
using Meterdata.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace TimescaleDbWorker;

public class TimescaleDbTimeseriesImporter(ILogger<TimescaleDbTimeseriesImporter> logger, IConfiguration configuration) : ITimeseriesImporter
{
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

    private NpgsqlConnection? connection;


    public async Task ImportDataAsync(string fileName, IEnumerable<MeterReading> readings)
    {
        var con = await EnsureConnectionAsync();
        try
        {
            await using var writer = await con.BeginBinaryImportAsync(
                $"COPY lmc.{configuration["ContainerName"]} (meter_id, measure_time, consumption, label) FROM STDIN (FORMAT BINARY)");
            foreach (var reading in readings)
            {
                foreach (var measurement in reading.Readings.Where(r => !r.Timestamp.IsInfiniteOrEmpty()))
                {
                    // Ensure the Timestamp is in UTC
                    var timestamp = measurement.Timestamp.Kind == DateTimeKind.Utc
                        ? measurement.Timestamp
                        : DateTime.SpecifyKind(measurement.Timestamp, DateTimeKind.Utc);
                    if (timestamp != null)
                    {
                        //logger.LogInformation($"{measurement.Value}");
                        await writer.StartRowAsync().ConfigureAwait(false);
                        await writer.WriteAsync(reading.MeterId, NpgsqlTypes.NpgsqlDbType.Varchar).ConfigureAwait(false);
                        await writer.WriteAsync(timestamp, NpgsqlTypes.NpgsqlDbType.TimestampTz).ConfigureAwait(false);
                        await writer.WriteAsync(measurement.Value, NpgsqlTypes.NpgsqlDbType.Double).ConfigureAwait(false);
                        await writer.WriteAsync("initial", NpgsqlTypes.NpgsqlDbType.Text).ConfigureAwait(false);
                    }
                }
            }

            await writer.CompleteAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while importing data: {Message}", e.Message);
        }
    }

    public Task CloseAsync()
    {
        connection?.Close();
        return Task.CompletedTask;
    }

    private async Task<NpgsqlConnection> EnsureConnectionAsync()
    {
        if (connection == null)
        {
            connection = new NpgsqlConnection(
                connectionString:
                "Server=localhost;Port=5432;User Id=postgres;Password=MyInitialAdminPassword;Database=postgres;Include Error Detail=true;");
            connection.Open();
        }

        return connection;
    }
}