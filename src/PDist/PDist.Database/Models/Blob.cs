namespace PDist.Database.Models;

public record Blob : DataItem
{
    public string Path { get; set; }
    public List<string> Tags { get; set; }
    public string Checksum { get; set; }
}