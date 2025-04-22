# Findings on InfluxDb

## Setting up InfluxDb in Docker, locally

Based on the official docs: https://docs.influxdata.com/influxdb/v2/install/use-docker-compose/

Running the image (after creating the `.env.influxdb2-admin-*` files with secrets) can be done by executing this

```
docker compose up influxdb2
```

## Administer and manage InfluxDb

Just navigate to http://localhost:8086/ and login with your credentials