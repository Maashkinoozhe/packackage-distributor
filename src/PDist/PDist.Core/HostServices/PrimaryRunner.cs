using Microsoft.Extensions.Options;
using PDist.Core.Configuration;
using PDist.Core.Contracts;
using PDist.Datagram;
using System.Diagnostics;
using System.Net.Sockets;

namespace PDist.Core.HostServices;

public class PrimaryRunner : IRunner
{
    private readonly ServerOptions _serverOptions;
    private UdpClient? UdpListenClient { get; set; }

    public PrimaryRunner(IOptionsMonitor<ServerOptions> optionsMonitor)
    {
        this._serverOptions = optionsMonitor.CurrentValue ?? throw new ArgumentNullException(nameof(optionsMonitor) + "." + nameof(optionsMonitor.CurrentValue));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = GetUdpListenClient();
            try
            {
                var datagram = await client.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                var data = UdpDatagram.Deserialize(datagram.Buffer);

                Trace.WriteLine($"Received from {datagram.RemoteEndPoint.Address}:{datagram.RemoteEndPoint.Port} ({data.Type})");
                Trace.WriteLine($"Primary waiting for completion, Port is {_serverOptions.UdpComListenPort}");
                await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                Trace.TraceError(ex.Message);
            }
        }
        Trace.WriteLine($"Primary is stopped");
    }

    private UdpClient GetUdpListenClient()
    {
        if (UdpListenClient == null)
        {
            UdpListenClient = new UdpClient(this._serverOptions.UdpComListenPort);
        }
        return UdpListenClient;
    }
}