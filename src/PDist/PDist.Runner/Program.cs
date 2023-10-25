using Microsoft.Extensions.DependencyInjection;
using PDist.Core.Contracts;
using System.Diagnostics;

namespace PDist.Runner;

internal class Program
{
    private static readonly CancellationTokenSource CancellationTokenSource = new();
    
    static async Task Main(string[] args)
    {
        Trace.WriteLine("Starting PDist");

        ServiceCollection serviceCollection = new ServiceCollection();
        new Startup().ConfigureServices(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider(true);

        Console.CancelKeyPress += (_, args) =>
        {
            Trace.WriteLine("Canceling...");
            CancellationTokenSource.Cancel();
            args.Cancel = true;
        };

        var runners = serviceProvider.GetServices<IRunner>();
        var runnerTasks = runners.Select(r => r.RunAsync(CancellationTokenSource.Token)).ToList();

        try
        {
            await Task.WhenAll(runnerTasks).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.Message);
            
            await Task.WhenAll(runnerTasks.Where(r => !r.IsCompleted)).ConfigureAwait(false);
            throw;
        }

        await serviceProvider.DisposeAsync().ConfigureAwait(false);
        Trace.WriteLine("PDist stopped.");
    }
}