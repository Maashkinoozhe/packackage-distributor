using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PDist.Database.Models;

[Flags]
[JsonConverter(typeof(StringEnumConverter))]
public enum NodeFeatures
{
    None = 0,
    PassivePackageProvider = 1,
    ActivePackageCache = 2,
    PeerConnectionEstablisher = 4,
}