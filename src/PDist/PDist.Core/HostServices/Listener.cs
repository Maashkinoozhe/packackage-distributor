using Microsoft.Extensions.Options;
using PDist.Core.Configuration;
using PDist.Core.Contracts;
using PDist.Core.Services;
using PDist.Database.Models;
using PDist.Datagram;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace PDist.Core.HostServices;

public class Listener : IRunner
{
    private readonly JobManager _jobManager;
    private readonly INetworkService _networkService;
    private readonly ServerOptions _serverOptions;
    private UdpClient? UdpListenClient { get; set; }
    private IPEndPoint? LocalIpEndPoint { get; set; }
    private readonly BlockingCollection<OutboundPackage> _outChannel = new();

    public Listener(IOptionsMonitor<ServerOptions> optionsMonitor, JobManager jobManager, INetworkService networkService)
    {
        this._serverOptions = optionsMonitor.CurrentValue ?? throw new ArgumentNullException(nameof(optionsMonitor) + "." + nameof(optionsMonitor.CurrentValue));

        _jobManager = jobManager ?? throw new ArgumentNullException(nameof(jobManager));
        _jobManager.SetOutChannel(_outChannel);

        _networkService = networkService ?? throw new ArgumentNullException(nameof(networkService));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Thread sender = StartSender(cancellationToken);
        Thread ui = StartUi(cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = GetUdpListenClient();
            try
            {
                var datagram = await client.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                var data = UdpDatagram.Deserialize(datagram.Buffer);
                var matchingLocalIpAddress = _networkService.GetMatchingLocalEndpointForRemoteAddress(datagram.RemoteEndPoint.Address, TimeSpan.FromMinutes(5));
                var matchingLocalIpEndPoint = new IPEndPoint(matchingLocalIpAddress, _serverOptions.UdpComListenPort);
                var package = new InboundPackage(data, datagram.RemoteEndPoint, matchingLocalIpEndPoint);

                _jobManager.FeedJob(package);
                Trace.WriteLine($"Job {package.Datagram.JobId} Received from {datagram.RemoteEndPoint.Address}:{datagram.RemoteEndPoint.Port} ({data.Type})");
            }
            catch (OperationCanceledException ex)
            {
                Trace.TraceError(ex.Message);
            }
        }
        sender.Join();
        ui.Join();
        Trace.WriteLine($"Primary is stopped");
    }

    private Thread StartUi(CancellationToken cancellationToken)
    {
        var ui = new Ui(_serverOptions);
        var thread = new Thread(ui.Run);
        thread.Start();
        return thread;
    }

    private Thread StartSender(CancellationToken cancellationToken)
    {
        async void Start()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var outPackage = _outChannel.Take(cancellationToken);
                Trace.WriteLine($"Job {outPackage.Datagram.JobId} Send to {outPackage.Target} ({outPackage.Datagram.Type})");
                await SendDatagramAsync(outPackage, cancellationToken).ConfigureAwait(false);
            }
        }

        var thread = new Thread(Start);
        thread.Start();
        return thread;
    }

    public async Task SendDatagramAsync(OutboundPackage package, CancellationToken cancellationToken)
    {
        var client = GetUdpListenClient();
        var bytes = package.Datagram.Serialize();
        await client.SendAsync(bytes, bytes.Length, package.Target).ConfigureAwait(false);
    }
    
    private UdpClient? _client { get; set; }

    private UdpClient GetUdpListenClient()
    {
        if (_client == null)
        {
            LocalIpEndPoint = new IPEndPoint(IPAddress.Any, this._serverOptions.UdpComListenPort);
            _client = new UdpClient(LocalIpEndPoint);
        }
        return _client;
    }
}