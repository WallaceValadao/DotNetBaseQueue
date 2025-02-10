using DotNetBaseQueue.Handler.Sample;
using DotNetBaseQueue.RabbitMQ.Handler;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole();

builder
    .AddWorkerConfiguration(builder.Configuration)
    .AddHandler<ExampleHandler, ExampleMessage>("ExampleQueueConfiguration");

var host = builder.Build();
host.Run();
