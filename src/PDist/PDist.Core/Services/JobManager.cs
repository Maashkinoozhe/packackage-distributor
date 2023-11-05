using PDist.Core.Jobs;
using PDist.Datagram;
using System.Collections.Concurrent;

namespace PDist.Core.Services;

public class JobManager
{
    private BlockingCollection<OutboundPackage> _outChannel;
    private readonly IPayloadProcessorFactory _payloadProcessorFactory;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ConcurrentDictionary<string, JobHandle> _jobs = new();

    public JobManager(IServiceProvider serviceProvider, IPayloadProcessorFactory payloadProcessorFactory)
    {
        _payloadProcessorFactory = payloadProcessorFactory ?? throw new ArgumentNullException(nameof(payloadProcessorFactory));
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void SetOutChannel(BlockingCollection<OutboundPackage> outChannel)
    {
        _outChannel = outChannel ?? throw new ArgumentNullException(nameof(outChannel));
    }

    public void FeedJob(InboundPackage package)
    {
        EnsureJobExists(package);
        if (!_jobs.TryGetValue(package.JobId, out var handle))
        {
            throw new ArgumentException($"Job {package.JobId} does not exist", nameof(package));
        }
        handle.InChannel.Add(package);
    }

    private void EnsureJobExists(InboundPackage package)
    {
        var jobId = package.JobId;
        var jobProcessor = _payloadProcessorFactory.GetProcessor(package.Datagram.PayloadCode);
        try
        {
            OpenJob(jobId, jobProcessor);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public void OpenJob(string jobId, IJobProcessor processor)
    {
        var job = new ThreadedJob();
        var thread = new Thread(job.Start);
        var inChannel = new BlockingCollection<InboundPackage>();
        var jobHandle = new JobHandle(jobId, job, inChannel, thread);

        if (!_jobs.TryAdd(jobId, jobHandle))
        {
            throw new ArgumentException($"Job {jobId} already exists", nameof(jobId));
        }

        job.Setup(jobId, inChannel, _outChannel, _cancellationTokenSource.Token, processor);

        thread.Start();
    }

    public void CloseJob(string jobId)
    {
        if (!_jobs.TryGetValue(jobId, out var handle))
        {
            throw new ArgumentException("Job does not exist", nameof(jobId));
        }
        handle.InChannel.CompleteAdding();
        handle.Thread.Join();

        _jobs.TryRemove(jobId, out _);
    }

    private record JobHandle(string Id, IJob Job, BlockingCollection<InboundPackage> InChannel, Thread Thread);
}