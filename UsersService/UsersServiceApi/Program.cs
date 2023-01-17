using GreenPipes;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using UsersService.Application.Consumers;
using UsersService.Application.DataBase;
using UsersService.Domain.Models;
using UsersService.Infrastructure.Consumers;
using UsersService.Infrastructure.DataBase;
using UsersService.Infrastructure.DataBase.Context;

var builder = WebApplication.CreateBuilder(args);

var rabbitConnectionString = Environment.GetEnvironmentVariable("RabbitConnection") ?? builder.Configuration.GetConnectionString("RabbitConnection");
var dbConnectionString = Environment.GetEnvironmentVariable("UsersDbConnection") ?? builder.Configuration.GetConnectionString("UsersDbConnection");

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUsersRepository, UsersRepository>();

builder.Services.AddDbContext<UsersContext>(opt =>
    opt.UseSqlServer(dbConnectionString, b => b.MigrationsAssembly("UsersService.Infrastructure.DataBase")));

builder.Services.AddHealthChecks().AddSqlServer(dbConnectionString);

builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();
    cfg.AddDelayedMessageScheduler();

    cfg.AddConsumer<SetWaitingStatusConsumer>();
    cfg.AddConsumer<CancelSetWaitingStatusConsumer>();
    cfg.AddConsumer<StartTripConsumer>();
    cfg.AddConsumer<CancelStartTripConsumer>();
    cfg.AddConsumer<SetFreeStatusConsumer>();
    cfg.AddConsumer<CancelSetFreeStatusConsumer>();

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
var dbContext = serviceProvider.GetRequiredService<UsersContext>();
dbContext.Database.EnsureDeleted();
dbContext.Database.EnsureCreated();

var user = new User()
{
    Id = Guid.Parse("f1b6756c-c8e1-417e-8b87-f36b6b528a92"),
    Name = "Artem",
    Surname = "Hatsko",
    Email = "g.artema31@gmail.com",
    PhoneNumber = "+375447940007",
    Coordinates = new Coordinates()
    {
        Latitude = 11,
        Longitude = 11
    },
    Status = new Status()
    {
        Name = "Free"
    }
};
dbContext.Users.Add(user);
dbContext.SaveChanges();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseStatusCodePages();

//    app.UseSwagger();
//    app.UseSwaggerUI();
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.Run();
