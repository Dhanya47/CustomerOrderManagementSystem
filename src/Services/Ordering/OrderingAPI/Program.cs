
﻿using System.Diagnostics;
using System.Linq;
using BuildingBlocks.CQRS;
﻿using Carter;
using Google.Cloud.Spanner.Data;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ordering.API;
using Ordering.Application;
using OrderingAPI;
using OrderingInfrastructure;


var builder = WebApplication.CreateBuilder(args);

// Register Carter
builder.Services.AddCarter();

// Register MediatR — point it to the assembly where your handlers live
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderHandler).Assembly);
});

builder.Services.AddScoped<ICommandHandler<CreateOrderCommand, CreateOrderResult>, CreateOrderHandler>();
builder.Services.AddScoped<IEventPublisher, PubSubEventHandler>();
builder.Services.Configure<PubSubOptions>(
    builder.Configuration.GetSection("PubSub"));

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


builder.Services.AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddAPIServices();
    
app.MapGet("/", () => "Hello World test again!");


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering API V1");
    });
}

// Map Carter modules (e.g. CreateOrderModule)
app.MapCarter();


app.Run();


    