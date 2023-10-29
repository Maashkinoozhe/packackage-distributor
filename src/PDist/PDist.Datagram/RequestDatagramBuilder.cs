using System.Net;

namespace PDist.Datagram;

public class RequestDatagramBuilder
{
    private IPEndPoint _endpoint;
    private object _request;

    public void SetReturnEndpoint(IPEndPoint endpoint)
    {
        _endpoint = endpoint;
    }

    public void SetRequest<T>(T request)
    {
        _request = request;
    }

    public IEnumerable<UdpDatagram> GetUdpDatagram()
    {
        short payloadCode = _request.GetPayloadCode();
        var payload = new RequestSerializer().Serialize(_request);
        var totalBytes = payload.Length;
        var bytesPacked = 0;

        var jobId = new Random((int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).Next();

        for (int i = 0; bytesPacked < totalBytes; i++)
        {
            int payloadStart = bytesPacked + 1;
            int payloadEnd = payloadStart + Math.Min(totalBytes - bytesPacked, 1300);

            yield return UdpDatagram.Create(
                DatagramType.Request,
                jobId,
                i,
                _endpoint,
                payloadCode,
                new Span<byte>(payload, payloadStart, payloadEnd)
            );
            bytesPacked = payloadEnd;
        }
    }
}