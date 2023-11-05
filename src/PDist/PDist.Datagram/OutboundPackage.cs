using System.Net;

namespace PDist.Datagram;

public record OutboundPackage(UdpDatagram Datagram, IPEndPoint Target);