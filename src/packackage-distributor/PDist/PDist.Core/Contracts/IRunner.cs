namespace PDist.Core.Contracts;

public interface IRunner
{
    Task RunAsync(CancellationToken cancellationToken);
}