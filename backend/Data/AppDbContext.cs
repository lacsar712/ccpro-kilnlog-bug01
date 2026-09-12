using KilnLog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KilnLog.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Studio> Studios => Set<Studio>();
    public DbSet<Kiln> Kilns => Set<Kiln>();
    public DbSet<FiringSchedule> FiringSchedules => Set<FiringSchedule>();
    public DbSet<ScheduleSegment> ScheduleSegments => Set<ScheduleSegment>();
    public DbSet<LoadBatch> LoadBatches => Set<LoadBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).HasMaxLength(64);
            e.Property(x => x.Role).HasMaxLength(32);
        });

        modelBuilder.Entity<Studio>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.City).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<Kiln>(e =>
        {
            e.Property(x => x.KilnCode).HasMaxLength(64).IsRequired();
            e.Property(x => x.FuelType).HasMaxLength(32).IsRequired();
            e.Property(x => x.Status).HasMaxLength(32).IsRequired();
            e.HasIndex(x => new { x.StudioId, x.KilnCode }).IsUnique();
            e.HasOne(x => x.Studio).WithMany(s => s.Kilns).HasForeignKey(x => x.StudioId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FiringSchedule>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.ConeOrTarget).HasMaxLength(64).IsRequired();
            e.Property(x => x.Status).HasMaxLength(32).IsRequired();
            e.HasOne(x => x.Kiln).WithMany(k => k.Schedules).HasForeignKey(x => x.KilnId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScheduleSegment>(e =>
        {
            e.Property(x => x.RampCPerHour).HasPrecision(10, 2);
            e.HasOne(x => x.Schedule).WithMany(s => s.Segments).HasForeignKey(x => x.ScheduleId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.ScheduleId, x.Seq }).IsUnique();
        });

        modelBuilder.Entity<LoadBatch>(e =>
        {
            e.Property(x => x.Status).HasMaxLength(32).IsRequired();
            e.HasOne(x => x.Kiln).WithMany(k => k.LoadBatches).HasForeignKey(x => x.KilnId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Schedule).WithMany(s => s.LoadBatches).HasForeignKey(x => x.ScheduleId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
