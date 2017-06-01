using System.Data.Entity;

namespace MVC_2b.Models
{
    public class MVCContext : DbContext
    {
        public MVCContext()
            : base("MVCConnection")
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }

        public DbSet<Location> Locations { get; set; }
        public DbSet<ConfRoom> ConfRooms { get; set; }
        public DbSet<Event> Events { get; set; }
    }
}