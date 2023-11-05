using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FluentAssertions;
using PDist.Core.Services;
using Xunit.Abstractions;

namespace PDist.Core.Tests;

public class NetworkTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public NetworkTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ShowNetworkAdapters()
    {
        _testOutputHelper.WriteLine(new NetworkService().GetMyIpEndpoint()?.ToString());
    }

    [Fact]
    public void SendToMySelf()
    {
        UdpClient sender = new UdpClient(6960, AddressFamily.InterNetwork);
        sender.Send(Encoding.UTF8.GetBytes("Hello"),new IPEndPoint(IPAddress.Parse("127.0.0.1"),6960));
        IPEndPoint endpoint = null;
        var data = sender.Receive(ref endpoint);
        Encoding.UTF8.GetString(data).Should().Be("Hello");
    }

    [Fact]
    public async Task SendBeforeReceiving()
    {
        ConcurrentBag<string> receiveBag = new ConcurrentBag<string>();
        var receiverPort = 6969;

        UdpClient sender = new UdpClient(6960, AddressFamily.InterNetwork);
        UdpClient receiver = new UdpClient(receiverPort, AddressFamily.InterNetwork);
        receiver.Client.ReceiveBufferSize = 5000000;

        for (int i = 0; i < 100; i++)
        {
            sender.Send(Encoding.UTF8.GetBytes($"Hello World {i:000}"), 15, new IPEndPoint(IPAddress.Parse("127.0.0.1"), receiverPort));
        }

        while (true)
        {
            var token = new CancellationTokenSource(500).Token;
            try
            {
                var result = await receiver.ReceiveAsync(token);
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
                socket.Connect(result.RemoteEndPoint);
                var socketLocalEndPoint = socket.LocalEndPoint as IPEndPoint;
                receiveBag.Add(Encoding.UTF8.GetString(result.Buffer) + $" from {result.RemoteEndPoint} to {socketLocalEndPoint.Address}:{receiverPort}");
            }
            catch (OperationCanceledException e)
            {
                _testOutputHelper.WriteLine(e.Message);
                break;
            }
        }

        foreach (var msg in receiveBag.Reverse())
        {
            _testOutputHelper.WriteLine(msg);
        }

        receiveBag.Count.Should().Be(100);
    }
}