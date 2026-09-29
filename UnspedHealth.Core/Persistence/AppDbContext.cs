using Microsoft.EntityFrameworkCore;
using UnspedHealth.Core.Entities;

namespace UnspedHealth.API.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Server> Servers { get; set; }
        public DbSet<ServiceHealthCheck> ServiceHealthChecks { get; set; }
        public DbSet<HealthCheckLog> HealthCheckLogs { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServiceHealthCheck>().HasIndex(x => x.ServerIP);
            modelBuilder.Entity<HealthCheckLog>().HasIndex(x => x.HealthCheckID);
        }
    }
}
