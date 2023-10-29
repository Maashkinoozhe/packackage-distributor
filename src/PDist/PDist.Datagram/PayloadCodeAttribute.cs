namespace PDist.Datagram;

public class PayloadCodeAttribute : Attribute
{
    public PayloadCodeAttribute(short code)
    {
        Code = code;
    }

    public short Code { get; set; }
}