using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace PDist.Datagram;

public enum DatagramType
{
    RequestGetNodeInfo
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UdpDatagram
{
    public short TypeId;
    public int SequenceNumber;
    public short Length;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1300)]
    public byte[] Payload;

    public DatagramType Type => (DatagramType)TypeId;

    public static UdpDatagram Create(DatagramType type , int sequenceNumber, byte[] payload)
    {
        if (payload.Length > 1300)
        {
            throw new ArgumentException("Payload too large");
        }

        UdpDatagram data = new()
        {
            TypeId = (short)type,
            SequenceNumber = sequenceNumber,
            Payload = new byte[1300],
            Length = (short)payload.Length
        };

        Array.Copy(payload, data.Payload, payload.Length);

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
        UdpDatagramHeader header = default(UdpDatagramHeader);
        int headerSize = Marshal.SizeOf(header);
        IntPtr headerPtr = Marshal.AllocHGlobal(headerSize);
        Marshal.Copy(data, 0, headerPtr, headerSize);
        header = (UdpDatagramHeader)Marshal.PtrToStructure(headerPtr, typeof(UdpDatagramHeader));
        Marshal.FreeHGlobal(headerPtr);

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