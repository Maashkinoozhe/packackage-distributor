using Microsoft.Extensions.Options;
using PDist.Core.Configuration;
using PDist.Database;
using PDist.Database.Models;
using PDist.Datagram;
using System.Net;

namespace PDist.Core.Jobs;

[PayloadCode(PayloadTypes.ListPackages)]
public record ListPackagesInit();

[PayloadCode(PayloadTypes.ListPackages)]
public record ListPackagesResponse(Package[] Packages);

[PayloadProcessor(PayloadTypes.ListPackages)]
public class ListPackagesJob : IJobProcessor
{
    private readonly IRepository<Package> _packageRepository;
    private readonly ServerOptions _serverOptions;
    private IPEndPoint _uiEndPoint;

    public ListPackagesJob(
        IOptionsMonitor<ServerOptions> optionsMonitor,
        IRepository<Package> packageRepository
        )
    {
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _serverOptions = optionsMonitor.CurrentValue ?? throw new ArgumentNullException(nameof(optionsMonitor));
    }

    private enum mode
    {
        undefined,
        client,
        server
    }

    private mode Mode { get; set; }

    public async Task<bool> ProcessNewDataAsync(InboundPackage inPackage, Action<OutboundPackage> sendResponse)
    {
        if (inPackage.Datagram.Type == DatagramType.Init)
        {
            // Setting up this Job
            Mode = mode.client;
            _uiEndPoint = inPackage.Source;

            // Send request
            var request = new GetNodesRequest();
            var udpDatagrams = new DatagramBuilder()
                .SetJobId(inPackage.Datagram.JobId)
                .SetPayload(request)
                .SetSenderNodeId(PayloadTypes.GetNodes)
                .GetUdpDatagram();

            var ipEndPoint = IPEndPoint.Parse(_serverOptions.MasterAddress);
            ipEndPoint.Port = _serverOptions.UdpComListenPort;

            sendResponse(new OutboundPackage(udpDatagrams.First(), ipEndPoint));

            return false;
        }

        if (inPackage.Datagram.Type == DatagramType.Request)
        {
            var packages = await _packageRepository.ListAsync().ConfigureAwait(false);
            var udpDatagrams = new DatagramBuilder()
                .SetJobId(inPackage.Datagram.JobId)
                .SetPayload(new ListPackagesResponse(packages.ToArray()))
                .SetSenderNodeId(60)
                .GetUdpDatagram();
            foreach (var udpDatagram in udpDatagrams)
            {
                sendResponse(new OutboundPackage(udpDatagram, _uiEndPoint));
            }
            return true;
        }

        return false;
    }

    public Task<bool> ProcessWakeupAsync(Action<OutboundPackage> sendResponse)
    {
        return Task.FromResult(false);
    }

    public Task ProcessCompetedAsync(Action<OutboundPackage> sendResponse)
    {
        throw new NotImplementedException();
    }
}