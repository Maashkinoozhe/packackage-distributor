using FluentAssertions;
using PDist.Core.Jobs;
using PDist.Core.Services;
using PDist.Datagram;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Xunit.Abstractions;

namespace PDist.Core.Tests;

public class ThreadedJobTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ThreadedJobTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CallAndRespond()
    {
        var jobId = "ab1";
        IJob job = new ThreadedJob();

        var input = new BlockingCollection<InboundPackage>();
        var output = new BlockingCollection<OutboundPackage>();
        job.Setup(jobId, input, output, new CancellationToken(), new TestJobProcessor());
        var thread = new Thread(() => job.Start())
        {
            Name = $"Job: {jobId}"
        };
        thread.Start();

        var sourceEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 6900);
        var returnEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 6969);
        var udpDatagram = UdpDatagram.Create(DatagramType.Request, 321, 123, 0, 1, Encoding.UTF8.GetBytes("Hello World"));
        var inPackage = new InboundPackage(udpDatagram, sourceEndPoint, sourceEndPoint);
        input.Add(inPackage);

        output.TryTake(out var result, 10000).Should().BeTrue();
        result.Datagram.PayloadCode.Should().Be(69);

        input.CompleteAdding();
        thread.Join();
    }

    [PayloadProcessor(-1)]
    private class TestJobProcessor : IJobProcessor
    {
        public static ITestOutputHelper? TestOutputHelper;

        public Task<bool> ProcessNewDataAsync(InboundPackage newData, Action<OutboundPackage> sendResponse)
        {
            TestOutputHelper?.WriteLine($"{nameof(ProcessNewDataAsync)}");
            var response = UdpDatagram.Create(DatagramType.Request, 321, 123, 0, 69, Encoding.UTF8.GetBytes("Hello World"));
            sendResponse(new OutboundPackage(response, newData.Host));
            return Task.FromResult(true);
        }

        public Task<bool> ProcessWakeupAsync(Action<OutboundPackage> sendResponse)
        {
            TestOutputHelper?.WriteLine($"{nameof(ProcessWakeupAsync)}");
            return Task.FromResult(true);
        }

        public Task ProcessCompetedAsync(Action<OutboundPackage> sendResponse)
        {
            TestOutputHelper?.WriteLine($"{nameof(ProcessCompetedAsync)}");
            return Task.CompletedTask;
        }
    }
}