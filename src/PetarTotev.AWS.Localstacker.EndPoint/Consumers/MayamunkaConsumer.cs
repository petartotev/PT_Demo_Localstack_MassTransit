using MassTransit;
using PetarTotev.AWS.Localstacker.Contracts.Messages;

namespace PetarTotev.AWS.Localstacker.EndPoint.Consumers;

public class MayamunkaConsumer : IConsumer<MayamunkaMessage>
{
    public async Task Consume(ConsumeContext<MayamunkaMessage> context)
    {
        var message = context.Message;
        await Console.Out.WriteAsync(message.ToString());
        await File.WriteAllTextAsync("./MayamunkaConsumer.txt", message.ToString());
    }
}
