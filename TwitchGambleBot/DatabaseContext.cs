using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TwitchBot;

//Neue Generieren
//dotnet ef migrations add Name
//dotnet ef database update

public class DatabaseContext : DbContext
{
    string connectionString = "server=sailehd.de;port=3308;database=default;user=default;password=AJDhshmzt463;";
    MySqlServerVersion serverVersion = new MySqlServerVersion(new Version(8, 0, 28));
    
     
    
    public DbSet<Message> Messages { get; set; }
    //public DbSet<Post> Posts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(connectionString, serverVersion)
            // The following three options help with debugging, but should
            // be changed or removed for production.
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }
}

public class Message
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int ID { get; set; }
    public string MessageText { get; set; }
}