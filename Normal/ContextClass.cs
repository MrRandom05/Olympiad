using Microsoft.EntityFrameworkCore;
using Normal.Models;

namespace Normal
{
    public class ContextClass : DbContext
    {
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AnimalType> AnimalTypes { get; set; }
        public DbSet<AnimalVisitedLocation> AnimalVisitedLocations { get; set; }
        public DbSet<LocationPoint> LocationPoints { get; set; }

        public ContextClass(DbContextOptions<ContextClass> options) : base(options)
        {
            Database.EnsureCreated();
        }

    }
}
