using QueueGroupDemo.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Subscriber>();
builder.Services.AddHostedService<Subscriber2>();

var host = builder.Build();
host.Run();
