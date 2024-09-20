using FinalNatsDemo.Orders.Configuration;
using FinalNatsDemo.Orders.Data;

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

// setup identity
// setup authentication
builder.Services.RegisterServices();
builder.Services.SetupJsonOptions();

var app = builder.Build();
app.RegisterSwagger();
app.RegisterMiddleware();

// seed db here if needed
await app.ApplyMigrationsAsync<OrderDataContext>();
app.Run();

// this is here for integration tests
public partial class Program;

// todo: add launchSettings for all projects
// todo: add sqlite database with seed for inventory
// todo: add endpoint to create an order using an Item array. an item contains an id relating to a value in Inventory table
// todo: publish order.created (handle duplicates)
// todo: handle order.created - reduce inventory of item & create shipping record for order
// todo: brainstorm other ideas
// todo: complete slides :)
