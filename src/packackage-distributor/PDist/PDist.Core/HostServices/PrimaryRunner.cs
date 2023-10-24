using PDist.Core.Contracts;
using System.Diagnostics;

namespace PDist.Core.HostServices;

public class PrimaryRunner : IRunner
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Trace.WriteLine("Primary started");

        while (!cancellationToken.IsCancellationRequested)
        {
            Trace.WriteLine("Primary waiting for completion");
            await Task.Delay(TimeSpan.FromMilliseconds(300)).ConfigureAwait(false);
        }

        Trace.WriteLine("Primary finished");
    }
}