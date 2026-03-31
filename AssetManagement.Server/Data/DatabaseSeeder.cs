/*
 * FILE: DatabaseSeeder.cs
 * PROJECT: AssetManagement.Server / Data
 * PURPOSE: Seeds the PostgreSQL database with initial reference and demonstration data
 *          on first application startup. Called from Program.cs after MigrateAsync().
 *          Only runs when the database is empty (checks Sites table). Seeds:
 *          - 2 office sites: Naples (NAP) and Fort Myers (FTM)
 *          - 6 departments across both sites
 *          - 3 RBAC roles: Admin, Manager, Viewer
 *          - 5 demo user accounts with bcrypt-hashed passwords
 *          - 7 hardware assets with compliance and lifecycle data
 *          - 4 asset assignments and 4 lifecycle transition events
 *          - 6 software licenses (Microsoft 365, Adobe, Clio, Zoom)
 *          - 7 initial audit log entries
 */

using AssetManagement.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Server.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AssetManagementDbContext db)
    {
        // Run any pending migrations first
        await db.Database.MigrateAsync();

        // Only seed if the database is empty
        if (await db.Sites.AnyAsync()) return;

        // ── Sites ────────────────────────────────────────────────────────────
        var naples    = new Site { Code = "NAP", Name = "Naples" };
        var fortMyers = new Site { Code = "FTM", Name = "Fort Myers" };
        db.Sites.AddRange(naples, fortMyers);
        await db.SaveChangesAsync();

        // ── Departments ───────────────────────────────────────────────────────
        var litigation    = new Department { SiteId = naples.Id,    Name = "Litigation" };
        var realEstate    = new Department { SiteId = naples.Id,    Name = "Real Estate" };
        var it            = new Department { SiteId = naples.Id,    Name = "IT" };
        var familyLaw     = new Department { SiteId = fortMyers.Id, Name = "Family Law" };
        var corporate     = new Department { SiteId = fortMyers.Id, Name = "Corporate" };
        var administration = new Department { SiteId = fortMyers.Id, Name = "Administration" };
        db.Departments.AddRange(litigation, realEstate, it, familyLaw, corporate, administration);
        await db.SaveChangesAsync();

        // ── Users (passwords hashed with bcrypt) ──────────────────────────────
        var admin = new AppUser
        {
            Username = "admin", DisplayName = "Brandon Campbell",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin", SiteId = naples.Id, DeptId = it.Id, IsActive = true
        };
        var manager = new AppUser
        {
            Username = "manager", DisplayName = "Max Owens",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"),
            Role = "Manager", SiteId = naples.Id, DeptId = litigation.Id, IsActive = true
        };
        var viewer = new AppUser
        {
            Username = "viewer", DisplayName = "Nathan Laws",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("view123"),
            Role = "Viewer", SiteId = fortMyers.Id, DeptId = familyLaw.Id, IsActive = true
        };
        var dbernard = new AppUser
        {
            Username = "dbernard", DisplayName = "Dave Bernard",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass456"),
            Role = "Manager", SiteId = fortMyers.Id, DeptId = corporate.Id, IsActive = true
        };
        var jsmith = new AppUser
        {
            Username = "jsmith", DisplayName = "Jane Smith",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass789"),
            Role = "Viewer", SiteId = naples.Id, DeptId = realEstate.Id, IsActive = true
        };
        db.Users.AddRange(admin, manager, viewer, dbernard, jsmith);
        await db.SaveChangesAsync();

        // ── Assets ────────────────────────────────────────────────────────────
        var a1 = new Asset { AssetCode="HW-2026-001", AssetTag="TAG-1001", SerialNumber="SN-DELL-X1234",  AssetType="Laptop",  Manufacturer="Dell",   Model="Latitude 5540",           SiteId=naples.Id,    LifecycleStatus="Deployed",    Hostname="NAP-LT-001",  Description="Primary workstation",        ComplianceId="CMP-001", EncryptionProtocol="BitLocker", LastAudit=new DateOnly(2025,11,1),  CreatedAt=new DateOnly(2025,1,15) };
        var a2 = new Asset { AssetCode="HW-2026-002", AssetTag="TAG-1002", SerialNumber="SN-APPLE-M9876", AssetType="Laptop",  Manufacturer="Apple",  Model="MacBook Pro 14\"",         SiteId=naples.Id,    LifecycleStatus="Available",   Hostname="",            Description="",                           ComplianceId="",        EncryptionProtocol="FileVault",  LastAudit=new DateOnly(2025,10,15), CreatedAt=new DateOnly(2025,2,10) };
        var a3 = new Asset { AssetCode="HW-2026-003", AssetTag="TAG-1003", SerialNumber="SN-HP-M4561",    AssetType="Desktop", Manufacturer="HP",     Model="EliteDesk 800 G9",        SiteId=fortMyers.Id, LifecycleStatus="Deployed",    Hostname="FTM-DT-001",  Description="Reception desk",             ComplianceId="CMP-003", EncryptionProtocol="BitLocker",  LastAudit=new DateOnly(2025,9,20),  CreatedAt=new DateOnly(2025,3,1)  };
        var a4 = new Asset { AssetCode="HW-2026-004", AssetTag="TAG-1004", SerialNumber="SN-CISCO-R8541", AssetType="Switch",  Manufacturer="Cisco",  Model="Catalyst 2960X",          SiteId=naples.Id,    LifecycleStatus="Deployed",    Hostname="NAP-SW-CORE", Description="Core network switch",        ComplianceId="CMP-004", EncryptionProtocol="",           LastAudit=new DateOnly(2025,8,1),   CreatedAt=new DateOnly(2024,12,1) };
        var a5 = new Asset { AssetCode="HW-2026-005", AssetTag="TAG-1005", SerialNumber="SN-CANON-P2233", AssetType="Printer", Manufacturer="Canon",  Model="imageCLASS MF644Cdw",     SiteId=fortMyers.Id, LifecycleStatus="Maintenance", Hostname="",            Description="Color printer — paper jam",  ComplianceId="",        EncryptionProtocol="",           LastAudit=null,                     CreatedAt=new DateOnly(2024,11,10)};
        var a6 = new Asset { AssetCode="HW-2026-006", AssetTag="TAG-1006", SerialNumber="SN-DELL-M7821",  AssetType="Monitor", Manufacturer="Dell",   Model="UltraSharp U2723DE",      SiteId=naples.Id,    LifecycleStatus="Available",   Hostname="",            Description="",                           ComplianceId="",        EncryptionProtocol="",           LastAudit=null,                     CreatedAt=new DateOnly(2025,4,1)  };
        var a7 = new Asset { AssetCode="HW-2026-007", AssetTag="TAG-1007", SerialNumber="SN-LEN-T1928",   AssetType="Laptop",  Manufacturer="Lenovo", Model="ThinkPad X1 Carbon",      SiteId=fortMyers.Id, LifecycleStatus="Retired",     Hostname="FTM-LT-OLD1", Description="End-of-life, 2019 model",    ComplianceId="",        EncryptionProtocol="BitLocker",  LastAudit=new DateOnly(2024,6,1),   CreatedAt=new DateOnly(2019,5,1)  };
        db.Assets.AddRange(a1, a2, a3, a4, a5, a6, a7);
        await db.SaveChangesAsync();

        // ── Assignments ───────────────────────────────────────────────────────
        db.Assignments.AddRange(
            new Assignment { AssetId=a1.Id, UserId=manager.Id,  EffectiveDate=new DateOnly(2025,1,20),  EndDate=null,                      CreatedById=admin.Id, CreatedAt=new DateOnly(2025,1,20)  },
            new Assignment { AssetId=a3.Id, UserId=viewer.Id,   EffectiveDate=new DateOnly(2025,3,5),   EndDate=null,                      CreatedById=admin.Id, CreatedAt=new DateOnly(2025,3,5)   },
            new Assignment { AssetId=a4.Id, UserId=dbernard.Id, EffectiveDate=new DateOnly(2025,1,1),   EndDate=null,                      CreatedById=admin.Id, CreatedAt=new DateOnly(2025,1,1)   },
            new Assignment { AssetId=a6.Id, UserId=jsmith.Id,   EffectiveDate=new DateOnly(2025,4,10),  EndDate=new DateOnly(2025,9,30),   CreatedById=admin.Id, CreatedAt=new DateOnly(2025,4,10)  }
        );
        await db.SaveChangesAsync();

        // ── Lifecycle Events ──────────────────────────────────────────────────
        db.LifecycleEvents.AddRange(
            new LifecycleEvent { AssetId=a1.Id, OldStatus="Available",  NewStatus="Deployed",    Reason="Assigned to Max Owens - Litigation team",            ChangedById=admin.Id,    ChangedAt=new DateOnly(2025,1,20)  },
            new LifecycleEvent { AssetId=a5.Id, OldStatus="Deployed",   NewStatus="Maintenance", Reason="Paper jam hardware fault — sent for repair",         ChangedById=dbernard.Id, ChangedAt=new DateOnly(2026,1,10)  },
            new LifecycleEvent { AssetId=a7.Id, OldStatus="Deployed",   NewStatus="Retired",     Reason="End of support lifecycle — replaced with new unit",  ChangedById=admin.Id,    ChangedAt=new DateOnly(2025,12,1)  },
            new LifecycleEvent { AssetId=a3.Id, OldStatus="Available",  NewStatus="Deployed",    Reason="Deployed to Fort Myers reception",                   ChangedById=dbernard.Id, ChangedAt=new DateOnly(2025,3,5)   }
        );
        await db.SaveChangesAsync();

        // ── Licenses ──────────────────────────────────────────────────────────
        db.Licenses.AddRange(
            new License { Product="Microsoft 365 Business",    Vendor="Microsoft", SiteId=naples.Id,    Type="Subscription", Total=15, Start=new DateOnly(2025,1,1),  Expiry=new DateOnly(2026,1,1),  Status="Active"  },
            new License { Product="Microsoft 365 Business",    Vendor="Microsoft", SiteId=fortMyers.Id, Type="Subscription", Total=10, Start=new DateOnly(2025,1,1),  Expiry=new DateOnly(2026,1,1),  Status="Active"  },
            new License { Product="Adobe Acrobat Pro",         Vendor="Adobe",     SiteId=naples.Id,    Type="Per-Seat",     Total=5,  Start=new DateOnly(2024,6,1),  Expiry=new DateOnly(2025,6,1),  Status="Expired" },
            new License { Product="Clio Manage (Legal)",       Vendor="Clio",      SiteId=naples.Id,    Type="Enterprise",   Total=20, Start=new DateOnly(2025,3,1),  Expiry=new DateOnly(2026,3,1),  Status="Active"  },
            new License { Product="Clio Manage (Legal)",       Vendor="Clio",      SiteId=fortMyers.Id, Type="Enterprise",   Total=15, Start=new DateOnly(2025,3,1),  Expiry=new DateOnly(2026,3,1),  Status="Active"  },
            new License { Product="Zoom Meetings",             Vendor="Zoom",      SiteId=fortMyers.Id, Type="Volume",       Total=25, Start=new DateOnly(2025,2,15), Expiry=new DateOnly(2026,2,15), Status="Active"  }
        );
        await db.SaveChangesAsync();

        // ── Audit Log ─────────────────────────────────────────────────────────
        db.AuditEntries.AddRange(
            new AuditEntry { Ts=new DateTime(2025,1,15,10,0,0,DateTimeKind.Utc),  UserId=admin.Id,    Username="admin",    Action="Created asset",            Target="HW-2026-001"                   },
            new AuditEntry { Ts=new DateTime(2025,1,20,9,30,0,DateTimeKind.Utc),  UserId=admin.Id,    Username="admin",    Action="Assigned asset HW-2026-001", Target="Max Owens"                   },
            new AuditEntry { Ts=new DateTime(2025,3,5,11,15,0,DateTimeKind.Utc),  UserId=dbernard.Id, Username="dbernard", Action="Deployed asset",             Target="HW-2026-003"                  },
            new AuditEntry { Ts=new DateTime(2026,1,10,14,22,0,DateTimeKind.Utc), UserId=dbernard.Id, Username="dbernard", Action="Updated lifecycle for",      Target="HW-2026-005 → Maintenance"    },
            new AuditEntry { Ts=new DateTime(2025,12,1,16,45,0,DateTimeKind.Utc), UserId=admin.Id,    Username="admin",    Action="Retired asset",              Target="HW-2026-007"                  },
            new AuditEntry { Ts=new DateTime(2025,4,10,8,10,0,DateTimeKind.Utc),  UserId=admin.Id,    Username="admin",    Action="Assigned monitor",           Target="HW-2026-006 to Jane Smith"    },
            new AuditEntry { Ts=new DateTime(2025,9,30,17,0,0,DateTimeKind.Utc),  UserId=admin.Id,    Username="admin",    Action="Returned assignment",        Target="HW-2026-006"                  }
        );
        await db.SaveChangesAsync();
    }
}
