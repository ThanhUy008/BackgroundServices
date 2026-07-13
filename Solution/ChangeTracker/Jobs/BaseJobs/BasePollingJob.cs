using System.Runtime.CompilerServices;
using ChangeTracker.BaseServices;
using ChangeTracker.Services;
using ChangeTracker.Services.BaseServices;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Entities;
using Shared.Helpers;
using Shared.Messages;

namespace ChangeTracker.Jobs.BaseJobs;

public abstract class BasePollingJob<TEntity, TMessage> : BackgroundService
    where TEntity : ITrackable
    where TMessage : class
{
    private readonly ILogger<BasePollingJob<TEntity, TMessage>> _logger;
    private readonly BasePollingService<TEntity> _pollingService;
    private readonly BaseTransformService<TEntity, TMessage> _transformService;

    private static SemaphoreSlim semaphore = new SemaphoreSlim(2, 2);

    public BasePollingJob(
        ILogger<BasePollingJob<TEntity, TMessage>> logger,
        BasePollingService<TEntity> pollingService,
        BaseTransformService<TEntity, TMessage> transformService
    )
    {
        _logger = logger;
        _pollingService = pollingService;
        _transformService = transformService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Polling start");

        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Polling loop start");

            try
            {
                await foreach (
                    var (messages, latestUpdatedOn) in FetchAndTransform(cancellationToken)
                )
                {
                    //This lock the enumarable in a single job.
                    // Meaning Customer job is blocked within itself, not customer job can run all enumarable and Item is blocked.

                    await semaphore.WaitAsync(cancellationToken);

                    if (messages.Count == 0)
                    {
                        _logger.LogWarning("No changed entities found");
                        semaphore.Release();

                        break;
                    }

                    foreach (var message in messages)
                    {
                        // _logger.LogInformation(
                        //     $"Processing customer: {message.Name} {message.Description} {message.Items.Length} items"
                        // );
                    }
                    // _logger.LogWarning("Polling loop end");
                    _logger.LogWarning("Generation of message: {0} of", GC.GetGeneration(messages));

                    await _pollingService.UpdateVersionTrackerAsync(
                        latestUpdatedOn,
                        cancellationToken
                    );

                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while looping PollingBackGroundJob");
                semaphore.Release();
            }
            GCLogger.Log(_logger);

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }

        _logger.LogInformation("PollingBackgroundJob stopped completely.");
    }

    public async IAsyncEnumerable<(List<TMessage>, DateTime)> FetchAndTransform(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await foreach (
            var newlyChangedEntities in _pollingService.FetchNewlyChangedEntitiesAsync(
                cancellationToken
            )
        )
        {
            if (newlyChangedEntities.Count == 0)
            {
                _logger.LogInformation("No changed entities found");
                // yield return (new List<TMessage>(), DateTime.MinValue);
                yield break;
            }

            var temp = await _transformService.TransformChangedEntities(
                newlyChangedEntities,
                cancellationToken
            );

            _logger.LogWarning(
                "Fetched and transformed {Count} changed {Type}",
                temp.Count,
                typeof(TMessage).Name
            );
            _logger.LogWarning("Generation of temp: {0}", GC.GetGeneration(temp));
            yield return (temp, newlyChangedEntities.Max(x => x.UpdatedOn));
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cleaning up resources before closing...");
        await base.StopAsync(cancellationToken);
    }
}
