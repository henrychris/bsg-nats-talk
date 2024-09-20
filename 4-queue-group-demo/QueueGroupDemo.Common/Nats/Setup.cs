using Microsoft.Extensions.DependencyInjection;
using NATS.Client.Core;
using NATS.Client.JetStream;

namespace QueueGroupDemo.Common.Nats
{
    public static class Setup
    {
        public static void AddNats(this IServiceCollection services)
        {
            var natsOpts = new NatsOpts { Url = NatsConfig.DefaultUrl };

            var natsConnection = new NatsConnection(natsOpts);
            services.AddSingleton<INatsConnectionPool>(new NatsConnectionPool(natsOpts));
            services.AddSingleton(new NatsJSContext(natsConnection, new NatsJSOpts(natsOpts)));
            services.AddScoped<INatsWrapper, NatsWrapper>();
        }
    }
}
