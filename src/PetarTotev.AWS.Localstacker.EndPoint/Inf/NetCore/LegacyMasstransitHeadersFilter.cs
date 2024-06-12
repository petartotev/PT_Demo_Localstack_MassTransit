using MassTransit;

namespace PetarTotev.AWS.Localstacker.EndPoint.Inf.NetCore;

public class LegacyMasstransitHeadersFilter<T> : IFilter<PublishContext<T>> where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        if (context.TryGetPayload<RabbitMqSendContext>(out var rabbitMqSendContext))
        {
            context.Headers.Set("Content-Type", "application/vnd.masstransit+json");
        }
    }

    public void Probe(ProbeContext context) { }
}
