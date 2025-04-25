using CassandraWorker;
using InfluxDbWorker;
using TimescaleDbWorker;
using Meterdata;
using Meterdata.Importers;
using Meterdata.Parsers;
using Meterdata.Processors;
using Meterdata.Reporting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
// builder.Services.AddSingleton<IMeterfileParser, LondonMeterfileParser>();
builder.Services.AddSingleton<IMeterfileParser, LightWeightMeterfileParser>();

builder.Services.AddSingleton<Reporter>();
//builder.Services.AddSingleton<ITimeseriesImporter, TimescaleDbTimeseriesImporter>();
builder.Services.AddKeyedSingleton<ITimeseriesImporter, NullImporter>("null");
builder.Services.AddKeyedSingleton<ITimeseriesImporter, TimescaleDbTimeseriesImporter>("timescaledb");
builder.Services.AddKeyedSingleton<ITimeseriesImporter, InfluxDbTimeseriesImporter>("influxdb");
builder.Services.AddKeyedSingleton<ITimeseriesImporter, InfluxDbDirectTimeseriesImporter>("influxdbdirect");
builder.Services.AddKeyedSingleton<ITimeseriesImporter, CassandraTimeseriesImporter>("cassandra");


builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
var host = builder.Build();
host.Run();