using Common.Engine.Extensions;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddPlugUtopia();

var app = builder.Build();

app.UsePlugUtopia();

app.Run();