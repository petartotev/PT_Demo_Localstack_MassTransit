using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace PetarTotev.AWS.Localstacker.ServiceTests.Helpers;

public class SqsHelper
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _sqsQueueUrl;

    public SqsHelper(IAmazonSQS sqsClient, string sqsQueueUrl)
    {
        _sqsClient = sqsClient;
        _sqsQueueUrl = sqsQueueUrl;
    }

    public async Task SendMessageAsync(string message)
    {
        await _sqsClient.SendMessageAsync(_sqsQueueUrl, message);
    }

    public async Task<T> GetNextSqsMessageAsync<T>()
    {
        var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest { QueueUrl = _sqsQueueUrl, WaitTimeSeconds = 5 });
        if (response.Messages.Count == 0)
        {
            return default;
        }

        var message = response.Messages.First();
        await _sqsClient.DeleteMessageAsync(_sqsQueueUrl, message.ReceiptHandle);

        return JsonConvert.DeserializeObject<T>(message.Body);
    }

    public async Task<List<T>> GetSqsMessagesAsync<T>()
    {
        var recieveMessageRequest = new ReceiveMessageRequest
        {
            MaxNumberOfMessages = 10,
            QueueUrl = _sqsQueueUrl,
            WaitTimeSeconds = 5,
        };

        var response = await _sqsClient.ReceiveMessageAsync(recieveMessageRequest);
        if (response.Messages.Count == 0)
        {
            return default;
        }

        var messages = response.Messages;
        var messageBodies = new List<T>();
        foreach (var message in messages)
        {
            await _sqsClient.DeleteMessageAsync(_sqsQueueUrl, message.ReceiptHandle);
            messageBodies.Add(JsonConvert.DeserializeObject<T>(message.Body));
        }

        return messageBodies;
    }

    public async Task PurgeQueueAsync()
    {
        await _sqsClient.PurgeQueueAsync(new PurgeQueueRequest { QueueUrl = _sqsQueueUrl });
    }

    public void Dispose()
    {
        _sqsClient.Dispose();
    }
}
