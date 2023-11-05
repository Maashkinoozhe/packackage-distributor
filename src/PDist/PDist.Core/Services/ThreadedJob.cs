using System.Collections.Concurrent;
using PDist.Core.Jobs;
using PDist.Datagram;

namespace PDist.Core.Services;

public class ThreadedJob : IJob
{
    private readonly string _jobId;

    private string Id { get; set; }
    private BlockingCollection<InboundPackage> Input { get; set; }
    private BlockingCollection<OutboundPackage> Output { get; set; }
    private CancellationToken CancellationToken { get; set; }
    private IJobProcessor Processor { get; set; }

    public void Setup(string id, BlockingCollection<InboundPackage> input, BlockingCollection<OutboundPackage> output, CancellationToken cancellationToken, IJobProcessor processor)
    {
        Id = id;
        Input = input;
        Output = output;
        CancellationToken = cancellationToken;
        Processor = processor;
    }

    public async void Start()
    {
        while (!CancellationToken.IsCancellationRequested && !Input.IsCompleted)
        {
            if (Input.TryTake(out var inPackage, 10000, CancellationToken))
            {
                await Processor.ProcessNewDataAsync(inPackage, Respond).ConfigureAwait(false);
            }
            else
            {
                if (!Input.IsAddingCompleted)
                {
                    await Processor.ProcessWakeupAsync(Respond).ConfigureAwait(false);
                }
            }
        }

        if (Input.IsCompleted)
        {
            await Processor.ProcessCompetedAsync(Respond).ConfigureAwait(false);
        }
    }

    private void Respond(OutboundPackage response)
    {
        Output.Add(response, CancellationToken);
    }
}