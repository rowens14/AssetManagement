/*
 * FILE: AssetInventoryController.cs
 * PROJECT: AssetManagement.Server / Controllers
 * PURPOSE: REST API controller for hardware asset inventory (Tier 2 — Business Logic).
 *          In in-memory mode, reads from and writes to InMemoryDataStore.Assets.
 *          All CRUD operations work fully — creates, edits and deletes persist for
 *          the lifetime of the server process. Validates uniqueness of asset_code,
 *          asset_tag and serial_number against the in-memory list.
 *          Routes: GET /api/assets, POST /api/assets,
 *                  PUT /api/assets/{id}, DELETE /api/assets/{id}
 */

using AssetManagement.Server.Data;
using AssetManagement.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet]
    public List<AssetDto> GetAll() =>
        store.Assets.OrderByDescending(a => a.CreatedAt).ToList();

    [HttpPost]
    public ActionResult<AssetDto> Create([FromBody] SaveAssetRequest req)
    {
        if (store.Assets.Any(a =>
                a.AssetCode    == req.AssetCode    ||
                a.AssetTag     == req.AssetTag     ||
                a.SerialNumber == req.SerialNumber))
            return Conflict("Asset code, tag, or serial already in use.");

        var site = store.Sites.FirstOrDefault(s => s.Id == req.SiteId);
        var asset = new AssetDto
        {
            Id                = store.NextAssetId(),
            AssetCode         = req.AssetCode,
            AssetTag          = req.AssetTag,
            SerialNumber      = req.SerialNumber,
            AssetType         = req.AssetType,
            Manufacturer      = req.Manufacturer,
            Model             = req.Model,
            SiteId            = req.SiteId,
            SiteName          = site?.Name ?? "",
            LifecycleStatus   = req.LifecycleStatus,
            Hostname          = req.Hostname,
            Description       = req.Description,
            ComplianceId      = req.ComplianceId,
            EncryptionProtocol= req.EncryptionProtocol,
            LastAudit         = req.LastAudit,
            CreatedAt         = DateOnly.FromDateTime(DateTime.UtcNow),
        };
        store.Assets.Add(asset);
        return CreatedAtAction(nameof(GetAll), asset);
    }

    [HttpPut("{id}")]
    public ActionResult<AssetDto> Update(int id, [FromBody] SaveAssetRequest req)
    {
        var asset = store.Assets.FirstOrDefault(a => a.Id == id);
        if (asset == null) return NotFound();

        if (store.Assets.Any(a => a.Id != id && (
                a.AssetCode    == req.AssetCode    ||
                a.AssetTag     == req.AssetTag     ||
                a.SerialNumber == req.SerialNumber)))
            return Conflict("Asset code, tag, or serial already in use.");

        var site = store.Sites.FirstOrDefault(s => s.Id == req.SiteId);
        asset.AssetCode          = req.AssetCode;
        asset.AssetTag           = req.AssetTag;
        asset.SerialNumber       = req.SerialNumber;
        asset.AssetType          = req.AssetType;
        asset.Manufacturer       = req.Manufacturer;
        asset.Model              = req.Model;
        asset.SiteId             = req.SiteId;
        asset.SiteName           = site?.Name ?? "";
        asset.LifecycleStatus    = req.LifecycleStatus;
        asset.Hostname           = req.Hostname;
        asset.Description        = req.Description;
        asset.ComplianceId       = req.ComplianceId;
        asset.EncryptionProtocol = req.EncryptionProtocol;
        asset.LastAudit          = req.LastAudit;
        return Ok(asset);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var asset = store.Assets.FirstOrDefault(a => a.Id == id);
        if (asset == null) return NotFound();
        store.Assets.Remove(asset);
        store.Assignments.RemoveAll(a => a.AssetId == id);
        return NoContent();
    }
}
