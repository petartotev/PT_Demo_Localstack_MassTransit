using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PetarTotev.AWS.Localstacker.EndPoint.Exceptions;

[Serializable]
public class InvalidRabbitMqMessageException : Exception
{
    private string MessageId { get; set; }

    public InvalidRabbitMqMessageException() { }
    public InvalidRabbitMqMessageException(string message) : base(message) { }
    public InvalidRabbitMqMessageException(string message, Exception inner) : base(message, inner) { }
    public InvalidRabbitMqMessageException(string message, string messageId)
        : this(message)
    {
        this.MessageId = messageId;
    }

    public InvalidRabbitMqMessageException(string message, string messageId, Exception inner)
        : this(message, inner)
    {
        this.MessageId = messageId;
    }

    protected InvalidRabbitMqMessageException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        MessageId = info.GetString(nameof(MessageId));
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));

        info.AddValue(nameof(MessageId), MessageId);
        base.GetObjectData(info, context);
    }
}
