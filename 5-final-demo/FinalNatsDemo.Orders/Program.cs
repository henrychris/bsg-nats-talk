using System.Reflection;
using FinalNatsDemo.Common.Nats;
using FinalNatsDemo.Orders.Configuration;
using FinalNatsDemo.Orders.Data;
using FinalNatsDemo.Orders.EventHandlers;
using FluentValidation;
using NATS.Client.Core;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;

builder
    .Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

builder.Services.SetupConfigFiles();

// setup database
builder.Services.SetupDatabase<OrderDataContext>();
builder.Services.SetupControllers();
builder.Services.AddSwagger();
builder.Services.SetupFilters();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// setup identity
// setup authentication
builder.Services.RegisterServices();
builder.Services.SetupJsonOptions();

builder.Services.AddSingleton(sp =>
{
    var opts = new NatsOpts { Url = NatsConfig.DefaultUrl };
    return new NatsConnection(opts);
});
builder.Services.AddSingleton<INatsWrapper, NatsWrapper>();

builder.Services.AddHostedService<ProductCreatedHandler>();
builder.Services.AddHostedService<ProductUpdatedHandler>();

var app = builder.Build();
app.RegisterSwagger();
app.RegisterMiddleware();

// seed db here if needed
await app.ApplyMigrationsAsync<OrderDataContext>();
app.Run();

// this is here for integration tests
public partial class Program;

// todo: publish order.created (handle duplicates)
// todo: handle order.created - reduce inventory of item & create shipping record for order
// todo: complete slides :)
