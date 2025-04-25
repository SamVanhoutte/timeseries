# Findings on Cassandra

## Setting up Cassandra in Docker, locally

Well explained [here](https://cassandra.apache.org/_/quickstart.html)

```bash
docker pull cassandra:latest
docker network create cassandra
docker run --rm -d --name cassandra --hostname cassandra --network cassandra cassandra
```
 Or use the docker compose file in the `setup` folders

```bash
docker compose -p cassandra up
```

## Administer and manage Cassandra

Best way is to use [Cassandra Workbench](https://marketplace.visualstudio.com/items/?itemName=kdcro101.vscode-cassandra) in Visual Studio Code.
Rider also allows to connect to a keyspace


## Defining our artifacts

**Key space**: this is like the database, wich a replication strategy and factor (number of copies).

Creating a key space, can be done like the following:

```sql
CREATE KEYSPACE timeseries
WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 3};
```

**Table (column family)**: a database table with each row having the possibility to have different number of columns (wide row stores)


First we just define the table, nothing special here. 
It's important to consider the key, as this will be a combination of the PartitionKey (used for scaling) and one or more columns.

```sql
CREATE TABLE IF NOT EXISTS meter_readings (
    meter_id text,
    measure_time timestamp,
    consumption double,
    label text,
    PRIMARY KEY (meter_id, measure_time)
) WITH CLUSTERING ORDER BY (measure_time DESC);
```


### Upserting data


```SQL

```

# High availability

Multiple nodes are set up in a cluster, and are considered peers (instead of master/slave).  The cluster protocol is gossip.