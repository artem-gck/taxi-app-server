using GreenPipes;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using OrchestratorService.Saga;
using OrchestratorService.Saga.State;

var builder = WebApplication.CreateBuilder(args);

var rabbitConnection = Environment.GetEnvironmentVariable("RabbitConnection") ?? builder.Configuration.GetConnectionString("RabbitConnection");
var dbConnectionString = Environment.GetEnvironmentVariable("SagaConnection") ?? builder.Configuration.GetConnectionString("SagaConnection");

builder.Services.AddHealthChecks();

builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();
    cfg.AddDelayedMessageScheduler();

    cfg.AddSagaStateMachine<OrderCarSaga, OrderCarSagaState>()
       .MongoDbRepository(r =>
       {
           r.Connection = dbConnectionString;
           r.DatabaseName = "orderDb";
           r.CollectionName = "orders";
       });

    cfg.AddSagaStateMachine<ProcessCarSaga, ProcessCarSagaState>()
       .MongoDbRepository(r =>
       {
           r.Connection = dbConnectionString;
           r.DatabaseName = "orderDb";
           r.CollectionName = "processes";
       });

    cfg.AddSagaStateMachine<FinishCarSaga, FinishCarSagaState>()
       .MongoDbRepository(r =>
       {
           r.Connection = dbConnectionString;
           r.DatabaseName = "orderDb";
           r.CollectionName = "finish";
       });

    cfg.UsingRabbitMq((brc, rbfc) =>
    {
        rbfc.UseInMemoryOutbox();
        rbfc.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        });
        rbfc.UseDelayedMessageScheduler();
        rbfc.Host(rabbitConnection, h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        rbfc.ConfigureEndpoints(brc);
    });
}).AddMassTransitHostedService();

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
