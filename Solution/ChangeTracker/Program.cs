using ChangeTracker.Jobs;
using ChangeTracker.Services;
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
builder.Services.AddSingleton<PollingCustomerServices>();
builder.Services.AddSingleton<TransformService>();

builder.Services.AddHostedService<PollingBackgroundJob>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
