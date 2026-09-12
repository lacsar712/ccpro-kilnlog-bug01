namespace KilnLog.Api.Models;

public class LoadBatch
{
    public int Id { get; set; }
    public int KilnId { get; set; }
    public int ScheduleId { get; set; }
    public DateOnly LoadDate { get; set; }
    public int PieceCount { get; set; }
    public string? GlazeNotes { get; set; }
    /// <summary>planned | loaded | fired | unloaded</summary>
    public string Status { get; set; } = "planned";

    public Kiln? Kiln { get; set; }
    public FiringSchedule? Schedule { get; set; }
}
