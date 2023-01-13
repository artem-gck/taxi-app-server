using DriversService.Adapters.Consumers;
using DriversService.Adapters.DataBase;
using DriversService.Adapters.DataBase.Context;
using DriversService.Ports.DataBase;
using GreenPipes;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var rabbitConnectionString = Environment.GetEnvironmentVariable("RabbitConnection") ?? builder.Configuration.GetConnectionString("RabbitConnection");
var dbConnectionString = Environment.GetEnvironmentVariable("DriversDbConnection") ?? builder.Configuration.GetConnectionString("DriversDbConnection");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDriversRepository, DriversRepository>();

builder.Services.AddDbContext<DriversContext>(opt =>
    opt.UseSqlServer(dbConnectionString, b => b.MigrationsAssembly("DriversService.Adapters.DataBase")));

builder.Services.AddHealthChecks().AddSqlServer(dbConnectionString);

builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();
    cfg.AddDelayedMessageScheduler();

    cfg.AddConsumer<SetGoesToUserStatusConsumer>();

    cfg.UsingRabbitMq((brc, rbfc) =>
    {
        rbfc.UseInMemoryOutbox();

        rbfc.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        });

        rbfc.UseDelayedMessageScheduler();
        rbfc.Host(rabbitConnectionString, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        rbfc.ConfigureEndpoints(brc);
    });
}).AddMassTransitHostedService();

var serviceProvider = builder.Services.BuildServiceProvider();
var dbContext = serviceProvider.GetRequiredService<DriversContext>();
dbContext.Database.EnsureCreated();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseStatusCodePages();

    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
