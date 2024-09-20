using FinalNatsDemo.Common.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;

namespace FinalNatsDemo.Orders.Configuration
{
    public static class DatabaseConfiguration
    {
        public static void SetupDatabase<T>(this IServiceCollection services)
            where T : DbContext
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine($"Current Environment: {env}");
            string connectionString;

            if (env == Environments.Development || env == Environments.Production || env == Environments.Staging)
            {
                var dbSettings = services.BuildServiceProvider().GetService<IOptionsSnapshot<DatabaseSettings>>()?.Value;

                connectionString = dbSettings!.ConnectionString!;
                Console.WriteLine("Database registered.");
            }
            else
            {
                // when running integration tests don't add a database.
                return;
            }

            services.AddDbContext<T>(options =>
            {
                options.UseNpgsql(
                    connectionString,
                    o =>
                    {
                        o.MigrationsHistoryTable(tableName: HistoryRepository.DefaultTableName, typeof(T).Name);
                        o.MigrationsAssembly("FinalNatsDemo.Orders");
                        o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                    }
                );
            });
        }

        public static async Task ApplyMigrationsAsync<T>(this IHost host)
            where T : DbContext
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            using var scope = host.Services.CreateScope();
            if (environment == Environments.Staging || environment == Environments.Development)
            {
                Console.WriteLine("Applying migrations...");
                var context = scope.ServiceProvider.GetRequiredService<T>();
                await context.Database.MigrateAsync();
                Console.WriteLine("Complete!");
            }
        }
    }
}
