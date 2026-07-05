using Microsoft.EntityFrameworkCore;

namespace Shared.DbContexts;

public static class DbContextExtensions
{
    public static string GetTableName<T>(this CustomerDbContext context)
        where T : class
    {
        var entityType = context.Model.FindEntityType(typeof(T));
        return entityType?.GetTableName();
    }
}
