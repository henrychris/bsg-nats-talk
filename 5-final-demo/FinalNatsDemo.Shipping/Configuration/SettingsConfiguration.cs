using FinalNatsDemo.Common.Settings;

namespace FinalNatsDemo.Shipping.Configuration
{
    internal static class SettingsConfiguration
    {
        /// <summary>
        /// Configures a settings class with values from the application's configuration.
        /// </summary>
        /// <remarks>
        /// The settings class' name must match the name of the section in the configuration.
        /// </remarks>
        /// <typeparam name="T">The type of the settings class.</typeparam>
        /// <param name="services">The IServiceCollection to add the settings to.</param>
        /// <param name="configuration">The application's configuration.</param>
        private static void ConfigureSettings<T>(IServiceCollection services)
            where T : class, new()
        {
            services.AddOptions<T>().BindConfiguration(typeof(T).Name).ValidateDataAnnotations().ValidateOnStart();
        }

        /// <summary>
        /// Sets up the application's configuration files and binds the settings classes to the configuration.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the settings to.</param>
        public static void SetupConfigFiles(this IServiceCollection services)
        {
            ConfigureSettings<DatabaseSettings>(services);
            Console.WriteLine("Secrets have been bound to classes.");
        }
    }
}
