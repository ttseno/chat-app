using Microsoft.EntityFrameworkCore;

namespace ChatWSServer
{
    public class DatabaseContext : DbContext
    {
        public DbSet<ChatMessage> ChatMessages { get; set; }
        
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

    }
}