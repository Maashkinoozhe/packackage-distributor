namespace PDist.Database.Models;

public abstract record DataItem
{
    public Guid Id { get; set; }
    public DateTimeOffset Created { get; set; }
}