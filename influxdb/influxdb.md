# Findings on InfluxDb

## Setting up InfluxDb in Docker, locally

Based on the official docs: https://docs.influxdata.com/influxdb/v2/install/use-docker-compose/

Running the image (after creating the `.env.influxdb2-admin-*` files with secrets) can be done by executing this

```
docker compose up -p influx influxdb2
```

## Administer and manage InfluxDb

Just navigate to http://localhost:8086/ and login with your credentials

## Main concepts

- **Buckets**: A bucket is a named location where time series data is stored. All buckets have a Retention Policy, a duration of time that each data point persists.
- 

# Data operations

## Writing data

Using the LineProtocol, you add a measurement with 
- a name
- zero or more tags (name/value), aka columns
- a timestamp with a given precision (nano, micro, milli, second)

An example of the line protocol, built in C#:

```csharp
var line = $"consumption,meterid=\"{reading.MeterId}\" value={pointReading.Value.ToString(CultureInfo.InvariantCulture)} {(pointReading.Timestamp - UnixEpoch).TotalNanoseconds}");
```

## Overwriting same data point


## Querying data

An example query to get the total consumption in tha past 7 days for each meter:

```
from(bucket: "meter01")
  |> range(start: -7d)
  |> filter(fn: (r) => r._measurement == "consumption")
  |> aggregateWindow(every: 1d, fn: sum, createEmpty: false)
  |> group(columns: ["MeterId"])
  |> yield(name: "sum_per_day_per_meter")
```

```
from(bucket: "lightweight")
  |> range(start: v.timeRangeStart, stop: v.timeRangeStop)
  |> filter(fn: (r) => r["_measurement"] == "prediction")
  |> filter(fn: (r) => r["meterid"] == "TestMeter")
  |> filter(fn: (r) => r["_field"] == "value")
  |> aggregateWindow(every: v.windowPeriod, fn: mean, createEmpty: false)
  |> yield(name: "mean")
```