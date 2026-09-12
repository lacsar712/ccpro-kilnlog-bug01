using KilnLog.Api.Data;
using KilnLog.Api.Dtos;
using KilnLog.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KilnLog.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/kilns")]
public class KilnsController : ControllerBase
{
    private static readonly HashSet<string> Fuels = new(StringComparer.OrdinalIgnoreCase) { "electric", "gas", "wood" };
    private static readonly HashSet<string> Statuses = new(StringComparer.OrdinalIgnoreCase) { "idle", "firing", "cooling" };

    private readonly AppDbContext _db;

    public KilnsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<KilnDto>>> List([FromQuery] int? studioId)
    {
        var q = _db.Kilns.Include(k => k.Studio).AsQueryable();
        if (studioId.HasValue) q = q.Where(k => k.StudioId == studioId.Value);
        var list = await q.OrderBy(k => k.Id)
            .Select(k => new KilnDto(k.Id, k.StudioId, k.Studio!.Name, k.KilnCode, k.MaxTempC, k.FuelType, k.Status))
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<KilnDto>> Create([FromBody] KilnWriteDto dto)
    {
        var err = Validate(dto);
        if (err is not null) return BadRequest(new { message = err });
        if (!await _db.Studios.AnyAsync(s => s.Id == dto.StudioId))
            return BadRequest(new { message = "工作室不存在" });
        if (await _db.Kilns.AnyAsync(k => k.StudioId == dto.StudioId && k.KilnCode == dto.KilnCode.Trim()))
            return BadRequest(new { message = "同工作室窑炉编号已存在" });

        var k = new Kiln
        {
            StudioId = dto.StudioId,
            KilnCode = dto.KilnCode.Trim(),
            MaxTempC = dto.MaxTempC,
            FuelType = dto.FuelType.ToLowerInvariant(),
            Status = dto.Status.ToLowerInvariant()
        };
        _db.Kilns.Add(k);
        await _db.SaveChangesAsync();
        await _db.Entry(k).Reference(x => x.Studio).LoadAsync();
        return Ok(new KilnDto(k.Id, k.StudioId, k.Studio?.Name, k.KilnCode, k.MaxTempC, k.FuelType, k.Status));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<KilnDto>> Update(int id, [FromBody] KilnWriteDto dto)
    {
        var err = Validate(dto);
        if (err is not null) return BadRequest(new { message = err });

        var k = await _db.Kilns.Include(x => x.Studio).FirstOrDefaultAsync(x => x.Id == id);
        if (k is null) return NotFound(new { message = "窑炉不存在" });
        if (!await _db.Studios.AnyAsync(s => s.Id == dto.StudioId))
            return BadRequest(new { message = "工作室不存在" });
        if (await _db.Kilns.AnyAsync(x => x.StudioId == dto.StudioId && x.KilnCode == dto.KilnCode.Trim() && x.Id != id))
            return BadRequest(new { message = "同工作室窑炉编号已存在" });

        k.StudioId = dto.StudioId;
        k.KilnCode = dto.KilnCode.Trim();
        k.MaxTempC = dto.MaxTempC;
        k.FuelType = dto.FuelType.ToLowerInvariant();
        k.Status = dto.Status.ToLowerInvariant();
        await _db.SaveChangesAsync();
        await _db.Entry(k).Reference(x => x.Studio).LoadAsync();
        return Ok(new KilnDto(k.Id, k.StudioId, k.Studio?.Name, k.KilnCode, k.MaxTempC, k.FuelType, k.Status));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var k = await _db.Kilns.FindAsync(id);
        if (k is null) return NotFound(new { message = "窑炉不存在" });
        if (await _db.LoadBatches.AnyAsync(b => b.KilnId == id))
            return BadRequest(new { message = "存在装窑批次，无法删除" });
        _db.Kilns.Remove(k);
        await _db.SaveChangesAsync();
        return Ok(new { message = "已删除" });
    }

    private static string? Validate(KilnWriteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.KilnCode)) return "窑炉编号必填";
        if (dto.MaxTempC <= 0) return "最高温度须大于 0";
        if (!Fuels.Contains(dto.FuelType)) return "燃料类型须为 electric/gas/wood";
        if (!Statuses.Contains(dto.Status)) return "状态须为 idle/firing/cooling";
        return null;
    }
}
