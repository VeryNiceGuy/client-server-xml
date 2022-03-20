using Microsoft.EntityFrameworkCore;

namespace Server.Models
{
    public class ServerContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Element> Elements { get; set; }
        public DbSet<Duplicate> Duplicates { get; set; }

        private const string connectionString = "Server=DESKTOP-L93LL3S\\SQLEXPRESS;Database=ClientServerXml;Trusted_Connection=True;";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlServer(connectionString);
        }
    }
}
