using Microsoft.Extensions.Configuration;

namespace PDist.Core.Configuration;

public class ServerOptions
{
    [ConfigurationKeyName("UdpComListenPort")]
    public int UdpComListenPort { get; set; }

    [ConfigurationKeyName("MasterAddress")]
    public string MasterAddress { get; set; }

    [ConfigurationKeyName("NodeId")]
    public int NodeId { get; set; } // TODO: this should be written in the whoAmIService, not here

}