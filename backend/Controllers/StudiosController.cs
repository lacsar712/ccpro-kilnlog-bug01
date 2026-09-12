using KilnLog.Api.Data;
using KilnLog.Api.Dtos;
using KilnLog.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KilnLog.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/studios")]
public class StudiosController : ControllerBase
{
    private readonly AppDbContext _db;

    public StudiosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<StudioDto>>> List()
    {
        var list = await _db.Studios
            .OrderBy(s => s.Id)
            .Select(s => new StudioDto(s.Id, s.Name, s.City, s.Notes, s.Kilns.Count))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudioDto>> Get(int id)
    {
        var s = await _db.Studios.Include(x => x.Kilns).FirstOrDefaultAsync(x => x.Id == id);
        if (s is null) return NotFound(new { message = "工作室不存在" });
        return Ok(new StudioDto(s.Id, s.Name, s.City, s.Notes, s.Kilns.Count));
    }

    [HttpPost]
    public async Task<ActionResult<StudioDto>> Create([FromBody] StudioWriteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.City))
            return BadRequest(new { message = "名称与城市必填" });

        var s = new Studio { Name = dto.Name.Trim(), City = dto.City.Trim(), Notes = dto.Notes };
        _db.Studios.Add(s);
        await _db.SaveChangesAsync();
        return Ok(new StudioDto(s.Id, s.Name, s.City, s.Notes, 0));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StudioDto>> Update(int id, [FromBody] StudioWriteDto dto)
    {
        var s = await _db.Studios.Include(x => x.Kilns).FirstOrDefaultAsync(x => x.Id == id);
        if (s is null) return NotFound(new { message = "工作室不存在" });
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.City))
            return BadRequest(new { message = "名称与城市必填" });

        s.Name = dto.Name.Trim();
        s.City = dto.City.Trim();
        s.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        return Ok(new StudioDto(s.Id, s.Name, s.City, s.Notes, s.Kilns.Count));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Studios.FindAsync(id);
        if (s is null) return NotFound(new { message = "工作室不存在" });
        _db.Studios.Remove(s);
        await _db.SaveChangesAsync();
        return Ok(new { message = "已删除" });
    }
}
