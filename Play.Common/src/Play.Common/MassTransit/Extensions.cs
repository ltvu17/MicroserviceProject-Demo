using System.Reflection;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Play.Common.Settings;

namespace Play.Common.MassTransit
{
    public static class Extensions
    {
        public static WebApplicationBuilder AddMassTransitWithRabbitMQ(this WebApplicationBuilder builder)
        {
            builder.Services.AddMassTransit(config =>
        {
            config.AddConsumers(Assembly.GetEntryAssembly());
            config.UsingRabbitMq((context , configutator)=>
            {
                var rabbitMQSettings = builder.Configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                configutator.Host(rabbitMQSettings.Host);
                configutator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(
                    builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>().ServiceName, false
                ));
                configutator.UseMessageRetry(retryConfig =>{
                    retryConfig.Interval(3, TimeSpan.FromSeconds(5));
                });
            });
            });
            return builder;
        }
    }
}