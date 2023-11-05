using System.Net;

namespace PDist.Datagram;

public record InboundPackage(UdpDatagram Datagram, IPEndPoint Source, IPEndPoint Host)
{
    public string JobId => Datagram.JobId.ToString();
}