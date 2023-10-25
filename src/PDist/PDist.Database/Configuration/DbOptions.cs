using Microsoft.Extensions.Configuration;

namespace PDist.Database.Configuration;

public class DbOptions
{
    [ConfigurationKeyName("DbLocation")]
    public string DbLocation { get; set; }
}