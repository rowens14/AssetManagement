/*
 * FILE: AssetAssignmentController.cs
 * PROJECT: AssetManagement.Server / Controllers
 * PURPOSE: REST API controller for asset assignment tracking.
 *          In in-memory mode, reads/writes InMemoryDataStore.Assignments directly.
 *          Creating an assignment automatically sets the asset lifecycle status to
 *          "Deployed" and appends a lifecycle event, matching real-DB behaviour.
 *          Routes: GET /api/assignments, POST /api/assignments,
 *                  PUT /api/assignments/{id}/return, DELETE /api/assignments/{id}
 */

using AssetManagement.Server.Data;
using AssetManagement.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet]
    public List<AssignmentDto> GetAll() =>
        store.Assignments.OrderByDescending(a => a.EffectiveDate).ToList();

    [HttpPost]
    public ActionResult<AssignmentDto> Create(
        [FromBody] CreateAssignmentRequest req,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        var asset = store.Assets.FirstOrDefault(a => a.Id == req.AssetId);
        var user  = store.GetUserById(req.UserId);
        if (asset == null || user == null) return BadRequest("Invalid asset or user.");

        var createdBy = store.GetUserById(userId);
        var assignment = new AssignmentDto
        {
            Id                   = store.NextAssignmentId(),
            AssetId              = req.AssetId,
            AssetCode            = asset.AssetCode,
            AssetModel           = asset.Model,
            UserId               = req.UserId,
            UserDisplayName      = user.DisplayName,
            UserUsername         = user.Username,
            EffectiveDate        = req.EffectiveDate,
            EndDate              = null,
            CreatedByDisplayName = createdBy?.DisplayName ?? "System",
            CreatedAt            = DateOnly.FromDateTime(DateTime.UtcNow),
        };
        store.Assignments.Add(assignment);

        // Auto-deploy asset
        if (asset.LifecycleStatus != "Deployed")
        {
            var oldStatus = asset.LifecycleStatus;
            asset.LifecycleStatus = "Deployed";
            store.LifecycleEvents.Add(new LifecycleEventDto
            {
                Id                   = store.NextLcEventId(),
                AssetId              = asset.Id,
                AssetCode            = asset.AssetCode,
                OldStatus            = oldStatus,
                NewStatus            = "Deployed",
                Reason               = $"Assigned to {user.DisplayName}",
                ChangedByDisplayName = createdBy?.DisplayName ?? "System",
                ChangedAt            = DateOnly.FromDateTime(DateTime.UtcNow),
            });
        }
        return CreatedAtAction(nameof(GetAll), assignment);
    }

    [HttpPut("{id}/return")]
    public IActionResult Return(int id, [FromBody] ReturnAssignmentRequest req)
    {
        var assignment = store.Assignments.FirstOrDefault(a => a.Id == id);
        if (assignment == null) return NotFound();
        assignment.EndDate = req.ReturnDate;
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var assignment = store.Assignments.FirstOrDefault(a => a.Id == id);
        if (assignment == null) return NotFound();
        store.Assignments.Remove(assignment);
        return NoContent();
    }
}
