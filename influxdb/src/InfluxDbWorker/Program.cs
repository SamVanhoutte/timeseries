using InfluxDbWorker;
using Meterdata;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<DataImporter>();
builder.Services.AddSingleton<IMeterfileParser, LondonMeterfileParser>();
builder.Services.AddSingleton<ITimeseriesImporter, InfluxTimeseriesImporter>();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
var host = builder.Build();
host.Run();