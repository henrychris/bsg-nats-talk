using System.Reflection;
using FinalNatsDemo.Shipping.Configuration;
using FinalNatsDemo.Shipping.Data;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;

builder
    .Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

builder.Services.SetupConfigFiles();

builder.Services.SetupDatabase<ShippingDataContext>();
builder.Services.SetupControllers();
builder.Services.AddSwagger();
builder.Services.SetupFilters();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// setup identity
// setup authentication
builder.Services.RegisterServices();
builder.Services.SetupJsonOptions();

var app = builder.Build();
app.RegisterSwagger();
app.RegisterMiddleware();

await app.ApplyMigrationsAsync<ShippingDataContext>();
app.Run();

// this is here for integration tests
public partial class Program;
