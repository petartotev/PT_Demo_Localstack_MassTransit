# PT_Demo_Localstack_MassTransit

## Contents
- [Demo Localstack](#demo-localstack)
- [Demo MassTransit](#demo-masstransit)
    - [Contracts](#petartotevawslocalstackercontracts)
    - [EndPoint](#petartotevawslocalstackerendpoint)
    - [ServiceTests](#petartotevawslocalstackerservicetests)
- [Known Issues](#known-issues)
- [Commands](#commands)
    - [Localstack](#commands-localstack)
- [Links](#links)
    - [Localstack](#links-localstack)
    - [MassTransit](#links-masstransit)

## Demo Localstack

0. Let's imagine we have a SQS Queue in AWS named `mayamunka-test-queue`.

1. In the `/src` directory of the solution, create new `/localstack` directory having a single `localstack-init.sh` shell script file:

`./src/localstack/localstack-init.sh`

```
#!/bin/bash
echo 'Starting SQS...'
# SQS
echo 'Starting SQS => Cases'
while ! awslocal sqs list-queues | grep mayamunka-test-queue
do
    awslocal --region=us-east-1 sqs create-queue --queue-name mayamunka-test-queue
done
```

2. In the `/src` directory of the solution, create new `docker-compose.yml` file:

`./src/docker-compose.yml`

```
---
services:
  localstack:
    image: localstack/localstack:1.4.0
    volumes:
      - "localstackdata:/tmp/localstack"
      - "./localstack/localstack-init.sh:/docker-entrypoint-initaws.d/localstack-init.sh"
    ports:
      - '4563-4599:4563-4599'
    environment:
      SERVICES: sqs
      PERSISTENCE: /tmp/localstack/data # needed for persistance
      AWS_ACCESS_KEY_ID: test
      AWS_SECRET_ACCESS_KEY: test
    healthcheck:
      test: "awslocal sqs list-queues"
      interval: 10s
volumes:
  localstackdata:

```

💡 SUGGESTION: `localstack/localstack:1.4.0` seems to be a stable version widely used in DK!

3. Next, run the Localstack Docker container:

```
docker compose up -d
```

4. Check in the logs of the Localstack Docker Container that it is working fine:

```
docker logs src-localstack-1 --follow
```

Output:
```
WARNING
============================================================================
  It seems you are using an init script in /docker-entrypoint-initaws.d.
  The INIT_SCRIPTS_PATH have been deprecated with v1.1.0 and will be removed in future releases
  of LocalStack. Please use /etc/localstack/init/ready.d instead.
  You can suppress this warning by setting LEGACY_INIT_DIR=1.

  See: https://github.com/localstack/localstack/issues/7257
============================================================================

Waiting for all LocalStack services to be ready
2024-06-04 14:25:05,185 CRIT Supervisor is running as root.  Privileges were not dropped because no user is specified in the config file.  If you intend to run as root, you can set user=root in the config file to avoid this message.
2024-06-04 14:25:05,187 INFO supervisord started with pid 16
2024-06-04 14:25:06,189 INFO spawned: 'infra' with pid 21
2024-06-04 14:25:07,191 INFO success: infra entered RUNNING state, process has stayed up for > than 1 seconds (startsecs)
SERVICES variable is ignored if EAGER_SERVICE_LOADING=0.

LocalStack version: 1.4.0
LocalStack build date: 2023-02-13
LocalStack build git hash: dde691c8

2024-06-04T14:25:11.979  WARN --- [-functhread3] hypercorn.error            : ASGI Framework Lifespan error, continuing without Lifespan support
2024-06-04T14:25:11.980  INFO --- [-functhread3] hypercorn.error            : Running on https://0.0.0.0:4566 (CTRL + C to quit)
2024-06-04T14:25:11.985  INFO --- [  MainThread] localstack.utils.bootstrap : Execution of "start_runtime_components" took 1863.88ms
Ready.
/usr/local/bin/docker-entrypoint.sh: running /docker-entrypoint-initaws.d/localstack-init.sh
Starting SQS...
Starting SQS => Cases
2024-06-04T14:25:13.145  INFO --- [   asgi_gw_0] localstack.request.aws     : AWS sqs.CreateQueue => 200
{
    "QueueUrl": "http://localhost:4566/000000000000/mayamunka-test-queue"
}
2024-06-04T14:25:13.605  INFO --- [   asgi_gw_0] localstack.request.aws     : AWS sqs.ListQueues => 200
        "http://localhost:4566/000000000000/mayamunka-test-queue"

2024-06-04T14:25:15.384  INFO --- [   asgi_gw_0] localstack.request.aws     : AWS sqs.ListQueues => 200
```

✅ SUCCESS:
```
CreateQueue => 200
{
    "QueueUrl": "http://localhost:4566/000000000000/mayamunka-test-queue"
}
```

5. Next, you can start using the Localstack CLI:

```
docker ps // Get CONTAINER-ID of IMAGE localstack/localstack:1.4.0
docker exec -it CONTAINER-ID /bin/bash
```

6. Now you can enlist all queues using Localstack CLI:

```
awslocal sqs list-queues
```

Output:
```
{
    "QueueUrls": [
        "http://localhost:4566/000000000000/mayamunka-test-queue"
    ]
}
```

7. You can publish a message using Localstack CLI:

```
root@12a71d60c4db:/opt/code/localstack#
awslocal sqs send-message --queue-url http://localhost:4566/000000000000/mayamunka-test-queue --message-body "Hello World"
```

Output:
```
{
    "MD5OfMessageBody": "b10a8db164e0754105b7a99be72e3fe5",
    "MessageId": "5c2f0eec-ec7d-4aed-96a2-e2ea4b846124"
}
```

8. You can receive the message using Localstack CLI:

```
root@12a71d60c4db:/opt/code/localstack#
awslocal sqs receive-message --queue-url http://localhost:4566/000000000000/mayamunka-test-queue
```

Output:
```
{
    "Messages": [
        {
            "MessageId": "5c2f0eec-ec7d-4aed-96a2-e2ea4b846124",
            "ReceiptHandle": "YTNlZjFjNzQtZjJmMy00MzdjLWI2YzUtNzYzYWYwNjgwNDhhIGFybjphd3M6c3FzOnVzLWVhc3QtMTowMDAwMDAwMDAwMDA6bWF5YW11bmthLXRlc3QtcXVldWUgNWMyZjBlZWMtZWM3ZC00YWVkLTk2YTItZTJlYTRiODQ2MTI0IDE3MTc1MDg4NjguNjk0NTgwMw==",
            "MD5OfBody": "b10a8db164e0754105b7a99be72e3fe5",
            "Body": "Hello World"
        }
    ]
}
```

9. At the end of the Localstack Demo, you can remove the Container and its Volume/s:

```
docker compose down -v
```

## Demo MassTransit

Create new blank Solution `PetarTotev.AWS.Localstacker` which contains the following projects:
- PetarTotev.AWS.Localstacker.Contracts *// .NET Standard 2.0 Class Library*
- PetarTotev.AWS.Localstacker.EndPoint *// .NET 8.0 Web API Project*
- PetarTotev.AWS.Localstacker.ServiceTests *// .NET 8.0 NUnit Project*

### PetarTotev.AWS.Localstacker.Contracts

Introduce the following contract `MayamunkaMessage`:

```
[DataContract]
public class MayamunkaMessage
{
    [DataMember]
    public string FirstName { get; set; }
    [DataMember]
    public string LastName { get; set; }
    [DataMember]
    public string Email { get; set; }
    [DataMember]
    public int Coins { get; set; }

    public override string ToString() { ... }
}
```

### PetarTotev.AWS.Localstacker.EndPoint

1. Install the following NuGet packages:
- MassTransit (8.0.15)
- MassTransit.AmazonSQS (8.0.15)
- Newtonsoft.Json (9.0.1)
- Swashbuckle.AspNetCore (6.4.0) // default

⚠️ WARNING: Note that the combination of these exact versions for `MassTransit` NuGet in `EndPoint` project and `AWSSDK.Core` NuGet in `ServiceTests` project has lead to a successful result.  
If one uses higher versions of the NuGet packages, a series of different errors could follow.  
See section [Known Issues](#known-issues).

2. Introduce class `MayamunkaConsumer` which implements interface `IConsumer` from MassTransit:

```
public class MayamunkaConsumer : IConsumer<MayamunkaMessage>
{
    public async Task Consume(ConsumeContext<MayamunkaMessage> context)
    {
        var messageStringified = context.Message.ToString();
        await Console.Out.WriteAsync(messageStringified);
        await File.WriteAllTextAsync("./MayamunkaConsumer.txt", messageStringified);
    }
}
```

3. Introduce empty interface `ISqsBus` which implements interface `IBus` from MassTransit:

```
// Empty interface in order to register MassTransit for SQS
public interface ISqsBus : IBus { }
```

4. Create static class `LocalstackerServiceExtensions` containing the following extension method which will be invoked in Program.cs's Main method:

```
public static class LocalstackerServiceExtensions
{
    public static IServiceCollection AddLocalStackerMessageHandlers(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddMassTransit<ISqsBus>(busConfig =>
        {
            busConfig.AddConsumer<MayamunkaConsumer>();
            busConfig.UsingAmazonSqs((context, cfg) =>
            {
                // N.B. Make sure us-east-1 is equivalent to the region the SQS Queue belongs to!
                cfg.Host("us-east-1", sqsHostConfig =>
                {
                    sqsHostConfig.AllowTransportHeader(h => false);

                    // N.B. EndPoint running on Prod => AwsAccessKey and AwsSecretKey from appsettings.json should be VALID!!!
                    // N.B. EndPoint running locally => AwsAccessKey and AwsSecretKey from appsettings.Development.json:
                    //      launchSettings.json => "environmentVariables": { "ASPNETCORE_ENVIRONMENT": "Development" }
                    // N.B. ServiceTests running     => AwsAccessKey and AwsSecretKey from appsettings.ServiceTests.json:
                    //      ServiceTestsBase.cs => Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "ServiceTests");
                    var awsAccessKey = configuration.GetValue<string>("AwsAccessKey");
                    sqsHostConfig.AccessKey(awsAccessKey);
                    var awsSecretKey = configuration.GetValue<string>("AwsSecretKey");
                    sqsHostConfig.SecretKey(awsSecretKey);

                    // N.B. EndPoint running on Prod => AwsServiceUrl from appsettings.json should be kept NULL!!!
                    // N.B. EndPoint running locally => AwsServiceUrl from appsettings.Development.json:
                    //      launchSettings.json => "environmentVariables": { "ASPNETCORE_ENVIRONMENT": "Development" }
                    // N.B. ServiceTests running     => AwsServiceUrl from appsettings.ServiceTests.json:
                    //      ServiceTestsBase.cs => Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "ServiceTests");
                    var awsServiceUrl = configuration.GetValue<string>("AwsServiceUrl");

                    if (!string.IsNullOrEmpty(awsServiceUrl))
                    {
                        // N.B. Config should be set for Development and ServiceTests purposes only!
                        sqsHostConfig.Config(new AmazonSQSConfig { ServiceURL = awsServiceUrl });
                    }
                });

                cfg.UseRawJsonSerializer(RawSerializerOptions.AnyMessageType, true);

                var queueName = configuration.GetValue<string>("AwsSqsQueueMayamunka");
                cfg.ReceiveEndpoint(queueName, endpointConfigurator =>
                {
                    // prevent sns topic creation or subscription
                    endpointConfigurator.ConfigureConsumeTopology = false;
                    endpointConfigurator.PublishFaults = false;

                    // allows use the sqs dead-letter queue functionality instead of masstransit's
                    // native "_error" queue handling
                    endpointConfigurator.RethrowFaultedMessages();

                    // makes skipped messages be treated the same way as faulted messages
                    // disables masstransit's "_skipped" queue handling
                    endpointConfigurator.ThrowOnSkippedMessages();

                    endpointConfigurator.ConfigureConsumer<MayamunkaConsumer>(context);
                });
            });
        });

        return serviceCollection;
    }
}
```

5. In `appsettings.json`, add the following Aws- related props:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AwsSqsQueueMayamunka": "mayamunka-test-queue",
  // N.B. SHOULD BE KEPT NULL FOR PRODUCTION!!!
  //"AwsServiceUrl": "https://sqs.us-east-1.amazonaws.com/688785123666",
  // N.B. AWS Access Key is created in AWS IAM for User > Security Credentials > Access Keys > [Create access key]
  "AwsAccessKey": "BK1A5URYJZYAL5BMW4EU",
  // N.B. AWS Secret Key is created in AWS IAM for User > Security Credentials > Access Keys > [Create access key]
  "AwsSecretKey": "aI6IfbiPrZZaUIyiS4aD1i2i3i45F3WJJ4Q7+u+0"
}

```

💡 N.B: `appsettings.json` takes priority when the `ASPNETCORE_ENVIRONMENT` variable is set to `Production` or `null`?

💡 N.B: `AwsServiceUrl` will not be used for Prod.

6. In `appsettings.Development.json`, add the following Aws- related props:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AwsServiceUrl": "http://127.0.0.1:4566",
  "AwsAccessKey": "test",
  "AwsSecretKey": "test"
}
```

💡 `appsettings.Development.json` takes priority when the `ASPNETCORE_ENVIRONMENT` variable is set to `Development` (see `launchSettings.json`).

💡 `AwsServiceUrl` will be used only in case of Development and ServiceTests.

💡 `AwsSqsQueueMayamunka` will be taken from `appsettings.json`.

7. In `appsettings.ServiceTests.json`, add the following Aws- related props:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AwsServiceUrl": "http://127.0.0.1:4566",
  "AwsAccessKey": "test",
  "AwsSecretKey": "test"
}
```

💡 `appsettings.ServiceTests.json` takes priority when the `ASPNETCORE_ENVIRONMENT` variable is set to `ServiceTests` (see `OneTimeSetUpAsync` method in `ServiceTestsBase.cs`).

💡 `AwsServiceUrl` will be used only in case of Development and ServiceTests.

💡 `AwsSqsQueueMayamunka` will be taken from `appsettings.json`.

### PetarTotev.AWS.Localstacker.ServiceTests

1. Install the following NuGet packages:
- AWSSDK.Core (3.7.202.10)
- coverlet.collector (6.0.0) // default
- Microsoft.NET.Test.Sdk (17.8.0) // default
- NUnit (3.14.0) // default
- NUnit.Analyzers (3.9.0) // default
- NUnit3TestAdapter (4.5.0) // default

2. Create class `SqsHelper` by copy-pasting what we have in the DK repos.  
What we will be using is the method `SendMessageAsync`.

3. Create class `ServiceTestsBase` which initializes a new instance of SqsHelper.

4. Create class `MayamunkaConsumerTests` which inherits from `ServiceTestsBase`:

```
[TestFixture]
public class MayamunkaConsumerTests : ServiceTestsBase
{
    [Test]
    public async Task Consume_WithValidInput_ReturnsAsync()
    {
        // Arrange
        var message = new MayamunkaMessage
        {
            FirstName = "Petar",
            LastName = "Totev",
            Email = "petar@petar.petar",
            Coins = 100
        };

        // Act
        var messageSerialized = JsonConvert.SerializeObject(message);
        await SqsHelperMayamunka.SendMessageAsync(messageSerialized);

        Thread.Sleep(10000);
    }
}
```

## Known Issues

Section is in progress...

- https://github.com/localstack/localstack/issues/9610
- https://github.com/localstack/localstack/issues/8267

```
exception while calling sqs with unknown operation: Operation detection failed. Missing Action in request for query-protocol service ServiceModel(sqs).
```

## Commands

### Commands Localstack

```
docker compose up -d
docker logs src-localstack-1 --follow
docker ps // Get CONTAINER-ID of IMAGE localstack/localstack:1.4.0
docker exec -it CONTAINER-ID /bin/bash
awslocal sqs list-queues
awslocal sqs send-message --queue-url http://localhost:4566/000000000000/mayamunka-test-queue --message-body "Hello World"
awslocal sqs receive-message --queue-url http://localhost:4566/000000000000/mayamunka-test-queue
docker compose down -v
```

## Links

### Links Localstack
- https://hub.docker.com/r/localstack/localstack
- https://docs.localstack.cloud/user-guide/aws/sqs/
- https://www.localstack.cloud/

### Links MassTransit
- https://masstransit.io/
- https://masstransit.io/documentation/configuration/transports/amazon-sqs