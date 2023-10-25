namespace PDist.Database.Models;

public record Package : DataItem
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}