using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLibraryInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connStr = config.GetConnectionString("LibraryDb")
                     ?? throw new InvalidOperationException("Missing connection string 'LibraryDb'.");

        services.AddDbContext<LibraryDbContext>(opt =>
        {
            opt.UseSqlServer(connStr);
        });

        return services;
    }
}
