namespace PDist.Datagram;

public interface IRequestSerializer
{
    byte[] Serialize<T>(T request);
    T Deserialize<T>(byte[] data);
    object Deserialize(byte[] data);
}