/*
 * FILE: SharedDataTransferObjects.cs
 * PROJECT: AssetManagement.Shared / Models
 * PURPOSE: Defines all Data Transfer Object (DTO) classes shared between the Server API
 *          and the Blazor WASM Client. DTOs are intentionally lightweight — they carry
 *          only the data needed by the UI and contain no EF Core navigation properties
 *          or database-specific attributes. The Server maps entities → DTOs before
 *          sending responses; the Client maps DTOs → SaveRequest objects before posting.
 *
 *          DTOs defined here:
 *            SiteDto, DepartmentDto
 *            AppUserDto, LoginRequest, LoginResponse, CreateUserRequest
 *            AssetDto, SaveAssetRequest
 *            AssignmentDto, CreateAssignmentRequest, ReturnAssignmentRequest
 *            LifecycleEventDto, CreateLifecycleEventRequest
 *            LicenseDto, SaveLicenseRequest
 *            AuditEntryDto
 */

using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Shared.Models;

// ─── SITES ───────────────────────────────────────────────────────────────────
public class SiteDto
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

// ─── DEPARTMENTS ─────────────────────────────────────────────────────────────
public class DepartmentDto
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    public string SiteName { get; set; } = "";
    public string Name { get; set; } = "";
}

// ─── USERS ───────────────────────────────────────────────────────────────────
public class AppUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Role { get; set; } = "Viewer";
    public int SiteId { get; set; }
    public string SiteName { get; set; } = "";
    public int DeptId { get; set; }
    public string DeptName { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public string Initials => string.Concat(
        DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                   .Take(2).Select(w => w[0])).ToUpper();
}

public class LoginRequest
{
    [Required] public string Username { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public AppUserDto? User { get; set; }
}

public class CreateUserRequest
{
    [Required] public string Username { get; set; } = "";
    [Required] public string DisplayName { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    public string Role { get; set; } = "Viewer";
    public int SiteId { get; set; }
    public int DeptId { get; set; }
}

// ─── ASSETS ──────────────────────────────────────────────────────────────────
public class AssetDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = "";
    public string AssetTag { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string AssetType { get; set; } = "Laptop";
    public string Manufacturer { get; set; } = "";
    public string Model { get; set; } = "";
    public int SiteId { get; set; }
    public string SiteName { get; set; } = "";
    public string LifecycleStatus { get; set; } = "Available";
    public string Hostname { get; set; } = "";
    public string Description { get; set; } = "";
    public string ComplianceId { get; set; } = "";
    public string EncryptionProtocol { get; set; } = "";
    public DateOnly? LastAudit { get; set; }
    public DateOnly CreatedAt { get; set; }
}

public class SaveAssetRequest
{
    public int Id { get; set; }
    [Required] public string AssetCode { get; set; } = "";
    [Required] public string AssetTag { get; set; } = "";
    [Required] public string SerialNumber { get; set; } = "";
    [Required] public string AssetType { get; set; } = "Laptop";
    [Required] public string Manufacturer { get; set; } = "";
    [Required] public string Model { get; set; } = "";
    public int SiteId { get; set; }
    public string LifecycleStatus { get; set; } = "Available";
    public string Hostname { get; set; } = "";
    public string Description { get; set; } = "";
    public string ComplianceId { get; set; } = "";
    public string EncryptionProtocol { get; set; } = "";
    public DateOnly? LastAudit { get; set; }
}

// ─── ASSIGNMENTS ─────────────────────────────────────────────────────────────
public class AssignmentDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = "";
    public string AssetModel { get; set; } = "";
    public int UserId { get; set; }
    public string UserDisplayName { get; set; } = "";
    public string UserUsername { get; set; } = "";
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string CreatedByDisplayName { get; set; } = "";
    public DateOnly CreatedAt { get; set; }
    public bool IsActive => EndDate == null;
}

public class CreateAssignmentRequest
{
    public int AssetId { get; set; }
    public int UserId { get; set; }
    public DateOnly EffectiveDate { get; set; }
}

public class ReturnAssignmentRequest
{
    public DateOnly ReturnDate { get; set; }
}

// ─── LIFECYCLE ───────────────────────────────────────────────────────────────
public class LifecycleEventDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = "";
    public string OldStatus { get; set; } = "";
    public string NewStatus { get; set; } = "";
    public string Reason { get; set; } = "";
    public string ChangedByDisplayName { get; set; } = "";
    public DateOnly ChangedAt { get; set; }
}

public class CreateLifecycleEventRequest
{
    public int AssetId { get; set; }
    [Required] public string NewStatus { get; set; } = "";
    [Required] public string Reason { get; set; } = "";
}

// ─── LICENSES ────────────────────────────────────────────────────────────────
public class LicenseDto
{
    public int Id { get; set; }
    public string Product { get; set; } = "";
    public string Vendor { get; set; } = "";
    public int SiteId { get; set; }
    public string SiteName { get; set; } = "";
    public string Type { get; set; } = "Per-Seat";
    public int Total { get; set; }
    public DateOnly? Start { get; set; }
    public DateOnly? Expiry { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsExpiringSoon => Expiry.HasValue && Status == "Active" &&
        Expiry.Value.ToDateTime(TimeOnly.MinValue) < DateTime.UtcNow.AddDays(30);
}

public class SaveLicenseRequest
{
    public int Id { get; set; }
    [Required] public string Product { get; set; } = "";
    public string Vendor { get; set; } = "";
    public int SiteId { get; set; }
    public string Type { get; set; } = "Per-Seat";
    [Range(0, int.MaxValue)] public int Total { get; set; }
    public DateOnly? Start { get; set; }
    public DateOnly? Expiry { get; set; }
    public string Status { get; set; } = "Active";
}

// ─── AUDIT ───────────────────────────────────────────────────────────────────
public class AuditEntryDto
{
    public int Id { get; set; }
    public DateTime Ts { get; set; }
    public string Username { get; set; } = "";
    public string Action { get; set; } = "";
    public string Target { get; set; } = "";
}
