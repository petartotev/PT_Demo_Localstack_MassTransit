using Newtonsoft.Json;
using PetarTotev.AWS.Localstacker.Contracts.Messages;

namespace PetarTotev.AWS.Localstacker.ServiceTests.Tests.Consumers;

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
        var messageSerialized = JsonConvert.SerializeObject(message);

        // Act - ✅ should hit breakpoint in Consume() method of MayamunkaConsumer.cs in EndPoint.
        await SqsHelperMayamunka.SendMessageAsync(messageSerialized);

        Thread.Sleep(10000);
    }
}
