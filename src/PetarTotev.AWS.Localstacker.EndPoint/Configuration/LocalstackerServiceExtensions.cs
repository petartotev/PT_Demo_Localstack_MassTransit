using Amazon.SQS;
using MassTransit;
using MassTransit.Serialization;
using PetarTotev.AWS.Localstacker.EndPoint.Consumers;
using PetarTotev.AWS.Localstacker.EndPoint.Exceptions;
using PetarTotev.AWS.Localstacker.EndPoint.Inf.NetCore;
using PetarTotev.AWS.Localstacker.EndPoint.Sqs.Interfaces;

namespace PetarTotev.AWS.Localstacker.EndPoint.Configuration;

public static class LocalstackerServiceExtensions
{
    public static IServiceCollection AddLocalStackerMessageHandlers(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        // AWS SQS Queue
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

        // RabbitMQ
        serviceCollection.AddMassTransit(busConfig =>
        {
            busConfig.AddConsumer<PeterRabbitConsumer>();
            busConfig.UsingMessageBusSettings((context, cfg) =>
            {
                cfg.ReceiveEndpoint("localstacker_worker", e =>
                {
                    e.UseMessageRetry(
                        r =>
                        {
                            r.Ignore<InvalidRabbitMqMessageException>();
                            r.Incremental(
                                configuration.GetValue<int>("RabbitRetryAttempts"),
                                configuration.GetValue<TimeSpan>("StartRabbitRetryInterval"),
                                configuration.GetValue<TimeSpan>("RabbitRetryIntervalIncrement"));
                        });
                    e.ConfigureConsumer<PeterRabbitConsumer>(context);
                });
            });
        });

        return serviceCollection;
    }
}
