using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FinalNatsDemo.Orders.Configuration
{
    internal static class ServiceConfiguration
    {
        /// <summary>
        /// Register services in the DI container.
        /// </summary>
        /// <param name="services"></param>
        public static void RegisterServices(this IServiceCollection services)
        {
            // used for time manipulation and testing
            // we should use this instead of DateTime.Now
            services.AddSingleton(TimeProvider.System);
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}
