using DotNetBaseQueue.Handler.Sample;
using DotNetBaseQueue.RabbitMQ.Handler;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddWorkerConfiguration(builder.Configuration)
    .AddHandler<ExampleHandler, ExampleMessage>("ExampleQueueConfiguration");

var host = builder.Build();
host.Run();
