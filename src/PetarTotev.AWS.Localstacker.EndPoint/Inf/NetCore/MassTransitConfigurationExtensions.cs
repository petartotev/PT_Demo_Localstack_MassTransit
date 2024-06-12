using MassTransit;
using System.Net.Security;

namespace PetarTotev.AWS.Localstacker.EndPoint.Inf.NetCore;

public static class MassTransitConfigurationExtensions
{
    public static IBusRegistrationConfigurator UsingMessageBusSettings(
        this IBusRegistrationConfigurator busConfigurator,
        Action<IBusRegistrationContext, IBusFactoryConfigurator> configure)
    {
        busConfigurator.AddOptions<MessageBusOptions>().BindConfiguration(string.Empty);
        // Also try to bind to the Secrets Manager credentials record.
        busConfigurator.AddOptions<MessageBusOptions>().BindConfiguration("RabbitMqCredentials");

        var hostName = busConfigurator
            .BuildServiceProvider()
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<MessageBusOptions>>().Value.RabbitMqHost;

        if (hostName?.StartsWith("loopback://") ?? false)
        {
            var endpointUri = new Uri(hostName);
            busConfigurator.UsingInMemory(endpointUri, (context, configurator) => configure(context, configurator));
        }
        else
        {
            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                var settings = context.GetRequiredService<Microsoft.Extensions.Options.IOptions<MessageBusOptions>>().Value;
                OverrideMessageBusOptionsPropertiesWithHardcodedValues(settings);
                configurator.ConfigureRabbitMqHost(settings);
                configurator.UsePublishFilter(typeof(LegacyMasstransitHeadersFilter<>), context);
                configure(context, configurator);
            });
        }
        return busConfigurator;
    }

    private static IRabbitMqBusFactoryConfigurator ConfigureRabbitMqHost(this IRabbitMqBusFactoryConfigurator configurator, MessageBusOptions settings)
    {
        var endpointUri = new Uri(settings.RabbitMqHost);
        configurator.Host(endpointUri, hostConfig =>
        {
            hostConfig.Username(settings.RabbitMqUserName);
            hostConfig.Password(settings.RabbitMqPassword);

            // Without SetClientCertificateRequired(false), it tries to use a client certificate.
            if (settings.RabbitMqUseSsl)
            {
                hostConfig.UseSsl(s => s.AllowPolicyErrors(SslPolicyErrors.RemoteCertificateNameMismatch));
            }
        });
        return configurator;
    }

    /// <summary>
    /// This method is created in order to hardcode the necessary values of MessageBusOptions.
    /// In real life, this somehow gets populated out of the box.
    /// </summary>
    /// <param name="settings"></param>
    private static void OverrideMessageBusOptionsPropertiesWithHardcodedValues(MessageBusOptions settings)
    {
        settings.RabbitMqHost = "amqp://localhost";
        settings.RabbitMqUserName = "rabbit";
        settings.RabbitMqPassword = "Test321!";
        settings.RabbitMqUseSsl = false;
    }
}

