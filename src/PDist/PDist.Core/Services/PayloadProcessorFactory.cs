using Microsoft.Extensions.DependencyInjection;
using PDist.Core.Jobs;

namespace PDist.Core.Services;

public class PayloadProcessorFactory : IPayloadProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<short, Type> _types;

    public PayloadProcessorFactory(IServiceProvider serviceProvider, Dictionary<short, Type> types)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _types = types ?? throw new ArgumentNullException(nameof(types));
    }

    public IJobProcessor GetProcessor(short payloadCode)
    {
        var processorType = GetProcessorType(payloadCode);
        return _serviceProvider.GetRequiredService(processorType) as IJobProcessor ?? throw new InvalidOperationException($"Type {processorType.Name} is not assignable to {nameof(IJobProcessor)}");
    }

    public Type GetProcessorType(short payloadCode)
    {
        return _types.ContainsKey(payloadCode) ? _types[payloadCode] : throw new ArgumentException($"PayloadCode {payloadCode} cant be resolved to a processor", nameof(payloadCode));
    }
}