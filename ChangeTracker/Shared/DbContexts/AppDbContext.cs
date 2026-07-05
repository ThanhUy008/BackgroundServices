using Microsoft.EntityFrameworkCore;
using Shared.VersionTrackerEntities;

namespace Shared.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {

    }

    public override int SaveChanges()
    {
        ApplyTrackingData();
        return base.SaveChanges();
    }


    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyTrackingData();
        return base.SaveChangesAsync(cancellationToken);
    }


    private void ApplyTrackingData()
    {
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            if(entry is ITrackable trackedEntity)
            {
                if(entry.State == EntityState.Added){
                    trackedEntity.CreatedOn = DateTime.UtcNow;
                    trackedEntity.UpdatedOn = DateTime.UtcNow;
                }
                if(entry.State == EntityState.Modified){
                    trackedEntity.UpdatedOn = DateTime.UtcNow;
                }
            }
        }
    }


    public DbSet<VersionTracker> VersionTrackers {get;set;}
}