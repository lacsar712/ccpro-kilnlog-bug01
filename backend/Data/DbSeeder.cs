using KilnLog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KilnLog.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var admin = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "admin"
        };
        var potter = new User
        {
            Username = "potter",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "potter"
        };
        db.Users.AddRange(admin, potter);

        var studio = new Studio
        {
            Name = "青焰陶艺工作室",
            City = "景德镇",
            Notes = "主攻柴烧与电窑釉成"
        };
        db.Studios.Add(studio);
        db.SaveChanges();

        var kilnA = new Kiln
        {
            StudioId = studio.Id,
            KilnCode = "E-01",
            MaxTempC = 1300,
            FuelType = "electric",
            Status = "idle"
        };
        var kilnB = new Kiln
        {
            StudioId = studio.Id,
            KilnCode = "G-02",
            MaxTempC = 1280,
            FuelType = "gas",
            Status = "firing"
        };
        db.Kilns.AddRange(kilnA, kilnB);
        db.SaveChanges();

        var schedule = new FiringSchedule
        {
            KilnId = kilnA.Id,
            Name = "Cone 6 氧化釉成",
            ConeOrTarget = "Cone 6 / 1220°C",
            Status = "approved",
            Segments =
            {
                new ScheduleSegment { Seq = 1, RampCPerHour = 100, HoldMinutes = 0, TargetTempC = 600 },
                new ScheduleSegment { Seq = 2, RampCPerHour = 80, HoldMinutes = 15, TargetTempC = 1000 },
                new ScheduleSegment { Seq = 3, RampCPerHour = 60, HoldMinutes = 20, TargetTempC = 1220 }
            }
        };
        var draft = new FiringSchedule
        {
            KilnId = kilnB.Id,
            Name = "还原气窑试验",
            ConeOrTarget = "Cone 10 / 1280°C",
            Status = "draft",
            Segments =
            {
                new ScheduleSegment { Seq = 1, RampCPerHour = 90, HoldMinutes = 0, TargetTempC = 900 },
                new ScheduleSegment { Seq = 2, RampCPerHour = 50, HoldMinutes = 30, TargetTempC = 1280 }
            }
        };
        db.FiringSchedules.AddRange(schedule, draft);
        db.SaveChanges();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.LoadBatches.AddRange(
            new LoadBatch
            {
                KilnId = kilnA.Id,
                ScheduleId = schedule.Id,
                LoadDate = today.AddDays(-3),
                PieceCount = 24,
                GlazeNotes = "天青釉碗 ×12，茶盏 ×12",
                Status = "fired"
            },
            new LoadBatch
            {
                KilnId = kilnB.Id,
                ScheduleId = draft.Id,
                LoadDate = today,
                PieceCount = 18,
                GlazeNotes = "柴烧坯体预装",
                Status = "loaded"
            }
        );
        db.SaveChanges();
    }

    public static async Task MigrateAndSeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        Seed(db);
    }
}
