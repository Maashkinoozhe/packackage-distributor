using Microsoft.Extensions.Configuration;

namespace PDist.Core.Configuration;

public class ServerOptions
{
    [ConfigurationKeyName("UdpComListenPort")]
    public int UdpComListenPort { get; set; }
}