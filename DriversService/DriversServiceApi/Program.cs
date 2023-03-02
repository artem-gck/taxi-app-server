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

var builder = WebApplication.CreateBuilder(args);

var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnection") ?? builder.Configuration.GetConnectionString("ServiceBusConnection");
var dbConnectionString = Environment.GetEnvironmentVariable("DriversDbConnection") ?? builder.Configuration.GetConnectionString("DriversDbConnection");

builder.Services.AddScoped<IDriversRepository, DriversRepository>();

builder.Services.AddDbContext<DriversContext>(opt =>
    opt.UseSqlServer(dbConnectionString, b => b.MigrationsAssembly("DriversService.Adapters.DataBase")));

builder.Services.AddHealthChecks().AddSqlServer(dbConnectionString);

builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();

    cfg.AddServiceBusMessageScheduler();    

    cfg.AddConsumer<SetGoesToUserStatusConsumer>();
    cfg.AddConsumer<CancelSetGoesToUserStatusConsumer>();
    cfg.AddConsumer<SetOnTheTripStatusConsumer>();
    cfg.AddConsumer<SetFreeStatusConsumer>();

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
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
