/*
 * FILE: ReferenceDataControllers.cs
 * PROJECT: AssetManagement.Server / Controllers
 * PURPOSE: Six lightweight controllers serving reference and supporting data from
 *          InMemoryDataStore. No EF Core or PostgreSQL involved in this file.
 *
 *          LifecycleEventsController  — GET/POST asset lifecycle status transitions
 *          LicensesController         — GET/POST/PUT/DELETE software license records
 *          UsersController            — GET/POST users, PUT toggle active/inactive
 *          SitesController            — GET office site list (Naples, Fort Myers)
 *          DepartmentsController      — GET department list per site
 *          AuditController            — GET/POST immutable audit log entries
 */

using AssetManagement.Server.Data;
using AssetManagement.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Server.Controllers;

// ── LIFECYCLE EVENTS ─────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class LifecycleEventsController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet]
    public List<LifecycleEventDto> GetAll() =>
        store.LifecycleEvents.OrderByDescending(e => e.ChangedAt).ToList();

    [HttpPost]
    public ActionResult<LifecycleEventDto> Create(
        [FromBody] CreateLifecycleEventRequest req,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        var asset = store.Assets.FirstOrDefault(a => a.Id == req.AssetId);
        if (asset == null) return NotFound("Asset not found.");
        if (string.IsNullOrWhiteSpace(req.Reason)) return BadRequest("Reason is required.");

        var user = store.GetUserById(userId);
        var oldStatus = asset.LifecycleStatus;
        asset.LifecycleStatus = req.NewStatus;

        var ev = new LifecycleEventDto
        {
            Id                   = store.NextLcEventId(),
            AssetId              = req.AssetId,
            AssetCode            = asset.AssetCode,
            OldStatus            = oldStatus,
            NewStatus            = req.NewStatus,
            Reason               = req.Reason,
            ChangedByDisplayName = user?.DisplayName ?? "System",
            ChangedAt            = DateOnly.FromDateTime(DateTime.UtcNow),
        };
        store.LifecycleEvents.Add(ev);
        return CreatedAtAction(nameof(GetAll), ev);
    }
}

// ── LICENSES ─────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class LicensesController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet]
    public List<LicenseDto> GetAll() =>
        store.Licenses.OrderByDescending(l => l.Id).ToList();

    [HttpPost]
    public ActionResult<LicenseDto> Create([FromBody] SaveLicenseRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Product) || req.Total < 0)
            return BadRequest("Product name and valid quantity required.");

        var site = store.Sites.FirstOrDefault(s => s.Id == req.SiteId);
        var lic = new LicenseDto
        {
            Id       = store.NextLicenseId(),
            Product  = req.Product,
            Vendor   = req.Vendor,
            SiteId   = req.SiteId,
            SiteName = site?.Name ?? "",
            Type     = req.Type,
            Total    = req.Total,
            Start    = req.Start,
            Expiry   = req.Expiry,
            Status   = req.Status,
        };
        store.Licenses.Add(lic);
        return CreatedAtAction(nameof(GetAll), lic);
    }

    [HttpPut("{id}")]
    public ActionResult<LicenseDto> Update(int id, [FromBody] SaveLicenseRequest req)
    {
        var lic = store.Licenses.FirstOrDefault(l => l.Id == id);
        if (lic == null) return NotFound();
        if (string.IsNullOrWhiteSpace(req.Product) || req.Total < 0)
            return BadRequest("Product name and valid quantity required.");

        var site = store.Sites.FirstOrDefault(s => s.Id == req.SiteId);
        lic.Product  = req.Product;
        lic.Vendor   = req.Vendor;
        lic.SiteId   = req.SiteId;
        lic.SiteName = site?.Name ?? "";
        lic.Type     = req.Type;
        lic.Total    = req.Total;
        lic.Start    = req.Start;
        lic.Expiry   = req.Expiry;
        lic.Status   = req.Status;
        return Ok(lic);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var lic = store.Licenses.FirstOrDefault(l => l.Id == id);
        if (lic == null) return NotFound();
        store.Licenses.Remove(lic);
        return NoContent();
    }
}

// ── USERS ─────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class UsersController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet]
    public List<AppUserDto> GetAll() =>
        store.Users.Select(store.ToDto).ToList();

    [HttpPost]
    public ActionResult<AppUserDto> Create([FromBody] CreateUserRequest req)
    {
        if (store.Users.Any(u => u.Username == req.Username))
            return Conflict("Username already taken.");

        var site = store.Sites.FirstOrDefault(s => s.Id == req.SiteId);
        var dept = store.Departments.FirstOrDefault(d => d.Id == req.DeptId);
        var user = new InMemoryUser
        {
            Id            = store.NextUserId(),
            Username      = req.Username,
            DisplayName   = req.DisplayName,
            PasswordPlain = req.Password,
            Role          = req.Role,
            SiteId        = req.SiteId,
            SiteName      = site?.Name ?? "",
            DeptId        = req.DeptId,
            DeptName      = dept?.Name ?? "",
            IsActive      = true,
        };
        store.Users.Add(user);
        return CreatedAtAction(nameof(GetAll), store.ToDto(user));
    }

    [HttpPut("{id}/toggle")]
    public IActionResult Toggle(int id, [FromHeader(Name = "X-User-Id")] int requesterId)
    {
        if (id == requesterId) return BadRequest("Cannot deactivate yourself.");
        var user = store.GetUserById(id);
        if (user == null) return NotFound();
        user.IsActive = !user.IsActive;
        return NoContent();
    }
}

// ── SITES ─────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class SitesController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet]
    public List<SiteDto> GetAll() => store.Sites.ToList();
}

// ── DEPARTMENTS ───────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class DepartmentsController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet]
    public List<DepartmentDto> GetAll() => store.Departments.ToList();
}

// ── AUDIT ─────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class AuditController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet]
    public List<AuditEntryDto> GetAll() =>
        store.AuditEntries.OrderByDescending(e => e.Ts).ToList();

    [HttpPost]
    public IActionResult Add([FromBody] AuditEntryDto entry)
    {
        store.AuditEntries.Add(new AuditEntryDto
        {
            Id       = store.NextAuditId(),
            Ts       = DateTime.UtcNow,
            Username = entry.Username,
            Action   = entry.Action,
            Target   = entry.Target,
        });
        return NoContent();
    }
}
