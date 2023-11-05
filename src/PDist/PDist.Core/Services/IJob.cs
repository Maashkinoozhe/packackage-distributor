using PDist.Core.Jobs;
using PDist.Datagram;
using System.Collections.Concurrent;

namespace PDist.Core.Services;

public interface IJob
{
    void Setup(string id, BlockingCollection<InboundPackage> input, BlockingCollection<OutboundPackage> output, CancellationToken cancellationToken, IJobProcessor processor);
    void Start();
}