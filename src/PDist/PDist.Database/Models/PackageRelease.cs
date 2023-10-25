using System.Collections.ObjectModel;

namespace PDist.Database.Models;

public record PackageRelease : DataItem
{
    public Guid PackageId { get; set; }

    public Version Version { get; set; }

    Collection<Blob> Blobs { get; set; }
}