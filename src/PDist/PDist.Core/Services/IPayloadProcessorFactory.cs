using PDist.Core.Jobs;

namespace PDist.Core.Services;

public interface IPayloadProcessorFactory
{
    IJobProcessor GetProcessor(short payloadCode);
    Type GetProcessorType(short payloadCode);
}