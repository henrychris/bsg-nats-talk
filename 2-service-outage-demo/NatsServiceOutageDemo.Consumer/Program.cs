using NatsServiceOutageDemo.Consumer;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Consumer>();

var host = builder.Build();
host.Run();
