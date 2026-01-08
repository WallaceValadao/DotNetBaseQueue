using DotNetBaseQueue.Handler.Sample;
using DotNetBaseQueue.RabbitMQ.Handler;
using Microsoft.Extensions.Logging.Console;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.FormatterName = "json";
});

builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
    options.UseUtcTimestamp = true;
});

builder
    .AddWorkerConfiguration(builder.Configuration)
    .AddHandler<ExampleHandler, ExampleMessage>("ExampleQueueConfiguration");

var host = builder.Build();
host.Run();
