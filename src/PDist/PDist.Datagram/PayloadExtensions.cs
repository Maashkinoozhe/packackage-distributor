using System.Reflection;

namespace PDist.Datagram;

public static class PayloadExtensions
{
    public static short GetPayloadCode(this object payload)
    {
        var attribute = payload.GetType().GetCustomAttribute<PayloadCodeAttribute>();
        if (attribute == null)
        {
            throw new ArgumentException("type of payload has not payloadCodeAttribute assigned to it.", nameof(payload));
        }

        return attribute.Code;
    }
}