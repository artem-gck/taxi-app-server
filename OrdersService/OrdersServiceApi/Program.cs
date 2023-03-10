using GreenPipes;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using OrdersService.Access.DataBase.Context;
using OrdersService.Access.DataBase.Interfaces;
using OrdersService.Access.DataBase.Realisations;
using OrdersService.Services.Consumers;

var builder = WebApplication.CreateBuilder(args);

var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnection") ?? builder.Configuration.GetConnectionString("ServiceBusConnection");
var dbConnectionString = Environment.GetEnvironmentVariable("OrdersDbConnection") ?? builder.Configuration.GetConnectionString("OrdersDbConnection");

builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

builder.Services.AddDbContext<OrdersContext>(opt =>
    opt.UseSqlServer(dbConnectionString, b => b.MigrationsAssembly("OrdersService.Access.DataBase")));

builder.Services.AddHealthChecks().AddSqlServer(dbConnectionString);

builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();
    cfg.AddServiceBusMessageScheduler();

    cfg.AddConsumer<AddNewOrderConsumer>();
    cfg.AddConsumer<SetProcessingStatusToOrderConsumer>();
    cfg.AddConsumer<CancelSetProcessingStatusToOrderConsumer>();
    cfg.AddConsumer<SetFinishStatusToOrderConsumer>();
    cfg.AddConsumer<CancelSetFinishStatusToOrderConsumer>();

    cfg.UsingAzureServiceBus((brc, rbfc) =>
    {
        rbfc.UseInMemoryOutbox();

        rbfc.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        });

        rbfc.UseServiceBusMessageScheduler();
        rbfc.Host(serviceBusConnectionString);

        rbfc.ConfigureEndpoints(brc);
    });
}).AddMassTransitHostedService();

var serviceProvider = builder.Services.BuildServiceProvider();
var dbContext = serviceProvider.GetRequiredService<OrdersContext>();
dbContext.Database.EnsureDeleted();
dbContext.Database.EnsureCreated();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseStatusCodePages();
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
