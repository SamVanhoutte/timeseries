# Findings on TimescaleDb

## Setting up TimescaleDb in Docker, locally

Running the image (after creating the `.env.influxdb2-admin-*` files with secrets) can be done by executing this in the setup folder.  This will create two different containers.  One with the database, another one with the PGAdmin administration interface.

```bash
docker compose -p timescale up
```

## Administer and manage InfluxDb

Just navigate to http://localhost:8080/ and login with the management credentials and connect to the database

## Main concepts

- **Hyper table**: a virtual table, built on top of regular PostgreSQL tables, which is optimized for time series data.  It partitions data across time (and optionally space), for faster querying and storage.


## Defining our artifacts

First we just define the table, nothing special here. 
Except that it's advised to use lowerCase casing.

```sql
DROP TABLE lmc.meter_readings;

CREATE TABLE lmc.meter_readings (
    meter_id     TEXT NOT NULL,
    measure_time   TIMESTAMPTZ NOT NULL,
    consumption DOUBLE PRECISION NOT NULL,
    label     INTEGER NOT NULL,
    PRIMARY KEY (meter_id, measure_time)
);
```

On that table, we define a Hyper Table (TimescaleDB specific) that we partition by meterid (making 10 partitions here) and obviously , we use the `measure_time` column as time series.

> It is very important to perform this before data is being written into the table!

```sql
-- Create hyper table, using MeasureTime as time field, partition by MeterId (10 partitions)
SELECT create_hypertable('lmc.meter_readings', 'measure_time', 'meter_id', 10);
```

Adding indexing will drastically improve query performance

```sql
CREATE INDEX ON lmc.meter_readings (measure_time DESC);
CREATE INDEX ON lmc.meter_readings (meter_id);
```

### Continuous aggregates

Continuous aggregates are one of TimescaleDB’s most powerful features — they let you pre-compute and store aggregate data (like averages, sums, etc.) over time intervals to speed up queries dramatically.  So, we are doing this in the next step.

```sql
CREATE MATERIALIZED VIEW lmc.daily_total_consumption
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('1 day', measure_time) AS bucket,
    meter_id,
    SUM(consumption) AS total_consumption
FROM
    lmc.meter_readings
GROUP BY
    bucket, meter_id;
```

It's also good to have these refreshed automatically, an example to schedule these refreshes can be found below. 

```SQL
SELECT add_continuous_aggregate_policy('lmc.daily_total_consumption',
    start_offset => INTERVAL '366 days',
    end_offset   => INTERVAL '1 second',
    schedule_interval => INTERVAL '1 day');
```

Querying such a view can easily be done as follows

```SQL
SELECT *
FROM lmc.daily_total_consumption
WHERE bucket >= now() - INTERVAL '7 days'
```

### Upserting data

Upserting data should be done, when using UNIQUE indexes.  And it's done with a `INSERT ... ON CONFLICT` approach.

```SQL
INSERT INTO conditions
  VALUES ('2017-07-28 11:42:42.846621+00', 'office', 70.2, 50.1)
  ON CONFLICT (time, location) DO UPDATE
    SET temperature = excluded.temperature,
        humidity = excluded.humidity;
```