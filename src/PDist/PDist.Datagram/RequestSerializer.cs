using System.Text;
using Newtonsoft.Json;

namespace PDist.Datagram;

public class RequestSerializer : IRequestSerializer
{
    public byte[] Serialize<T>(T request)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request, typeof(T), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }));
    }

    public T Deserialize<T>(byte[] data)
    {
        return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data))!;
    }

    public object Deserialize(byte[] data)
    {
        return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data))!;
    }
}