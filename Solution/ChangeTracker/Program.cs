using ChangeTracker.Jobs;
using ChangeTracker.Services;
using ChangeTracker.Services.PollingServices;
using ChangeTracker.Services.TransformerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;
using Shared.DbContexts;
using Shared.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"))
);

builder.Services.AddDbContextFactory<CustomerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CustomerDb"))
);

//Doesn't matter anyway, it will be scoped to the background service,
//but we can use AddScoped for PollingCustomerServices
builder.Services.AddSingleton<CustomerPollingService>();
builder.Services.AddSingleton<ItemPollingService>();

builder.Services.AddSingleton<CustomerTransformService>();
builder.Services.AddSingleton<ItemTransformService>();

builder.Services.AddHostedService<CustomerPollingBackgroundJob>();
builder.Services.AddHostedService<ItemPollingBackgroundJob>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
