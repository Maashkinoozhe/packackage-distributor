using PDist.Core.Configuration;
using PDist.Core.Jobs;
using PDist.Datagram;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PDist.Core.Services;

public class Ui
{
    private readonly ServerOptions _serverOption;

    public Ui(ServerOptions serverOption)
    {
        _serverOption = serverOption ?? throw new ArgumentNullException(nameof(serverOption));
    }

    public async void Run()
    {
        Thread.Sleep(1000);
        while (true)
        {
            Dictionary<string, string> actions = new Dictionary<string, string>()
            {
                { "0", "GetNodes" },
                { "1", "AddPackage" }
            };
            Console.WriteLine("");
            Console.WriteLine("======= PDist UI =======");
            foreach (var action in actions)
            {
                Console.WriteLine($"   {$">{action.Key}<",3} - {action.Value}");
            }
            Console.WriteLine("");
            Console.Write("Select an action please: ");

            string? readLine = Console.ReadLine();

            if (readLine == "0")
            {
                await ListPackagesAsync().ConfigureAwait(false);
            }
            if (readLine == "1")
            {
                await AddPackageAsync().ConfigureAwait(false);
            }
        }
    }

    private async Task ListPackagesAsync()
    {
        var jobId = new Random((int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).Next();
        var udpClient = GetUdpClient();
        var request = new ListPackagesInit();
        var initialtor = UdpDatagram.Create(DatagramType.Init, 0, jobId, 0, 20, BinaryData.FromObjectAsJson(request).ToArray());
        await udpClient.SendAsync(initialtor.Serialize(), new IPEndPoint(IPAddress.Parse("127.0.0.1"), _serverOption.UdpComListenPort)).ConfigureAwait(false);
        var response = await udpClient.ReceiveAsync().ConfigureAwait(false);
        Console.WriteLine(Encoding.UTF8.GetString(UdpDatagram.Deserialize(response.Buffer).Payload));
    }

    private async Task AddPackageAsync()
    {
        throw new NotImplementedException();
    }

    private UdpClient? _client { get; set; }

    private UdpClient GetUdpClient()
    {
        if (_client == null)
        {
            _client = new UdpClient(new IPEndPoint(IPAddress.Any, 6900));
        }
        return _client;
    }
}