using NatsServiceOutageDemo.Subscriber;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Subscriber>();

var host = builder.Build();
host.Run();
