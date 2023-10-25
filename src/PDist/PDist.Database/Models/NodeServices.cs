namespace PDist.Database.Models;

[Flags]
public enum NodeServices
{
    None = 0,
    PassivePackageProvider = 1,
    ActivePackageCache = 2,
    PeerConnectionEstablisher = 4,
}