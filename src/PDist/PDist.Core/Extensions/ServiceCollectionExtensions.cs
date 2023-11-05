using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PDist.Core.Jobs;
using PDist.Core.Services;
using PDist.Datagram;

namespace PDist.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJobProcessors(this IServiceCollection serviceCollection)
    {
        var rootAssembly = Assembly.GetCallingAssembly();
        Assembly[] assemblies = rootAssembly.GetReferencedAssemblies().Select(Assembly.Load).Concat(new[] { rootAssembly }).ToArray();

        var knownTypes = assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsAssignableTo(typeof(IJobProcessor)) && x.GetCustomAttribute<PayloadProcessorAttribute>() != null);

        Dictionary<short, Type> payloadTypes = knownTypes
            .ToDictionary(x => x.GetCustomAttribute<PayloadProcessorAttribute>()!.Code, y => y);

        foreach (var type in payloadTypes.Values)
        {
            serviceCollection.TryAddSingleton(type);
        }

        serviceCollection.TryAddSingleton<IPayloadProcessorFactory>((sp) => new PayloadProcessorFactory(sp, payloadTypes));

        return serviceCollection;
    }
}