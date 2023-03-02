using Contracts.Shared.FinishTheTransaction;
using Contracts.Shared.OrderCarTransaction;
using Contracts.Shared.StartTripTransaction;
using GreenPipes;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var rabbitConnection = Environment.GetEnvironmentVariable("RabbitConnection") ?? builder.Configuration.GetConnectionString("RabbitConnection");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();
    //cfg.AddDelayedMessageScheduler();
    cfg.AddServiceBusMessageScheduler();

    cfg.UsingAzureServiceBus((brc, rbfc) =>
    {
        //rbfc.UseInMemoryOutbox();
        rbfc.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        });
        //rbfc.UseDelayedMessageScheduler();

        rbfc.UseServiceBusMessageScheduler();
        rbfc.Host(rabbitConnection);

        rbfc.Send<OrderCarRequest>(s => s.UseSessionIdFormatter(c => c.Message.Id.ToString("D")));
        rbfc.Send<ProcessCarRequest>(s => s.UseSessionIdFormatter(c => c.Message.Id.ToString("D")));
        rbfc.Send<FinishCarRequest>(s => s.UseSessionIdFormatter(c => c.Message.Id.ToString("D")));

        rbfc.ConfigureEndpoints(brc);
    });

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
});//.AddMassTransitHostedService();


var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseStatusCodePages();
}



app.MapControllers();

app.Run();
