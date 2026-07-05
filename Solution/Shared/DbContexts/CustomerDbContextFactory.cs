
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Shared.DbContexts;

public class CustomerDbContextFactory: IDesignTimeDbContextFactory<CustomerDbContext>
{
    public CustomerDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connStr = configuration.GetConnectionString("CustomerDb");
        
        if (string.IsNullOrWhiteSpace(connStr))
        {
            throw new InvalidOperationException("ConnectionStrings__CustomerDb environment variable is missing.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<CustomerDbContext>();


        optionsBuilder.UseNpgsql(connStr);

        return new CustomerDbContext(optionsBuilder.Options);
    } 
}