using Microsoft.Extensions.Options;
using PDist.Core.Configuration;
using PDist.Database;
using PDist.Database.Models;
using PDist.Datagram;

namespace PDist.Core.Jobs;

[PayloadCode(10)]
public record GetNodesRequest();

[PayloadCode(10)]
public record GetNodesResponse(Node[] Nodes);

[PayloadProcessor(10)]
public class GetNodesJob : IJobProcessor
{
    private readonly IRepository<Node> _nodesRepository;
    private readonly ServerOptions _serverOptions;

    public GetNodesJob(IRepository<Node> nodesRepository, IOptionsMonitor<ServerOptions> optionsMonitor)
    {
        _nodesRepository = nodesRepository ?? throw new ArgumentNullException(nameof(nodesRepository));
        _serverOptions = optionsMonitor.CurrentValue ?? throw new ArgumentNullException(nameof(optionsMonitor));
    }

    public async Task<bool> ProcessNewDataAsync(InboundPackage inPackage, Action<OutboundPackage> sendResponse)
    {
        List<Node> nodes = new();
        foreach (var node in await _nodesRepository.ListAsync().ConfigureAwait(false))
        {
            nodes.Add(node);
        }

        var response = new GetNodesResponse(nodes.ToArray());
        IEnumerable<UdpDatagram> responseDatagrams = new DatagramBuilder()
            .SetPayload(response)
            .SetSenderNodeId(_serverOptions.NodeId)
            .GetUdpDatagram();

        foreach (var datagram in responseDatagrams)
        {
            sendResponse(new OutboundPackage(datagram, inPackage.Source));
        }

        return true;
    }

    public Task<bool> ProcessWakeupAsync(Action<OutboundPackage> sendResponse)
    {
        throw new NotImplementedException();
    }

    public Task ProcessCompetedAsync(Action<OutboundPackage> sendResponse)
    {
        throw new NotImplementedException();
    }
}