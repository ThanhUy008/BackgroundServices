using Confluent.Kafka;
using ConsumerDisplayWebApp.Jobs;
using Microsoft.Extensions.Options;
using Shared.ConfigurateObjects;
using Shared.Messages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<KafkaConsumerSettings>(builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton(sp =>
{
    var consumerSettings = sp.GetRequiredService<IOptions<KafkaConsumerSettings>>().Value;

    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = consumerSettings.BootstrapServers,
        GroupId = consumerSettings.ConsumerGroupId,
        AutoOffsetReset =
            consumerSettings.AutoOffsetReset == "Earliest"
                ? AutoOffsetReset.Earliest
                : AutoOffsetReset.Latest,
        EnableAutoCommit = consumerSettings.AutoCommit,
        FetchMaxBytes = consumerSettings.FetchMaxBytes,
        MessageMaxBytes = consumerSettings.MessageMaxBytes,
        MaxPollRecords = consumerSettings.MaxPollRecords,
        EnableAutoOffsetStore = consumerSettings.AutoOffsetStore,
    };

    return new ConsumerBuilder<string, string>(consumerConfig).Build();
});

builder.Services.AddHostedService<BaseConsumerJob<CustomerMessage>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
