using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PDist.Core.Extensions;
using PDist.Core.HostServices;
using PDist.Core.Jobs;
using PDist.Core.Services;
using PDist.Datagram;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Xunit.Abstractions;

namespace PDist.Core.Tests;

public class JobManagerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public JobManagerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CallResponse1Job()
    {
        // arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<TestJobProcessor, TestJobProcessor>()
            .AddJobProcessors()
            .BuildServiceProvider();
        var output = new BlockingCollection<OutboundPackage>();
        var jobManager = new JobManager(serviceProvider, serviceProvider.GetRequiredService<IPayloadProcessorFactory>());
        jobManager.SetOutChannel(output);

        var sourceEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 6900);
        var hostEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 2 }), 6969);
        var udpDatagram = UdpDatagram.Create(DatagramType.Request, 321, 123, 0, 12, Encoding.UTF8.GetBytes("Hello World"));
        var inPackage = new InboundPackage(udpDatagram, sourceEndPoint, hostEndPoint);
        TestJobProcessor.TestOutputHelper = _testOutputHelper;

        //  action
        jobManager.FeedJob(inPackage);

        // assert
        output.TryTake(out var result, 10000).Should().BeTrue();
        result.Datagram.PayloadCode.Should().Be(69);
        result.Target.Should().BeEquivalentTo(hostEndPoint);

        jobManager.CloseJob(inPackage.JobId);
    }

    [PayloadProcessor(12)]
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