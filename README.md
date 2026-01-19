# DotNetBaseQueue

This library aims to facilitate the use of asynchronous processes in .NET projects, with an initial focus entirely on RabbitMQ.

## Installation

### Message Publishing

```shell
dotnet add package DotNetBaseQueue.RabbitMQ.Publish
```

### Message Consumption by Handler


```shell
dotnet add package DotNetBaseQueue.RabbitMQ.Handler
```

## Sending Messages

To send messages, add the following to your services:

```c#
builder.Services.AddQueuePublishSingleton(CONFIG_APPSETTINGS);
```

To send a message, use dependency injection to get the IQueuePublish interface and utilize the available method.

Example:

```c#
public class TestController : BaseController
{
    private readonly IQueuePublish _queuePublish;

    public TestController(IQueuePublish queuePublish)
    {
        _queuePublish = queuePublish;
    }

    [HttpPost]
    public async Task<IActionResult> PostEntity(object obj)
    {
        await _queuePublish.PublishAsync(obj, CONFIG_PublishSectionQueue);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> PostEntities(List<object> objects)
    {
        await _queuePublish.PublishList(objects, CONFIG_PublishSectionQueue);

        return Ok();
    }
}
```

## Adding a Consumer

 - The message object must implement the IRabbitEvent interface or IRabbitEventRetry (for using retry).

 - The class for message processing must implement the IRabbitEventHandler interface.

 - Add the consumer to your .NET services using the AddWorkerConfiguration and AddHandler Methods.

Example:

Startup:

```c# 
services
    .AddWorkerConfiguration(configuration, CONFIG_APPSETTINGS)
    .AddHandler<SendMessageHandler, Message>(CONFIG);
```


```c#
public class Message : IQueueEvent
{
    public string SenderId { get; set; }
}

public class SendMessageHandler : IQueueEventHandler<Message>
{
    public async Task HandleAsync(Message command)
    {
        // Handle the message
    }
}
```

## Configuring appsettings.json

You can configure the appsettings.json file as follows:

Base:
```JSON
  "QueueConfiguration": {
    "HostName": "",
    "Port": 5672,
    "UserName": "",
    "Password": "",
    "VirtualHost": "/"
  },
```

Only handler config:
```JSON
  "QueueConfiguration": {
    "ExchangeName": "",
    "ExchangeType": "direct",
    "RoutingKey": "",
    "QueueName": "",
    "NumberOfWorkroles": 1,
    "CreateDeadLetterQueue": true,
    "CreateRetryQueue": true,
    "SecondsToRetry": 15,
    "NumberTryRetry": 3
  },
```

All:
```JSON
  "QueueConfiguration": {
    "HostName": "",
    "Port": 5672,
    "VirtualHost": "/",
    "UserName": "",
    "Password": "",

    "ExchangeName": "",
    "ExchangeType": "direct",
    "RoutingKey": "",
    "QueueName": "",
    "NumberOfWorkroles": 1,
    "CreateDeadLetterQueue": true,
    "CreateRetryQueue": true,
    "SecondsToRetry": 15,
    "NumberTryRetry": 3,
    "RoutingKeys": [ "" ]
  },
```