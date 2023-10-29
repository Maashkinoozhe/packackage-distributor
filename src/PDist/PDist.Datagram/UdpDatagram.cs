using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace PDist.Datagram;

public enum DatagramType
{
    Request,
    Response,
    Push
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UdpDatagram
{
    public short TypeId;
    public int JobId;
    public int SequenceId;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] ReturnEndPointAddress;
    public int ReturnEndPointPort;

    public short PayloadCode;
    public short Length;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1300)]
    public byte[] Payload;

    public DatagramType Type => (DatagramType)TypeId;
    public IPEndPoint ReturnEndPoint => new(new IPAddress(ReturnEndPointAddress), ReturnEndPointPort);

    public static UdpDatagram Create(DatagramType type, int jobId, int sequenceId, IPEndPoint returnEndPoint, short payloadCode, Span<byte> payload)
    {
        if (payload.Length > 1300)
        {
            throw new ArgumentException("Payload too large");
        }

        if (returnEndPoint.AddressFamily != AddressFamily.InterNetwork || returnEndPoint.Address.GetAddressBytes().Length > 4)
        {
            throw new ArgumentException("Only ipv4 Endpoints are supported as of now.", nameof(returnEndPoint));
        }

        UdpDatagram data = new()
        {
            TypeId = (short)type,
            JobId = jobId,
            SequenceId = sequenceId,
            ReturnEndPointAddress = returnEndPoint.Address.GetAddressBytes(),
            ReturnEndPointPort = returnEndPoint.Port,
            PayloadCode = payloadCode,
            Payload = new byte[1300],
            Length = (short)payload.Length
        };

        payload.CopyTo(data.Payload.AsSpan());

        return data;
    }

    public byte[] Serialize()
    {
        int size = Marshal.SizeOf(this);
        byte[] buffer = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(this, ptr, true);
        Marshal.Copy(ptr, buffer, 0, size);
        Marshal.FreeHGlobal(ptr);

        return buffer;
    }

    public static UdpDatagram Deserialize(byte[] data)
    {
        UdpDatagram result = default(UdpDatagram);
        int bodySize = Marshal.SizeOf(result);

        int size = bodySize;

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(data, 0, ptr, size);
        result = (UdpDatagram)Marshal.PtrToStructure(ptr, typeof(UdpDatagram));
        Marshal.FreeHGlobal(ptr);

        return result;
    }
}