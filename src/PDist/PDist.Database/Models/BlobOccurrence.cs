namespace PDist.Database.Models;

public record BlobOccurrence : DataItem
{
    public Guid NodeId { get; set; }
    public Guid BlobId { get; set; }
    public DateTimeOffset LastSeen { get; set; }
}