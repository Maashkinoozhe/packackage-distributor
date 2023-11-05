namespace PDist.Datagram;

[AttributeUsage(AttributeTargets.Class)]
public class PayloadProcessorAttribute : Attribute
{
    public PayloadProcessorAttribute(short code)
    {
        Code = code;
    }

    public short Code { get; set; }
}