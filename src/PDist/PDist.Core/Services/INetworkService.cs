using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using PDist.Datagram;

namespace PDist.Core.Services;

public interface INetworkService
{
    IPAddress GetMatchingLocalEndpointForRemoteAddress(IPAddress remoteAddress, TimeSpan timeToLive);

    /// <summary>
    /// Lets hope this is not needed.
    /// Creating a Udp Client listening on a port and all interfaces should be enough.
    /// When sending, the client will use a suitable interface to reach the target.
    /// When receiving a datagram, the client will tell us the ip of the sender.
    /// So the ReturnAddress in a <see cref="UdpDatagram"/> is probably also not needed.
    /// </summary>
    IPAddress? GetMyIpEndpoint();
}

public class NetworkService : INetworkService
{
    private record CachedIpAddress(IPAddress Address, DateTimeOffset ValidUntil);

    ConcurrentDictionary<IPAddress, CachedIpAddress> CachedConnections = new();

    public IPAddress GetMatchingLocalEndpointForRemoteAddress(IPAddress remoteAddress, TimeSpan timeToLive)
    {
        if (CachedConnections.TryGetValue(remoteAddress, out var cached))
        {
            if (cached.ValidUntil > DateTimeOffset.UtcNow)
            {
                return cached.Address;
            }
        }

        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        socket.Connect(new IPEndPoint(remoteAddress, 80));
        if (!socket.Connected)
        {
            throw new Exception("Could not connect to remote address. Therefore, no matching local endpoint could be found.");
        }
        var endpoint = socket.LocalEndPoint as IPEndPoint ?? throw new Exception("Local endpoint is not an IPEndPoint. Even though it should.");

        var cachedIpAddress = new CachedIpAddress(endpoint.Address, DateTimeOffset.UtcNow + timeToLive);
        CachedConnections.AddOrUpdate(remoteAddress, cachedIpAddress, (_, _) => cachedIpAddress);

        return endpoint.Address;
    }

    public IPAddress? GetMyIpEndpoint()
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(i => i.Supports(NetworkInterfaceComponent.IPv4))
            .Where(i => i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Where(i => i.OperationalStatus == OperationalStatus.Up)
            .Where(i => i.GetIPProperties().UnicastAddresses.Any(x => x.Address.AddressFamily == AddressFamily.InterNetwork))
            .Where(i => i.GetIPProperties().GatewayAddresses.Any(x => x.Address.AddressFamily == AddressFamily.InterNetwork))
            .ToList();

        return networkInterfaces
            .FirstOrDefault()
            .GetIPProperties().UnicastAddresses
            .FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
            .Address;
    }
}