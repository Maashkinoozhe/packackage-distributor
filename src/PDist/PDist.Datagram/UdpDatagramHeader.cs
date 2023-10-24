using System.Runtime.InteropServices;

namespace PDist.Datagram;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UdpDatagramHeader
{
    public short TypeId;
    public int SequenceNumber;
    public short Length;
}