using DriversService.Adapters.Consumers;
using DriversService.Adapters.DataBase;
using DriversService.Adapters.DataBase.Context;
using DriversService.Domain.Models;
using DriversService.Ports.DataBase;
using GreenPipes;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
    cfg.AddConsumer<CancelSetGoesToUserStatusConsumer>();
    cfg.AddConsumer<SetOnTheTripStatusConsumer>();
    cfg.AddConsumer<CancelSetOnTheTripStatusConsumer>();

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
dbContext.Database.EnsureDeleted();
dbContext.Database.EnsureCreated();

var driver = new Driver()
{
    Id = Guid.Parse("a5a2add3-4241-4d0f-8736-b156b6f6508d"),
    Name = "Artem",
    Surname = "Hatsko",
    Email = "g.artema31@gmail.com",
    PhoneNumber = "+375447940007",
    IsOnline = true,
    Status = new Status()
    {
        Name = "Free"
    },
    Coordinates = new Coordinates()
    {
        Latitude = 11,
        Longitude = 11
    },
    Raiting = 5,
    CountOfReview = 5,
    Experience = new TimeSpan(100)
};

dbContext.Drivers.Add(driver);
dbContext.SaveChanges();

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
