using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using MonthlyScheduler.Models;

namespace MonthlyScheduler.Data;

public class SchedulerDbContext : DbContext
{
    public DbSet<Member> Members { get; set; }
    public DbSet<ServiceSchedule> ServiceSchedules { get; set; }
    public DbSet<DutyAssignment> DutyAssignments { get; set; }
    public DbSet<MemberDuty> MemberDuties { get; set; }
    public DbSet<DutyType> DutyTypes { get; set; }
    public DbSet<GeneratedSchedule> GeneratedSchedules { get; set; }
    public DbSet<DailySchedule> DailySchedules { get; set; }
    public DbSet<ScheduleAssignment> ScheduleAssignments { get; set; }

    public string DbPath { get; }

    public SchedulerDbContext()
    {
        // Get the directory where the application executable is located
        var appPath = AppDomain.CurrentDomain.BaseDirectory;
        
        // Create a "data" directory if it doesn't exist
        var dataPath = Path.Combine(appPath, "data");
        Directory.CreateDirectory(dataPath);
        
        // Set database path in the data directory
        DbPath = Path.Combine(dataPath, "scheduler.db");
        Console.WriteLine($"Using database at: {DbPath}");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}")
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

        Console.WriteLine($"Configuring database connection to: {DbPath}");
        
        // Verify the database exists and is accessible
        if (File.Exists(DbPath))
        {
            Console.WriteLine("Database file exists");
            try
            {
                using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={DbPath}");
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Members";
                var count = Convert.ToInt32(command.ExecuteScalar());
                Console.WriteLine($"Direct database query shows {count} members in the database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing database: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Database file does not exist!");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Member -> MemberDuty relationship
        modelBuilder.Entity<Member>()
            .HasMany(m => m.AvailableDuties)
            .WithOne(d => d.Member)
            .HasForeignKey(d => d.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure DutyType -> MemberDuty relationship
        modelBuilder.Entity<MemberDuty>()
            .HasOne(md => md.DutyType)
            .WithMany(dt => dt.MemberDuties)
            .HasForeignKey(md => md.DutyTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure DutyType -> DutyAssignment relationship
        modelBuilder.Entity<DutyAssignment>()
            .HasOne(da => da.DutyType)
            .WithMany(dt => dt.DutyAssignments)
            .HasForeignKey(da => da.DutyTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure GeneratedSchedule -> DailySchedule relationship
        modelBuilder.Entity<GeneratedSchedule>()
            .HasMany(s => s.DailySchedules)
            .WithOne(d => d.Schedule)
            .HasForeignKey(d => d.GeneratedScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure DailySchedule -> ScheduleAssignment relationship
        modelBuilder.Entity<DailySchedule>()
            .HasMany(d => d.Assignments)
            .WithOne(a => a.DailySchedule)
            .HasForeignKey(a => a.DailyScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for Year and Month (non-unique)
        modelBuilder.Entity<GeneratedSchedule>()
            .HasIndex(s => new { s.Year, s.Month });

        base.OnModelCreating(modelBuilder);
    }
}