#!/bin/bash

until cqlsh -e "describe keyspaces" cassandra 9042 >/dev/null 2>&1; do
  echo "Waiting for Cassandra to be ready..."
  sleep 10
done

echo "Create keyspace..."
cqlsh -e "CREATE KEYSPACE IF NOT EXISTS timeseries WITH replication = {'class': 'SimpleStrategy', 'replication_factor': '1'};" cassandra 9042
echo "Done!"