using MassTransit;
using PetarTotev.AWS.Localstacker.Contracts.Messages;

namespace PetarTotev.AWS.Localstacker.EndPoint.Consumers;

public class PeterRabbitConsumer : IConsumer<PeterRabbitMessage>
{
    public async Task Consume(ConsumeContext<PeterRabbitMessage> context)
    {
        var message = context?.Message;
        await Console.Out.WriteAsync(message?.ToString());
        await File.WriteAllTextAsync("./PeterRabbitConsumer.txt", message?.ToString());
    }
}
