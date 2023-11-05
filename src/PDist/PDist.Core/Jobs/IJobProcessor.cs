using PDist.Datagram;

namespace PDist.Core.Jobs;

public interface IJobProcessor
{
    Task<bool> ProcessNewDataAsync(InboundPackage inPackage, Action<OutboundPackage> sendResponse);
    Task<bool> ProcessWakeupAsync(Action<OutboundPackage> sendResponse);
    Task ProcessCompetedAsync(Action<OutboundPackage> sendResponse);
}