namespace KilnLog.Api.Models;

public class FiringSchedule
{
    public int Id { get; set; }
    public int KilnId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ConeOrTarget { get; set; } = string.Empty;
    /// <summary>draft | approved | retired</summary>
    public string Status { get; set; } = "draft";

    public Kiln? Kiln { get; set; }
    public ICollection<ScheduleSegment> Segments { get; set; } = new List<ScheduleSegment>();
    public ICollection<LoadBatch> LoadBatches { get; set; } = new List<LoadBatch>();
}
