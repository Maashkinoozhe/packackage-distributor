using FluentAssertions;
using System.Net;
using Xunit.Abstractions;

namespace PDist.Datagram.Tests;

public class DatagramTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DatagramTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void RoundTripSerialization()
    {
        var payload = new byte[] { 4, 5, 6 };
        var returnEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 12345);

        UdpDatagram data = UdpDatagram.Create(DatagramType.Request, 123456, 0, returnEndPoint, 1, payload.AsSpan());

        var serializedData = data.Serialize();

        _testOutputHelper.WriteLine("Telegram as Base64:\n{0}", Convert.ToBase64String(serializedData));

        UdpDatagram data2 = UdpDatagram.Deserialize(serializedData);

        data.TypeId.Should().Be(data2.TypeId);
        data.JobId.Should().Be(data2.JobId);
        data.ReturnEndPoint.Should().Be(returnEndPoint);
        data2.ReturnEndPoint.Should().Be(returnEndPoint);
        data.Length.Should().Be(data2.Length);
        data.Payload.Should().BeEquivalentTo(data2.Payload);
    }
}