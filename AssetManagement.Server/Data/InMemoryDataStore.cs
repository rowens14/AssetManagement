/*
 * FILE: InMemoryDataStore.cs
 * PROJECT: AssetManagement.Server / Data
 * PURPOSE: Development-only in-memory substitute for the PostgreSQL database.
 *          Registered as a singleton in Program.cs when USE_IN_MEMORY_STORE is set.
 *          Holds all seed data in plain C# lists and exposes the same CRUD operations
 *          that the real controllers need — no EF Core, no Npgsql, no connection string.
 *          All data lives only for the lifetime of the server process; changes made
 *          via the API (creates, edits, deletes) are reflected immediately in memory
 *          but are lost when the server restarts.
 *
 *          TO SWITCH BACK TO POSTGRESQL: Set USE_IN_MEMORY_STORE=false in
 *          appsettings.Development.json (or delete the key) and ensure PostgreSQL
 *          is running with the connection string in appsettings.json.
 *
 *          Seed data mirrors DatabaseSeeder.cs exactly so the UI behaves identically.
 */

using AssetManagement.Shared.Models;

namespace AssetManagement.Server.Data;

/// <summary>
/// Singleton in-memory data store. All lists are pre-populated with the same
/// demo seed data as DatabaseSeeder.cs. Thread-safety is not a concern for
/// single-developer local testing.
/// </summary>
public class InMemoryDataStore
{
    // ── Next-ID counters (simulate auto-increment PKs) ────────────────────
    private int _nextAssetId        = 100;
    private int _nextAssignmentId   = 100;
    private int _nextLcEventId      = 100;
    private int _nextLicenseId      = 100;
    private int _nextUserId         = 100;
    private int _nextAuditId        = 100;

    public int NextAssetId()      => _nextAssetId++;
    public int NextAssignmentId() => _nextAssignmentId++;
    public int NextLcEventId()    => _nextLcEventId++;
    public int NextLicenseId()    => _nextLicenseId++;
    public int NextUserId()       => _nextUserId++;
    public int NextAuditId()      => _nextAuditId++;

    // ── Reference data ────────────────────────────────────────────────────
    public List<SiteDto> Sites { get; } =
    [
        new() { Id = 1, Code = "NAP", Name = "Naples"     },
        new() { Id = 2, Code = "FTM", Name = "Fort Myers" },
    ];

    public List<DepartmentDto> Departments { get; } =
    [
        new() { Id = 1, SiteId = 1, SiteName = "Naples",     Name = "Litigation"    },
        new() { Id = 2, SiteId = 1, SiteName = "Naples",     Name = "Real Estate"   },
        new() { Id = 3, SiteId = 1, SiteName = "Naples",     Name = "IT"            },
        new() { Id = 4, SiteId = 2, SiteName = "Fort Myers", Name = "Family Law"    },
        new() { Id = 5, SiteId = 2, SiteName = "Fort Myers", Name = "Corporate"     },
        new() { Id = 6, SiteId = 2, SiteName = "Fort Myers", Name = "Administration"},
    ];

    // ── Users (passwords are plain strings for dev-only in-memory mode) ───
    public List<InMemoryUser> Users { get; } =
    [
        new() { Id=1, Username="admin",    DisplayName="Brandon Campbell", PasswordPlain="admin123", Role="Admin",   SiteId=1, SiteName="Naples",     DeptId=3, DeptName="IT",          IsActive=true },
        new() { Id=2, Username="manager",  DisplayName="Max Owens",        PasswordPlain="pass123",  Role="Manager", SiteId=1, SiteName="Naples",     DeptId=1, DeptName="Litigation",  IsActive=true },
        new() { Id=3, Username="viewer",   DisplayName="Nathan Laws",       PasswordPlain="view123",  Role="Viewer",  SiteId=2, SiteName="Fort Myers", DeptId=4, DeptName="Family Law", IsActive=true },
        new() { Id=4, Username="dbernard", DisplayName="Dave Bernard",      PasswordPlain="pass456",  Role="Manager", SiteId=2, SiteName="Fort Myers", DeptId=5, DeptName="Corporate",  IsActive=true },
        new() { Id=5, Username="jsmith",   DisplayName="Jane Smith",        PasswordPlain="pass789",  Role="Viewer",  SiteId=1, SiteName="Naples",     DeptId=2, DeptName="Real Estate",IsActive=true },
    ];

    // ── Assets ────────────────────────────────────────────────────────────
    public List<AssetDto> Assets { get; } =
    [
        new() { Id=1,  AssetCode="HW-2026-001", AssetTag="TAG-1001", SerialNumber="SN-DELL-X1234",  AssetType="Laptop",  Manufacturer="Dell",   Model="Latitude 5540",       SiteId=1, SiteName="Naples",     LifecycleStatus="Deployed",    Hostname="NAP-LT-001",  Description="Primary workstation",        ComplianceId="CMP-001", EncryptionProtocol="BitLocker", LastAudit=new DateOnly(2025,11,1),  CreatedAt=new DateOnly(2025,1,15)  },
        new() { Id=2,  AssetCode="HW-2026-002", AssetTag="TAG-1002", SerialNumber="SN-APPLE-M9876", AssetType="Laptop",  Manufacturer="Apple",  Model="MacBook Pro 14-inch", SiteId=1, SiteName="Naples",     LifecycleStatus="Available",   Hostname="",            Description="",                           ComplianceId="",        EncryptionProtocol="FileVault",  LastAudit=new DateOnly(2025,10,15), CreatedAt=new DateOnly(2025,2,10)  },
        new() { Id=3,  AssetCode="HW-2026-003", AssetTag="TAG-1003", SerialNumber="SN-HP-M4561",    AssetType="Desktop", Manufacturer="HP",     Model="EliteDesk 800 G9",    SiteId=2, SiteName="Fort Myers", LifecycleStatus="Deployed",    Hostname="FTM-DT-001",  Description="Reception desk",             ComplianceId="CMP-003", EncryptionProtocol="BitLocker",  LastAudit=new DateOnly(2025,9,20),  CreatedAt=new DateOnly(2025,3,1)   },
        new() { Id=4,  AssetCode="HW-2026-004", AssetTag="TAG-1004", SerialNumber="SN-CISCO-R8541", AssetType="Switch",  Manufacturer="Cisco",  Model="Catalyst 2960X",      SiteId=1, SiteName="Naples",     LifecycleStatus="Deployed",    Hostname="NAP-SW-CORE", Description="Core network switch",        ComplianceId="CMP-004", EncryptionProtocol="",           LastAudit=new DateOnly(2025,8,1),   CreatedAt=new DateOnly(2024,12,1)  },
        new() { Id=5,  AssetCode="HW-2026-005", AssetTag="TAG-1005", SerialNumber="SN-CANON-P2233", AssetType="Printer", Manufacturer="Canon",  Model="imageCLASS MF644Cdw", SiteId=2, SiteName="Fort Myers", LifecycleStatus="Maintenance", Hostname="",            Description="Color printer — paper jam",  ComplianceId="",        EncryptionProtocol="",           LastAudit=null,                     CreatedAt=new DateOnly(2024,11,10) },
        new() { Id=6,  AssetCode="HW-2026-006", AssetTag="TAG-1006", SerialNumber="SN-DELL-M7821",  AssetType="Monitor", Manufacturer="Dell",   Model="UltraSharp U2723DE",  SiteId=1, SiteName="Naples",     LifecycleStatus="Available",   Hostname="",            Description="",                           ComplianceId="",        EncryptionProtocol="",           LastAudit=null,                     CreatedAt=new DateOnly(2025,4,1)   },
        new() { Id=7,  AssetCode="HW-2026-007", AssetTag="TAG-1007", SerialNumber="SN-LEN-T1928",   AssetType="Laptop",  Manufacturer="Lenovo", Model="ThinkPad X1 Carbon",  SiteId=2, SiteName="Fort Myers", LifecycleStatus="Retired",     Hostname="FTM-LT-OLD1", Description="End-of-life, 2019 model",    ComplianceId="",        EncryptionProtocol="BitLocker",  LastAudit=new DateOnly(2024,6,1),   CreatedAt=new DateOnly(2019,5,1)   },
        new() { Id=8,  AssetCode="LAW-001",     AssetTag="LAW-001",  SerialNumber="SN123456",       AssetType="Laptop",  Manufacturer="Dell",   Model="Latitude 7420",       SiteId=2, SiteName="Fort Myers", LifecycleStatus="Deployed",    Hostname="",            Description="Migrated from original backup.", ComplianceId="",    EncryptionProtocol="",           LastAudit=null,                     CreatedAt=new DateOnly(2024,1,15)  },
    ];

    // ── Assignments ───────────────────────────────────────────────────────
    public List<AssignmentDto> Assignments { get; } =
    [
        new() { Id=1, AssetId=1, AssetCode="HW-2026-001", AssetModel="Latitude 5540",      UserId=2, UserDisplayName="Max Owens",   UserUsername="manager",  EffectiveDate=new DateOnly(2025,1,20),  EndDate=null,                    CreatedByDisplayName="Brandon Campbell", CreatedAt=new DateOnly(2025,1,20)  },
        new() { Id=2, AssetId=3, AssetCode="HW-2026-003", AssetModel="EliteDesk 800 G9",   UserId=3, UserDisplayName="Nathan Laws",  UserUsername="viewer",   EffectiveDate=new DateOnly(2025,3,5),   EndDate=null,                    CreatedByDisplayName="Brandon Campbell", CreatedAt=new DateOnly(2025,3,5)   },
        new() { Id=3, AssetId=4, AssetCode="HW-2026-004", AssetModel="Catalyst 2960X",     UserId=4, UserDisplayName="Dave Bernard", UserUsername="dbernard", EffectiveDate=new DateOnly(2025,1,1),   EndDate=null,                    CreatedByDisplayName="Brandon Campbell", CreatedAt=new DateOnly(2025,1,1)   },
        new() { Id=4, AssetId=6, AssetCode="HW-2026-006", AssetModel="UltraSharp U2723DE", UserId=5, UserDisplayName="Jane Smith",   UserUsername="jsmith",   EffectiveDate=new DateOnly(2025,4,10),  EndDate=new DateOnly(2025,9,30), CreatedByDisplayName="Brandon Campbell", CreatedAt=new DateOnly(2025,4,10)  },
    ];

    // ── Lifecycle events ──────────────────────────────────────────────────
    public List<LifecycleEventDto> LifecycleEvents { get; } =
    [
        new() { Id=1, AssetId=1, AssetCode="HW-2026-001", OldStatus="Available",  NewStatus="Deployed",    Reason="Assigned to Max Owens - Litigation team",           ChangedByDisplayName="Brandon Campbell", ChangedAt=new DateOnly(2025,1,20)  },
        new() { Id=2, AssetId=5, AssetCode="HW-2026-005", OldStatus="Deployed",   NewStatus="Maintenance", Reason="Paper jam hardware fault — sent for repair",         ChangedByDisplayName="Dave Bernard",     ChangedAt=new DateOnly(2026,1,10)  },
        new() { Id=3, AssetId=7, AssetCode="HW-2026-007", OldStatus="Deployed",   NewStatus="Retired",     Reason="End of support lifecycle — replaced with new unit",  ChangedByDisplayName="Brandon Campbell", ChangedAt=new DateOnly(2025,12,1)  },
        new() { Id=4, AssetId=3, AssetCode="HW-2026-003", OldStatus="Available",  NewStatus="Deployed",    Reason="Deployed to Fort Myers reception",                   ChangedByDisplayName="Dave Bernard",     ChangedAt=new DateOnly(2025,3,5)   },
    ];

    // ── Software licenses ─────────────────────────────────────────────────
    public List<LicenseDto> Licenses { get; } =
    [
        new() { Id=1, Product="Microsoft 365 Business", Vendor="Microsoft", SiteId=1, SiteName="Naples",     Type="Subscription", Total=15, Start=new DateOnly(2025,1,1),  Expiry=new DateOnly(2026,1,1),  Status="Active"  },
        new() { Id=2, Product="Microsoft 365 Business", Vendor="Microsoft", SiteId=2, SiteName="Fort Myers", Type="Subscription", Total=10, Start=new DateOnly(2025,1,1),  Expiry=new DateOnly(2026,1,1),  Status="Active"  },
        new() { Id=3, Product="Adobe Acrobat Pro",      Vendor="Adobe",     SiteId=1, SiteName="Naples",     Type="Per-Seat",     Total=5,  Start=new DateOnly(2024,6,1),  Expiry=new DateOnly(2025,6,1),  Status="Expired" },
        new() { Id=4, Product="Clio Manage",            Vendor="Clio",      SiteId=1, SiteName="Naples",     Type="Enterprise",   Total=20, Start=new DateOnly(2025,3,1),  Expiry=new DateOnly(2026,3,1),  Status="Active"  },
        new() { Id=5, Product="Clio Manage",            Vendor="Clio",      SiteId=2, SiteName="Fort Myers", Type="Enterprise",   Total=15, Start=new DateOnly(2025,3,1),  Expiry=new DateOnly(2026,3,1),  Status="Active"  },
        new() { Id=6, Product="Zoom Meetings",          Vendor="Zoom",      SiteId=2, SiteName="Fort Myers", Type="Volume",       Total=25, Start=new DateOnly(2025,2,15), Expiry=new DateOnly(2026,2,15), Status="Active"  },
    ];

    // ── Audit log ─────────────────────────────────────────────────────────
    public List<AuditEntryDto> AuditEntries { get; } =
    [
        new() { Id=1, Ts=new DateTime(2025,1,15,10,0,0,DateTimeKind.Utc),  Username="admin",    Action="Created asset",            Target="HW-2026-001"                 },
        new() { Id=2, Ts=new DateTime(2025,1,20,9,30,0,DateTimeKind.Utc),  Username="admin",    Action="Assigned asset HW-2026-001", Target="Max Owens"                 },
        new() { Id=3, Ts=new DateTime(2025,3,5,11,15,0,DateTimeKind.Utc),  Username="dbernard", Action="Deployed asset",             Target="HW-2026-003"                },
        new() { Id=4, Ts=new DateTime(2026,1,10,14,22,0,DateTimeKind.Utc), Username="dbernard", Action="Updated lifecycle for",      Target="HW-2026-005 → Maintenance"  },
        new() { Id=5, Ts=new DateTime(2025,12,1,16,45,0,DateTimeKind.Utc), Username="admin",    Action="Retired asset",              Target="HW-2026-007"                },
        new() { Id=6, Ts=new DateTime(2025,4,10,8,10,0,DateTimeKind.Utc),  Username="admin",    Action="Assigned monitor",           Target="HW-2026-006 to Jane Smith"  },
        new() { Id=7, Ts=new DateTime(2025,9,30,17,0,0,DateTimeKind.Utc),  Username="admin",    Action="Returned assignment",        Target="HW-2026-006"                },
    ];

    // ── Helper: find user by credentials (plain password for dev only) ────
    public InMemoryUser? FindUser(string username, string password) =>
        Users.FirstOrDefault(u => u.Username == username &&
                                  u.PasswordPlain == password &&
                                  u.IsActive);

    public InMemoryUser? GetUserById(int id) =>
        Users.FirstOrDefault(u => u.Id == id);

    public AppUserDto ToDto(InMemoryUser u) => new()
    {
        Id          = u.Id,
        Username    = u.Username,
        DisplayName = u.DisplayName,
        Role        = u.Role,
        SiteId      = u.SiteId,
        SiteName    = u.SiteName,
        DeptId      = u.DeptId,
        DeptName    = u.DeptName,
        IsActive    = u.IsActive,
    };
}

/// <summary>
/// Lightweight user record for in-memory mode. Stores the plain-text password
/// so login works without bcrypt. Never used when PostgreSQL is active.
/// </summary>
public class InMemoryUser
{
    public int    Id            { get; set; }
    public string Username      { get; set; } = "";
    public string DisplayName   { get; set; } = "";
    public string PasswordPlain { get; set; } = "";   // dev only — never sent to client
    public string Role          { get; set; } = "Viewer";
    public int    SiteId        { get; set; }
    public string SiteName      { get; set; } = "";
    public int    DeptId        { get; set; }
    public string DeptName      { get; set; } = "";
    public bool   IsActive      { get; set; } = true;
}
