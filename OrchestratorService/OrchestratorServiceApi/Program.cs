using GreenPipes;
using HealthChecks.UI.Client;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core.Saga;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using OrchestratorService.Saga;
using OrchestratorService.Saga.Definition;
using OrchestratorService.Saga.State;

var builder = WebApplication.CreateBuilder(args);

var rabbitConnection = Environment.GetEnvironmentVariable("RabbitConnection") ?? builder.Configuration.GetConnectionString("RabbitConnection");
var dbConnectionString = Environment.GetEnvironmentVariable("SagaConnection") ?? builder.Configuration.GetConnectionString("SagaConnection");

builder.Services.AddHealthChecks();

builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();
    //cfg.AddDelayedMessageScheduler();

    cfg.AddServiceBusMessageScheduler();

    cfg.AddSagaStateMachine<OrderCarSaga, OrderCarSagaState, OrderCarSagaDefinition>()
       .MessageSessionRepository();
    //.MongoDbRepository(r =>
    //{
    //    r.Connection = dbConnectionString;
    //    r.DatabaseName = "orderDb";
    //    r.CollectionName = "orders";
    //});

    cfg.AddSagaStateMachine<ProcessCarSaga, ProcessCarSagaState, ProcessCarSagaDefinition>()
       .MessageSessionRepository();
    //.MongoDbRepository(r =>
    //{
    //    r.Connection = dbConnectionString;
    //    r.DatabaseName = "orderDb";
    //    r.CollectionName = "processes";
    //});

    cfg.AddSagaStateMachine<FinishCarSaga, FinishCarSagaState, FinishCarSagaDefinition>()
       .MessageSessionRepository();
    //.MongoDbRepository(r =>
    //{
    //    r.Connection = dbConnectionString;
    //    r.DatabaseName = "orderDb";
    //    r.CollectionName = "finish";
    //});

    //cfg.UsingRabbitMq((brc, rbfc) =>
    //{
    //    rbfc.UseInMemoryOutbox();
    //    rbfc.UseMessageRetry(r =>
    //    {
    //        r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    //    });
    //    rbfc.UseDelayedMessageScheduler();
    //    rbfc.Host(rabbitConnection, h =>
    //    {
    //        h.Username("guest");
    //        h.Password("guest");
    //    });
    //    rbfc.ConfigureEndpoints(brc);
    //});

    cfg.UsingAzureServiceBus((brc, rbfc) =>
    {
        //var orderCarSaga = new OrderCarSaga();
        // This gives an Obsolete-warning 
        // var repository = new MessageSessionSagaRepository<OrderState>(); 
        // This is suggested instead
        //var repository = MessageSessionSagaRepository.Create<OrderCarSagaState>();

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
        //rbfc.UseDelayedMessageScheduler();

        rbfc.UseServiceBusMessageScheduler();
        
        rbfc.Host(rabbitConnection);
        rbfc.ConfigureEndpoints(brc);
    });
}).AddMassTransitHostedService();

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
