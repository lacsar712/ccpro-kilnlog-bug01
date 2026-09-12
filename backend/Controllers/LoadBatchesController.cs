using KilnLog.Api.Data;
using KilnLog.Api.Dtos;
using KilnLog.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KilnLog.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/load-batches")]
public class LoadBatchesController : ControllerBase
{
    private static readonly HashSet<string> Statuses = new(StringComparer.OrdinalIgnoreCase)
        { "planned", "loaded", "fired", "unloaded" };

    private readonly AppDbContext _db;

    public LoadBatchesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<LoadBatchDto>>> List([FromQuery] int? kilnId)
    {
        var q = _db.LoadBatches.Include(b => b.Kiln).Include(b => b.Schedule).AsQueryable();
        if (kilnId.HasValue) q = q.Where(b => b.KilnId == kilnId.Value);
        var list = await q.OrderByDescending(b => b.LoadDate).ThenByDescending(b => b.Id)
            .Select(b => new LoadBatchDto(
                b.Id, b.KilnId, b.Kiln!.KilnCode, b.ScheduleId, b.Schedule!.Name,
                b.LoadDate, b.PieceCount, b.GlazeNotes, b.Status))
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<LoadBatchDto>> Create([FromBody] LoadBatchWriteDto dto)
    {
        var err = await ValidateAsync(dto);
        if (err is not null) return BadRequest(new { message = err });

        var b = new LoadBatch
        {
            KilnId = dto.KilnId,
            ScheduleId = dto.ScheduleId,
            LoadDate = dto.LoadDate,
            PieceCount = dto.PieceCount,
            GlazeNotes = dto.GlazeNotes,
            Status = dto.Status.ToLowerInvariant()
        };
        _db.LoadBatches.Add(b);
        await _db.SaveChangesAsync();
        await _db.Entry(b).Reference(x => x.Kiln).LoadAsync();
        await _db.Entry(b).Reference(x => x.Schedule).LoadAsync();
        return Ok(new LoadBatchDto(b.Id, b.KilnId, b.Kiln?.KilnCode, b.ScheduleId, b.Schedule?.Name,
            b.LoadDate, b.PieceCount, b.GlazeNotes, b.Status));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LoadBatchDto>> Update(int id, [FromBody] LoadBatchWriteDto dto)
    {
        var err = await ValidateAsync(dto);
        if (err is not null) return BadRequest(new { message = err });

        var b = await _db.LoadBatches.Include(x => x.Kiln).Include(x => x.Schedule)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (b is null) return NotFound(new { message = "装窑批次不存在" });

        b.KilnId = dto.KilnId;
        b.ScheduleId = dto.ScheduleId;
        b.LoadDate = dto.LoadDate;
        b.PieceCount = dto.PieceCount;
        b.GlazeNotes = dto.GlazeNotes;
        b.Status = dto.Status.ToLowerInvariant();
        await _db.SaveChangesAsync();
        await _db.Entry(b).Reference(x => x.Kiln).LoadAsync();
        await _db.Entry(b).Reference(x => x.Schedule).LoadAsync();
        return Ok(new LoadBatchDto(b.Id, b.KilnId, b.Kiln?.KilnCode, b.ScheduleId, b.Schedule?.Name,
            b.LoadDate, b.PieceCount, b.GlazeNotes, b.Status));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var b = await _db.LoadBatches.FindAsync(id);
        if (b is null) return NotFound(new { message = "装窑批次不存在" });
        _db.LoadBatches.Remove(b);
        await _db.SaveChangesAsync();
        return Ok(new { message = "已删除" });
    }

    private async Task<string?> ValidateAsync(LoadBatchWriteDto dto)
    {
        if (dto.PieceCount <= 0) return "件数须大于 0";
        if (!Statuses.Contains(dto.Status)) return "状态须为 planned/loaded/fired/unloaded";
        if (!await _db.Kilns.AnyAsync(k => k.Id == dto.KilnId)) return "窑炉不存在";
        var schedule = await _db.FiringSchedules.FindAsync(dto.ScheduleId);
        if (schedule is null) return "烧成制度不存在";
        if (schedule.KilnId != dto.KilnId) return "制度须属于所选窑炉";
        return null;
    }
}
