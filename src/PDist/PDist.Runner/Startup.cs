using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PDist.Core.Configuration;
using PDist.Core.Contracts;
using PDist.Core.HostServices;
using PDist.Database;
using PDist.Database.Configuration;
using PDist.Database.Models;

namespace PDist.Runner;

internal class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddSingleton<IRunner, Listener>();
        services.AddSingleton<IRunner, DemoSetupService>();

        Trace.Listeners.Add(new ConsoleTraceListener());

        AddConfigurations(services);

        AddRepositories(services);
    }

    private void AddConfigurations(IServiceCollection services)
    {
        services.AddOptions<ServerOptions>().Configure<IConfiguration>((option, config) => config.GetSection("server").Bind(option));
        services.AddOptions<DbOptions>().Configure<IConfiguration>((option, config) => config.GetSection("server").Bind(option));
    }

    private void AddRepositories(IServiceCollection services)
    {
        services.AddSingleton<IRepository<Node>, Repository<Node>>();
        services.AddSingleton<IRepository<Package>, Repository<Package>>();
        services.AddSingleton<IRepository<PackageRelease>, Repository<PackageRelease>>();
        services.AddSingleton<IRepository<Blob>, Repository<Blob>>();
        services.AddSingleton<IRepository<BlobOccurrence>, Repository<BlobOccurrence>>();
    }
}