using System.Net;

namespace PDist.Datagram;

public class DatagramBuilder
{
    private int _senderNodeId;
    private object _request;
    private int? _jobId;

    public DatagramBuilder SetSenderNodeId(int sender)
    {
        _senderNodeId = sender;
        return this;
    }

    public DatagramBuilder SetPayload<T>(T request)
    {
        _request = request;
        return this;
    }

    public DatagramBuilder SetJobId(int id)
    {
        _jobId = id;
        return this;
    }

    public IEnumerable<UdpDatagram> GetUdpDatagram()
    {
        short payloadCode = _request.GetPayloadCode();
        var payload = new RequestSerializer().Serialize(_request);
        var totalBytes = payload.Length;
        var bytesPacked = 0;

        var jobId = _jobId ?? new Random((int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).Next();

        for (int i = 0; bytesPacked < totalBytes; i++)
        {
            int payloadStart = bytesPacked;
            int payloadEnd = payloadStart + Math.Min(totalBytes - bytesPacked, 1300);

            yield return UdpDatagram.Create(
                DatagramType.Request,
                _senderNodeId,
                jobId,
                i,
                payloadCode,
                new Span<byte>(payload, payloadStart, payloadEnd)
            );
            bytesPacked = payloadEnd+1;
        }
    }
}