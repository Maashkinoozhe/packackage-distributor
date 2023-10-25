using PDist.Core.Contracts;
using PDist.Database;
using PDist.Database.Models;

namespace PDist.Core.HostServices;

public class DemoSetupService : IRunner
{
    private readonly IRepository<Node> _nodeRepository;

    public DemoSetupService(IRepository<Node> nodeRepository)
    {
        _nodeRepository = nodeRepository ?? throw new ArgumentNullException(nameof(nodeRepository));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Guid nodeID = Guid.Parse("086DE364-4C12-44DB-ADBD-DB13E7F422BD");
        var node = new Node()
        {
            Id = nodeID,
            Name = "Master",
            Created = DateTimeOffset.UtcNow,
            Services = NodeServices.PassivePackageProvider | NodeServices.PeerConnectionEstablisher,
            Address = "127.0.0.1",
            LastSeen = DateTimeOffset.UtcNow
        };

        var knownNode = await _nodeRepository.GetAsync(node.Id).ConfigureAwait(false);
        if (knownNode is not { })
        {
            await _nodeRepository.CreateAsync(node).ConfigureAwait(false);
        }
        else
        {
            await _nodeRepository.UpdateAsync(node, knownNode).ConfigureAwait(false);
        }
    }
}