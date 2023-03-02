using GreenPipes;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using OrchestratorService.Saga;
using OrchestratorService.Saga.Definition;
using OrchestratorService.Saga.State;

var builder = WebApplication.CreateBuilder(args);

var serviceBusConnection = Environment.GetEnvironmentVariable("ServiceBusConnection") ?? builder.Configuration.GetConnectionString("ServiceBusConnection");
var dbConnectionString = Environment.GetEnvironmentVariable("SagaConnection") ?? builder.Configuration.GetConnectionString("SagaConnection");

builder.Services.AddHealthChecks();

builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();

    cfg.AddServiceBusMessageScheduler();

    cfg.AddSagaStateMachine<OrderCarSaga, OrderCarSagaState, OrderCarSagaDefinition>()
       .MessageSessionRepository();

    cfg.AddSagaStateMachine<ProcessCarSaga, ProcessCarSagaState, ProcessCarSagaDefinition>()
       .MessageSessionRepository();

    cfg.AddSagaStateMachine<FinishCarSaga, FinishCarSagaState, FinishCarSagaDefinition>()
       .MessageSessionRepository();

    cfg.UsingAzureServiceBus((brc, rbfc) =>
    {

        rbfc.ReceiveEndpoint("order-state", ep =>
        {
            ep.RequiresSession = true;
            ep.ConfigureSaga<OrderCarSagaState>(brc);
        });

        rbfc.ReceiveEndpoint("process-state", ep =>
        {
            ep.RequiresSession = true;
            ep.ConfigureSaga<ProcessCarSagaState>(brc);
        });

        rbfc.ReceiveEndpoint("finish-state", ep =>
        {
            ep.RequiresSession = true;
            ep.ConfigureSaga<FinishCarSagaState>(brc);
        });

        rbfc.UseInMemoryOutbox();
        rbfc.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        });

        rbfc.UseServiceBusMessageScheduler();
        
        rbfc.Host(serviceBusConnection);
        rbfc.ConfigureEndpoints(brc);
    });
}).AddMassTransitHostedService();

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
