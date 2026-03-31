/*
 * FILE: DatabaseEntityModels.cs
 * PROJECT: AssetManagement.Server / Data
 * PURPOSE: Defines all EF Core entity classes that map directly to PostgreSQL tables.
 *          These are server-side only — they are never sent to the client. Instead,
 *          controllers map them into the lightweight DTO classes defined in
 *          AssetManagement.Shared. Entities included:
 *
 *          Site            — Office locations (Naples, Fort Myers)
 *          Department      — Organisational units within a site
 *          AppUser         — System users with bcrypt-hashed passwords and RBAC roles
 *          Asset           — Hardware inventory records with compliance fields
 *          Assignment      — Tracks which user holds which asset and when
 *          LifecycleEvent  — Immutable history of asset status transitions
 *          License         — Software license records scoped to a site
 *          AuditEntry      — Append-only compliance audit log
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Server.Data;

public class Site
{
    public int Id { get; set; }
    [MaxLength(10)]  public string Code { get; set; } = "";
    [MaxLength(100)] public string Name { get; set; } = "";

    public ICollection<Department> Departments { get; set; } = [];
    public ICollection<AppUser>    Users        { get; set; } = [];
    public ICollection<Asset>      Assets       { get; set; } = [];
    public ICollection<License>    Licenses     { get; set; } = [];
}

public class Department
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    [MaxLength(100)] public string Name { get; set; } = "";

    public Site Site { get; set; } = null!;
    public ICollection<AppUser> Users { get; set; } = [];
}

public class AppUser
{
    public int Id { get; set; }
    [MaxLength(60)]  public string Username    { get; set; } = "";
    [MaxLength(120)] public string DisplayName { get; set; } = "";
    [MaxLength(256)] public string PasswordHash { get; set; } = "";  // bcrypt
    [MaxLength(20)]  public string Role         { get; set; } = "Viewer";
    public int  SiteId   { get; set; }
    public int  DeptId   { get; set; }
    public bool IsActive { get; set; } = true;

    public Site       Site       { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public ICollection<Assignment>      Assignments      { get; set; } = [];
    public ICollection<Assignment>      CreatedAssignments { get; set; } = [];
    public ICollection<LifecycleEvent>  LifecycleEvents  { get; set; } = [];
    public ICollection<AuditEntry>      AuditEntries     { get; set; } = [];
}

public class Asset
{
    public int Id { get; set; }
    [MaxLength(50)]  public string AssetCode          { get; set; } = "";
    [MaxLength(50)]  public string AssetTag            { get; set; } = "";
    [MaxLength(100)] public string SerialNumber        { get; set; } = "";
    [MaxLength(30)]  public string AssetType           { get; set; } = "Laptop";
    [MaxLength(80)]  public string Manufacturer        { get; set; } = "";
    [MaxLength(100)] public string Model               { get; set; } = "";
    public int SiteId { get; set; }
    [MaxLength(30)]  public string LifecycleStatus     { get; set; } = "Available";
    [MaxLength(100)] public string Hostname            { get; set; } = "";
    [MaxLength(500)] public string Description         { get; set; } = "";
    [MaxLength(60)]  public string ComplianceId        { get; set; } = "";
    [MaxLength(60)]  public string EncryptionProtocol  { get; set; } = "";
    public DateOnly? LastAudit  { get; set; }
    public DateOnly  CreatedAt  { get; set; }

    public Site                      Site             { get; set; } = null!;
    public ICollection<Assignment>   Assignments      { get; set; } = [];
    public ICollection<LifecycleEvent> LifecycleEvents { get; set; } = [];
}

public class Assignment
{
    public int     Id            { get; set; }
    public int     AssetId       { get; set; }
    public int     UserId        { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? EndDate      { get; set; }
    public int     CreatedById   { get; set; }
    public DateOnly CreatedAt    { get; set; }

    public Asset   Asset     { get; set; } = null!;
    public AppUser User      { get; set; } = null!;
    public AppUser CreatedBy { get; set; } = null!;
}

public class LifecycleEvent
{
    public int     Id          { get; set; }
    public int     AssetId     { get; set; }
    [MaxLength(30)] public string OldStatus { get; set; } = "";
    [MaxLength(30)] public string NewStatus { get; set; } = "";
    [MaxLength(500)] public string Reason   { get; set; } = "";
    public int     ChangedById { get; set; }
    public DateOnly ChangedAt  { get; set; }

    public Asset   Asset     { get; set; } = null!;
    public AppUser ChangedBy { get; set; } = null!;
}

public class License
{
    public int     Id      { get; set; }
    [MaxLength(120)] public string Product { get; set; } = "";
    [MaxLength(80)]  public string Vendor  { get; set; } = "";
    public int     SiteId  { get; set; }
    [MaxLength(30)]  public string Type    { get; set; } = "Per-Seat";
    public int     Total   { get; set; }
    public DateOnly? Start  { get; set; }
    public DateOnly? Expiry { get; set; }
    [MaxLength(20)] public string Status   { get; set; } = "Active";

    public Site Site { get; set; } = null!;
}

public class AuditEntry
{
    public int      Id       { get; set; }
    public DateTime Ts       { get; set; }
    public int?     UserId   { get; set; }
    [MaxLength(60)]  public string Username { get; set; } = "";
    [MaxLength(200)] public string Action   { get; set; } = "";
    [MaxLength(200)] public string Target   { get; set; } = "";

    public AppUser? User { get; set; }
}
