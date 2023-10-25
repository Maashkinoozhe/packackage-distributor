namespace PDist.Database.Models;

public record Node : DataItem
{
    public string Name { get; set; }
    public string Address { get; set; }
    public DateTimeOffset LastSeen { get; set; }
    public NodeFeatures Features { get; set; }
}