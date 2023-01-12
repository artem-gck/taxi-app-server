using GreenPipes;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using UsersService.Application.Consumers;
using UsersService.Application.DataBase;
using UsersService.Infrastructure.DataBase;
using UsersService.Infrastructure.DataBase.Context;

var builder = WebApplication.CreateBuilder(args);

var DbConnectionString = Environment.GetEnvironmentVariable("UsersDbConnection") ?? builder.Configuration.GetConnectionString("UsersDbConnection");

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUsersRepository, UsersRepository>();

builder.Services.AddDbContext<UsersContext>(opt =>
    opt.UseSqlServer(DbConnectionString, b => b.MigrationsAssembly("UsersService.Infrastructure.DataBase")));

builder.Services.AddHealthChecks().AddSqlServer(DbConnectionString);

builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();
    cfg.AddDelayedMessageScheduler();

    cfg.AddConsumer<SetWaitingStatusConsumer>();

    cfg.UsingRabbitMq((brc, rbfc) =>
    {
        rbfc.UseInMemoryOutbox();

        rbfc.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        });

        rbfc.UseDelayedMessageScheduler();
        rbfc.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        rbfc.ConfigureEndpoints(brc);
    });
}).AddMassTransitHostedService();

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
