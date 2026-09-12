using KilnLog.Api.Data;
using KilnLog.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KilnLog.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateOnly(now.Year, now.Month, 1);

        var kilnCount = await _db.Kilns.CountAsync();
        var firingKilnCount = await _db.Kilns.CountAsync(k => k.Status == "firing");
        var monthLoadBatchCount = await _db.LoadBatches.CountAsync(b => b.LoadDate >= monthStart);
        var approvedScheduleCount = await _db.FiringSchedules.CountAsync(s => s.Status == "approved");

        return Ok(new DashboardDto(kilnCount, firingKilnCount, monthLoadBatchCount, approvedScheduleCount));
    }
}
