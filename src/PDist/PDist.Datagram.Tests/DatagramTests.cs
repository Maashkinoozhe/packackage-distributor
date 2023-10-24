using System.Text;
using FluentAssertions;
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
        UdpDatagram data = UdpDatagram.Create(DatagramType.RequestGetNodeInfo, 0, payload);

        var serializedData = data.Serialize();

        _testOutputHelper.WriteLine("Telegram as Base64:\n{0}",Convert.ToBase64String(serializedData));

        UdpDatagram data2 = UdpDatagram.Deserialize(serializedData);

        data.TypeId.Should().Be(data2.TypeId);
        data.SequenceNumber.Should().Be(data2.SequenceNumber);
        data.Length.Should().Be(data2.Length);
        data.Payload.Should().BeEquivalentTo(data2.Payload);
    }
}