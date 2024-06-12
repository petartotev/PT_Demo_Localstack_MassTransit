namespace PetarTotev.AWS.Localstacker.EndPoint.Inf.NetCore;

public class MessageBusOptions
{
    public string RabbitMqHost { get; set; }
    public string RabbitMqUserName { get; set; }
    public string RabbitMqPassword { get; set; }
    public bool RabbitMqUseSsl { get; set; }
}
