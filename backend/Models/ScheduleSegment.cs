namespace KilnLog.Api.Models;

public class ScheduleSegment
{
    public int Id { get; set; }
    public int ScheduleId { get; set; }
    public int Seq { get; set; }
    public decimal RampCPerHour { get; set; }
    public int HoldMinutes { get; set; }
    public int TargetTempC { get; set; }

    public FiringSchedule? Schedule { get; set; }
}
