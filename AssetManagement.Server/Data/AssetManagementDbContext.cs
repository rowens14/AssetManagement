/*
 * FILE: AssetManagementDbContext.cs
 * PROJECT: AssetManagement.Server / Data
 * PURPOSE: The EF Core database context for the entire application. Defines all eight
 *          DbSet properties (Site, Department, AppUser, Asset, Assignment, LifecycleEvent,
 *          License, AuditEntry) and configures their mappings to the PostgreSQL schema
 *          using the Fluent API in OnModelCreating. Key configuration decisions:
 *          - All table and column names use snake_case (PostgreSQL convention)
 *          - Primary keys use IDENTITY ALWAYS (PostgreSQL integer sequences)
 *          - Foreign keys set ON DELETE RESTRICT to prevent accidental cascades
 *          - Unique indexes enforced on asset_code, asset_tag, serial_number, username
 *          - updated_at column on the asset table is managed by a database trigger
 */

using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Server.Data;

public class AssetManagementDbContext(DbContextOptions<AssetManagementDbContext> options) : DbContext(options)
{
    public DbSet<Site>           Sites           => Set<Site>();
    public DbSet<Department>     Departments     => Set<Department>();
    public DbSet<AppUser>        Users           => Set<AppUser>();
    public DbSet<Asset>          Assets          => Set<Asset>();
    public DbSet<Assignment>     Assignments     => Set<Assignment>();
    public DbSet<LifecycleEvent> LifecycleEvents => Set<LifecycleEvent>();
    public DbSet<License>        Licenses        => Set<License>();
    public DbSet<AuditEntry>     AuditEntries    => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── Sites ────────────────────────────────────────────────────────────
        mb.Entity<Site>(e =>
        {
            e.ToTable("sites");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(10).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ── Departments ───────────────────────────────────────────────────────
        mb.Entity<Department>(e =>
        {
            e.ToTable("departments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            e.Property(x => x.SiteId).HasColumnName("site_id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Site).WithMany(s => s.Departments)
             .HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Users ─────────────────────────────────────────────────────────────
        mb.Entity<AppUser>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            e.Property(x => x.Username).HasColumnName("username").HasMaxLength(60).IsRequired();
            e.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(120).IsRequired();
            e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(256).IsRequired();
            e.Property(x => x.Role).HasColumnName("role").HasMaxLength(20).IsRequired();
            e.Property(x => x.SiteId).HasColumnName("site_id");
            e.Property(x => x.DeptId).HasColumnName("dept_id");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.HasIndex(x => x.Username).IsUnique();
            e.HasOne(x => x.Site).WithMany(s => s.Users)
             .HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Department).WithMany(d => d.Users)
             .HasForeignKey(x => x.DeptId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Assets ────────────────────────────────────────────────────────────
        mb.Entity<Asset>(e =>
        {
            e.ToTable("assets");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            e.Property(x => x.AssetCode).HasColumnName("asset_code").HasMaxLength(50).IsRequired();
            e.Property(x => x.AssetTag).HasColumnName("asset_tag").HasMaxLength(50).IsRequired();
            e.Property(x => x.SerialNumber).HasColumnName("serial_number").HasMaxLength(100).IsRequired();
            e.Property(x => x.AssetType).HasColumnName("asset_type").HasMaxLength(30).IsRequired();
            e.Property(x => x.Manufacturer).HasColumnName("manufacturer").HasMaxLength(80).IsRequired();
            e.Property(x => x.Model).HasColumnName("model").HasMaxLength(100).IsRequired();
            e.Property(x => x.SiteId).HasColumnName("site_id");
            e.Property(x => x.LifecycleStatus).HasColumnName("lifecycle_status").HasMaxLength(30).HasDefaultValue("Available");
            e.Property(x => x.Hostname).HasColumnName("hostname").HasMaxLength(100).HasDefaultValue("");
            e.Property(x => x.Description).HasColumnName("description").HasMaxLength(500).HasDefaultValue("");
            e.Property(x => x.ComplianceId).HasColumnName("compliance_id").HasMaxLength(60).HasDefaultValue("");
            e.Property(x => x.EncryptionProtocol).HasColumnName("encryption_protocol").HasMaxLength(60).HasDefaultValue("");
            e.Property(x => x.LastAudit).HasColumnName("last_audit");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.AssetCode).IsUnique();
            e.HasIndex(x => x.AssetTag).IsUnique();
            e.HasIndex(x => x.SerialNumber).IsUnique();
            e.HasOne(x => x.Site).WithMany(s => s.Assets)
             .HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Assignments ───────────────────────────────────────────────────────
        mb.Entity<Assignment>(e =>
        {
            e.ToTable("assignments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            e.Property(x => x.AssetId).HasColumnName("asset_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.EffectiveDate).HasColumnName("effective_date");
            e.Property(x => x.EndDate).HasColumnName("end_date");
            e.Property(x => x.CreatedById).HasColumnName("created_by_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.Asset).WithMany(a => a.Assignments)
             .HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany(u => u.Assignments)
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CreatedBy).WithMany(u => u.CreatedAssignments)
             .HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
        });

        // ── LifecycleEvents ───────────────────────────────────────────────────
        mb.Entity<LifecycleEvent>(e =>
        {
            e.ToTable("lifecycle_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            e.Property(x => x.AssetId).HasColumnName("asset_id");
            e.Property(x => x.OldStatus).HasColumnName("old_status").HasMaxLength(30).IsRequired();
            e.Property(x => x.NewStatus).HasColumnName("new_status").HasMaxLength(30).IsRequired();
            e.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500).IsRequired();
            e.Property(x => x.ChangedById).HasColumnName("changed_by_id");
            e.Property(x => x.ChangedAt).HasColumnName("changed_at");
            e.HasOne(x => x.Asset).WithMany(a => a.LifecycleEvents)
             .HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ChangedBy).WithMany(u => u.LifecycleEvents)
             .HasForeignKey(x => x.ChangedById).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Licenses ──────────────────────────────────────────────────────────
        mb.Entity<License>(e =>
        {
            e.ToTable("licenses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            e.Property(x => x.Product).HasColumnName("product").HasMaxLength(120).IsRequired();
            e.Property(x => x.Vendor).HasColumnName("vendor").HasMaxLength(80).HasDefaultValue("");
            e.Property(x => x.SiteId).HasColumnName("site_id");
            e.Property(x => x.Type).HasColumnName("type").HasMaxLength(30).HasDefaultValue("Per-Seat");
            e.Property(x => x.Total).HasColumnName("total").HasDefaultValue(0);
            e.Property(x => x.Start).HasColumnName("start");
            e.Property(x => x.Expiry).HasColumnName("expiry");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("Active");
            e.HasOne(x => x.Site).WithMany(s => s.Licenses)
             .HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── AuditEntries ──────────────────────────────────────────────────────
        mb.Entity<AuditEntry>(e =>
        {
            e.ToTable("audit_entries");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            e.Property(x => x.Ts).HasColumnName("ts").HasColumnType("timestamptz");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Username).HasColumnName("username").HasMaxLength(60).IsRequired();
            e.Property(x => x.Action).HasColumnName("action").HasMaxLength(200).IsRequired();
            e.Property(x => x.Target).HasColumnName("target").HasMaxLength(200).HasDefaultValue("");
            e.HasOne(x => x.User).WithMany(u => u.AuditEntries)
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
