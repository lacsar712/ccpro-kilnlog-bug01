namespace KilnLog.Api.Models;

public class Kiln
{
    public int Id { get; set; }
    public int StudioId { get; set; }
    public string KilnCode { get; set; } = string.Empty;
    public int MaxTempC { get; set; }
    /// <summary>electric | gas | wood</summary>
    public string FuelType { get; set; } = "electric";
    /// <summary>idle | firing | cooling</summary>
    public string Status { get; set; } = "idle";

    public Studio? Studio { get; set; }
    public ICollection<FiringSchedule> Schedules { get; set; } = new List<FiringSchedule>();
    public ICollection<LoadBatch> LoadBatches { get; set; } = new List<LoadBatch>();
}
