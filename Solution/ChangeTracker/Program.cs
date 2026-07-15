using ChangeTracker.BaseServices;
using ChangeTracker.Jobs;
using ChangeTracker.Services;
using ChangeTracker.Services.PollingServices;
using ChangeTracker.Services.TransformerServices;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared;
using Shared.ConfigurateObjects;
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

builder.Services.AddSingleton<MessagePubliserService>();

builder.Services.AddHostedService<CustomerPollingBackgroundJob>();
builder.Services.AddHostedService<ItemPollingBackgroundJob>();

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton(sp =>
{
    var kafkaSettings = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;

    var producerConfig = new ProducerConfig
    {
        BootstrapServers = kafkaSettings.BootstrapServers,
        MessageMaxBytes = kafkaSettings.MessageMaxBytes,
        Acks = kafkaSettings.Ack == "all" ? Acks.All : Acks.Leader,
        EnableIdempotence = kafkaSettings.EnableIdempotence,
        MessageSendMaxRetries = kafkaSettings.MessageSendMaxRetries,
        MessageTimeoutMs = kafkaSettings.MessageTimeoutMs,
        LingerMs = 500, // Adjust as needed for batching,
        BatchSize = 10 * 1024, // Adjust as needed for batching
    };

    return new ProducerBuilder<string, string>(producerConfig).Build();
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
