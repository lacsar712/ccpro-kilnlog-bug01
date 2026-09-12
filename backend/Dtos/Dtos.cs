namespace KilnLog.Api.Dtos;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, UserDto User);

public record UserDto(int Id, string Username, string Role);

public record StudioDto(int Id, string Name, string City, string? Notes, int KilnCount);

public record StudioWriteDto(string Name, string City, string? Notes);

public record KilnDto(
    int Id,
    int StudioId,
    string? StudioName,
    string KilnCode,
    int MaxTempC,
    string FuelType,
    string Status);

public record KilnWriteDto(
    int StudioId,
    string KilnCode,
    int MaxTempC,
    string FuelType,
    string Status);

public record SegmentDto(int Id, int Seq, decimal RampCPerHour, int HoldMinutes, int TargetTempC);

public record SegmentWriteDto(int Seq, decimal RampCPerHour, int HoldMinutes, int TargetTempC);

public record ScheduleDto(
    int Id,
    int KilnId,
    string? KilnCode,
    string Name,
    string ConeOrTarget,
    string Status,
    List<SegmentDto> Segments);

public record ScheduleWriteDto(
    int KilnId,
    string Name,
    string ConeOrTarget,
    string Status,
    List<SegmentWriteDto> Segments);

public record LoadBatchDto(
    int Id,
    int KilnId,
    string? KilnCode,
    int ScheduleId,
    string? ScheduleName,
    DateOnly LoadDate,
    int PieceCount,
    string? GlazeNotes,
    string Status);

public record LoadBatchWriteDto(
    int KilnId,
    int ScheduleId,
    DateOnly LoadDate,
    int PieceCount,
    string? GlazeNotes,
    string Status);

public record DashboardDto(
    int KilnCount,
    int FiringKilnCount,
    int MonthLoadBatchCount,
    int ApprovedScheduleCount);
