using KilnLog.Api.Data;
using KilnLog.Api.Dtos;
using KilnLog.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KilnLog.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/firing-schedules")]
public class FiringSchedulesController : ControllerBase
{
    private static readonly HashSet<string> Statuses = new(StringComparer.OrdinalIgnoreCase)
        { "draft", "approved", "retired" };

    private readonly AppDbContext _db;

    public FiringSchedulesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ScheduleDto>>> List([FromQuery] int? kilnId)
    {
        var q = _db.FiringSchedules.Include(s => s.Kiln).Include(s => s.Segments).AsQueryable();
        if (kilnId.HasValue) q = q.Where(s => s.KilnId == kilnId.Value);
        var list = await q.OrderBy(s => s.Id).ToListAsync();
        return Ok(list.Select(Map).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ScheduleDto>> Get(int id)
    {
        var s = await _db.FiringSchedules.Include(x => x.Kiln).Include(x => x.Segments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (s is null) return NotFound(new { message = "烧成制度不存在" });
        return Ok(Map(s));
    }

    [HttpPost]
    public async Task<ActionResult<ScheduleDto>> Create([FromBody] ScheduleWriteDto dto)
    {
        var err = Validate(dto);
        if (err is not null) return BadRequest(new { message = err });
        if (!await _db.Kilns.AnyAsync(k => k.Id == dto.KilnId))
            return BadRequest(new { message = "窑炉不存在" });

        var s = new FiringSchedule
        {
            KilnId = dto.KilnId,
            Name = dto.Name.Trim(),
            ConeOrTarget = dto.ConeOrTarget.Trim(),
            Status = dto.Status.ToLowerInvariant(),
            Segments = dto.Segments.Select(seg => new ScheduleSegment
            {
                Seq = seg.Seq,
                RampCPerHour = seg.RampCPerHour,
                HoldMinutes = seg.HoldMinutes,
                TargetTempC = seg.TargetTempC
            }).ToList()
        };
        _db.FiringSchedules.Add(s);
        await _db.SaveChangesAsync();
        await _db.Entry(s).Reference(x => x.Kiln).LoadAsync();
        return Ok(Map(s));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ScheduleDto>> Update(int id, [FromBody] ScheduleWriteDto dto)
    {
        var err = Validate(dto);
        if (err is not null) return BadRequest(new { message = err });

        var s = await _db.FiringSchedules.Include(x => x.Kiln).Include(x => x.Segments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (s is null) return NotFound(new { message = "烧成制度不存在" });
        if (!await _db.Kilns.AnyAsync(k => k.Id == dto.KilnId))
            return BadRequest(new { message = "窑炉不存在" });

        s.KilnId = dto.KilnId;
        s.Name = dto.Name.Trim();
        s.ConeOrTarget = dto.ConeOrTarget.Trim();
        s.Status = dto.Status.ToLowerInvariant();
        _db.ScheduleSegments.RemoveRange(s.Segments);
        s.Segments = dto.Segments.Select(seg => new ScheduleSegment
        {
            Seq = seg.Seq,
            RampCPerHour = seg.RampCPerHour,
            HoldMinutes = seg.HoldMinutes,
            TargetTempC = seg.TargetTempC
        }).ToList();

        await _db.SaveChangesAsync();
        await _db.Entry(s).Reference(x => x.Kiln).LoadAsync();
        return Ok(Map(s));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.FiringSchedules.FindAsync(id);
        if (s is null) return NotFound(new { message = "烧成制度不存在" });
        if (await _db.LoadBatches.AnyAsync(b => b.ScheduleId == id))
            return BadRequest(new { message = "存在装窑批次引用，无法删除" });
        _db.FiringSchedules.Remove(s);
        await _db.SaveChangesAsync();
        return Ok(new { message = "已删除" });
    }

    private static ScheduleDto Map(FiringSchedule s) => new(
        s.Id,
        s.KilnId,
        s.Kiln?.KilnCode,
        s.Name,
        s.ConeOrTarget,
        s.Status,
        s.Segments.OrderBy(x => x.Seq).Select(x => new SegmentDto(x.Id, x.Seq, x.RampCPerHour, x.HoldMinutes, x.TargetTempC)).ToList());

    private static string? Validate(ScheduleWriteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return "制度名称必填";
        if (string.IsNullOrWhiteSpace(dto.ConeOrTarget)) return "锥号/目标温度必填";
        if (!Statuses.Contains(dto.Status)) return "状态须为 draft/approved/retired";
        if (dto.Segments is null || dto.Segments.Count == 0) return "至少一段曲线";
        if (dto.Segments.Select(x => x.Seq).Distinct().Count() != dto.Segments.Count)
            return "曲线段序号不可重复";
        return null;
    }
}
