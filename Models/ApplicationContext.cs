using Microsoft.EntityFrameworkCore;
namespace перенос_бд_на_Web.Models
{
    public class ApplicationContext: DbContext
    {
        public DbSet<NedostovernayaTM> tm { get; set; }
        public DbSet<ModelErrors> modelErrors { get; set; }
        public DbSet<TMValues> TMValues { get; set; }
        public DbSet<Slices> slices { get; set; }
        public DbSet <ActivePowerImbalance> active_power_imbalance { get; set; }
        public DbSet<ReactivePowerImbalance> reactive_power_imbalance { get; set; }
 
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) 
        { 
            Database.EnsureCreated();
        }

    }
}
