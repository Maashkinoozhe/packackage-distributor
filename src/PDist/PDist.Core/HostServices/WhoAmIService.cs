using Newtonsoft.Json;
using PDist.Core.Contracts;
using PDist.Database;
using PDist.Database.Models;
using System.Diagnostics;

namespace PDist.Core.HostServices;

public class WhoAmIService : IRunner
{
    private readonly IRepository<Node> _nodeRepository;
    private const string SelfNodeConfigPath = "./WhoAmI.json";

    public WhoAmIService(IRepository<Node> nodeRepository)
    {
        _nodeRepository = nodeRepository ?? throw new ArgumentNullException(nameof(nodeRepository));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        WhoAmI me;

        if (!File.Exists(SelfNodeConfigPath))
        {
            me = new WhoAmI(
                Guid.NewGuid(),
                Environment.MachineName + "." + Environment.UserName,
                NodeFeatures.PassivePackageProvider
                );

            var meJson = JsonConvert.SerializeObject(me, Formatting.Indented);
            await File.WriteAllTextAsync(SelfNodeConfigPath, meJson, cancellationToken).ConfigureAwait(false);
            Trace.WriteLine($"Created new {nameof(WhoAmI)} {me}");
        }
        else
        {
            var meJson = await File.ReadAllTextAsync(SelfNodeConfigPath, cancellationToken).ConfigureAwait(false);
            me = JsonConvert.DeserializeObject<WhoAmI>(meJson)!;
            Trace.WriteLine($"Loading {nameof(WhoAmI)} {me}");
        }

        var node = new Node()
        {
            Id = me.Id,
            Name = me.Name,
            Features = me.Features,
            Address = "127.0.0.1",
            Created = DateTimeOffset.UtcNow,
            LastSeen = DateTimeOffset.UtcNow
        };

        var knownNode = await _nodeRepository.GetAsync(node.Id).ConfigureAwait(false);
        if (knownNode is not { })
        {
            Trace.WriteLine($"Created self node in db");
            await _nodeRepository.CreateAsync(node).ConfigureAwait(false);
        }
        else
        {
            if (knownNode.Name == node.Name && knownNode.Features == node.Features)
            {
                // only update primary node if the name or services changed
                return;
            }
            Trace.WriteLine($"Update self node in db");
            await _nodeRepository.UpdateAsync(node, knownNode).ConfigureAwait(false);
        }
    }

    private record WhoAmI(Guid Id, string Name, NodeFeatures Features);
}