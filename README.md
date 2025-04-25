# Time series databases

A repo comparing different time series databases

## Relevant data sets

I am using the following data sets in the samples: 

- https://www.kaggle.com/datasets/pythonafroz/swiss-smart-meter-data?resource=download (I only focused on the )

Another relevant data set can be the following: 
- https://www.kaggle.com/datasets/jeanmidev/smart-meters-in-london

## General findings

The following table shows the different things that were compared, and allow to click through to the details on the different databases that were tested. 

| Database | Import duration | Upsert method | Scalability | Query performance | Details |
|---|---|---|---|---|---|
| InfluxDb | 134 sec | Silent overwrite | - | - | [Details](./influxdb/influxdb.cmd) |
| TimescaleDb | 122 sec | - | - | - | [Details](./timescaledb/timescaledb.cmd) |
| CassandraDb | - | - | - | - | [Details](./cassandradb/cassandradb.cmd) |