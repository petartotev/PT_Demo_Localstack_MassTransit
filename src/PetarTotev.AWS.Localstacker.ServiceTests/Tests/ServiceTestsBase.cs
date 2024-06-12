using Amazon.SQS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using PetarTotev.AWS.Localstacker.EndPoint.Configuration;
using PetarTotev.AWS.Localstacker.ServiceTests.Helpers;

namespace PetarTotev.AWS.Localstacker.ServiceTests.Tests;

public class ServiceTestsBase
{
    protected WebApplication App { get; set; }
    public AmazonSQSClient SqsClient { get; set; }
    public SqsHelper SqsHelperMayamunka { get; set; }

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "ServiceTests");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            //EnvironmentName = "ServiceTests"
        });

        builder.Services.AddLocalStackerMessageHandlers(builder.Configuration);

        App = builder.Build();
        await App.StartAsync().ConfigureAwait(true);

        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", builder.Configuration.GetValue<string>("AwsAccessKey"));
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", builder.Configuration.GetValue<string>("AwsSecretKey"));

        var awsServiceUrl = builder.Configuration.GetValue<string>("AwsServiceUrl");
        SqsClient = new AmazonSQSClient(new AmazonSQSConfig { ServiceURL = awsServiceUrl });

        var awsQueueName = builder.Configuration.GetValue<string>("AwsSqsQueueMayamunka");
        // FAILS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // OneTimeSetUp: Amazon.SQS.AmazonSQSException : exception while calling sqs with unknown operation: Operation detection failed. Missing Action in request for query-protocol service ServiceModel(sqs).
        // ----> Amazon.Runtime.Internal.HttpErrorResponseException : Exception of type 'Amazon.Runtime.Internal.HttpErrorResponseException' was thrown.
        var getQueueUrlResponse = await SqsClient.GetQueueUrlAsync(awsQueueName);
        var queueUrl = getQueueUrlResponse.QueueUrl;
        SqsHelperMayamunka = new SqsHelper(SqsClient, queueUrl);

        await SqsHelperMayamunka.SendMessageAsync("test");
        await SqsHelperMayamunka.PurgeQueueAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await App.StopAsync();
        await App.DisposeAsync();
        SqsClient.Dispose();
    }
}
