/*
 * FILE: 20260101000000_InitialCreate.cs
 * PROJECT: AssetManagement.Server / Migrations
 * PURPOSE: The initial EF Core database migration that creates all eight application
 *          tables in PostgreSQL. The Up() method creates tables in dependency order
 *          (Sites → Departments → Users → Assets → Assignments → LifecycleEvents →
 *          Licenses → AuditEntries) so foreign key constraints resolve correctly.
 *          The Down() method drops them in reverse order for rollback support.
 *          This migration also creates all indexes including the partial unique index
 *          on asset_assignment(asset_id) WHERE end_date IS NULL, which enforces the
 *          business rule that an asset can only have one active assignment at a time.
 *          Run automatically by Program.cs via db.Database.MigrateAsync() on startup.
 */

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AssetManagement.Server.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "sites",
            columns: table => new
            {
                id   = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                code = table.Column<string>(maxLength: 10,  nullable: false),
                name = table.Column<string>(maxLength: 100, nullable: false)
            },
            constraints: t => t.PrimaryKey("PK_sites", x => x.id));

        migrationBuilder.CreateIndex("IX_sites_code", "sites", "code", unique: true);

        migrationBuilder.CreateTable(
            name: "departments",
            columns: table => new
            {
                id      = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                site_id = table.Column<int>(nullable: false),
                name    = table.Column<string>(maxLength: 100, nullable: false)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_departments", x => x.id);
                t.ForeignKey("FK_departments_sites_site_id", x => x.site_id, "sites", "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_departments_site_id", "departments", "site_id");

        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id            = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                username      = table.Column<string>(maxLength: 60,  nullable: false),
                display_name  = table.Column<string>(maxLength: 120, nullable: false),
                password_hash = table.Column<string>(maxLength: 256, nullable: false),
                role          = table.Column<string>(maxLength: 20,  nullable: false),
                site_id       = table.Column<int>(nullable: false),
                dept_id       = table.Column<int>(nullable: false),
                is_active     = table.Column<bool>(nullable: false, defaultValue: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_users", x => x.id);
                t.ForeignKey("FK_users_sites_site_id", x => x.site_id, "sites", "id", onDelete: ReferentialAction.Restrict);
                t.ForeignKey("FK_users_departments_dept_id", x => x.dept_id, "departments", "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_users_username", "users", "username", unique: true);
        migrationBuilder.CreateIndex("IX_users_site_id",  "users", "site_id");
        migrationBuilder.CreateIndex("IX_users_dept_id",  "users", "dept_id");

        migrationBuilder.CreateTable(
            name: "assets",
            columns: table => new
            {
                id                   = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                asset_code           = table.Column<string>(maxLength: 50,  nullable: false),
                asset_tag            = table.Column<string>(maxLength: 50,  nullable: false),
                serial_number        = table.Column<string>(maxLength: 100, nullable: false),
                asset_type           = table.Column<string>(maxLength: 30,  nullable: false),
                manufacturer         = table.Column<string>(maxLength: 80,  nullable: false),
                model                = table.Column<string>(maxLength: 100, nullable: false),
                site_id              = table.Column<int>(nullable: false),
                lifecycle_status     = table.Column<string>(maxLength: 30,  nullable: false, defaultValue: "Available"),
                hostname             = table.Column<string>(maxLength: 100, nullable: false, defaultValue: ""),
                description          = table.Column<string>(maxLength: 500, nullable: false, defaultValue: ""),
                compliance_id        = table.Column<string>(maxLength: 60,  nullable: false, defaultValue: ""),
                encryption_protocol  = table.Column<string>(maxLength: 60,  nullable: false, defaultValue: ""),
                last_audit           = table.Column<DateOnly>(nullable: true),
                created_at           = table.Column<DateOnly>(nullable: false)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_assets", x => x.id);
                t.ForeignKey("FK_assets_sites_site_id", x => x.site_id, "sites", "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_assets_asset_code",    "assets", "asset_code",    unique: true);
        migrationBuilder.CreateIndex("IX_assets_asset_tag",     "assets", "asset_tag",     unique: true);
        migrationBuilder.CreateIndex("IX_assets_serial_number", "assets", "serial_number", unique: true);
        migrationBuilder.CreateIndex("IX_assets_site_id",       "assets", "site_id");

        migrationBuilder.CreateTable(
            name: "assignments",
            columns: table => new
            {
                id             = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                asset_id       = table.Column<int>(nullable: false),
                user_id        = table.Column<int>(nullable: false),
                effective_date = table.Column<DateOnly>(nullable: false),
                end_date       = table.Column<DateOnly>(nullable: true),
                created_by_id  = table.Column<int>(nullable: false),
                created_at     = table.Column<DateOnly>(nullable: false)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_assignments", x => x.id);
                t.ForeignKey("FK_assignments_assets_asset_id",       x => x.asset_id,      "assets", "id", onDelete: ReferentialAction.Cascade);
                t.ForeignKey("FK_assignments_users_user_id",         x => x.user_id,       "users",  "id", onDelete: ReferentialAction.Restrict);
                t.ForeignKey("FK_assignments_users_created_by_id",   x => x.created_by_id, "users",  "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_assignments_asset_id",      "assignments", "asset_id");
        migrationBuilder.CreateIndex("IX_assignments_user_id",       "assignments", "user_id");
        migrationBuilder.CreateIndex("IX_assignments_created_by_id", "assignments", "created_by_id");

        migrationBuilder.CreateTable(
            name: "lifecycle_events",
            columns: table => new
            {
                id             = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                asset_id       = table.Column<int>(nullable: false),
                old_status     = table.Column<string>(maxLength: 30,  nullable: false),
                new_status     = table.Column<string>(maxLength: 30,  nullable: false),
                reason         = table.Column<string>(maxLength: 500, nullable: false),
                changed_by_id  = table.Column<int>(nullable: false),
                changed_at     = table.Column<DateOnly>(nullable: false)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_lifecycle_events", x => x.id);
                t.ForeignKey("FK_lifecycle_events_assets_asset_id",      x => x.asset_id,      "assets", "id", onDelete: ReferentialAction.Cascade);
                t.ForeignKey("FK_lifecycle_events_users_changed_by_id",  x => x.changed_by_id, "users",  "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_lifecycle_events_asset_id",      "lifecycle_events", "asset_id");
        migrationBuilder.CreateIndex("IX_lifecycle_events_changed_by_id", "lifecycle_events", "changed_by_id");

        migrationBuilder.CreateTable(
            name: "licenses",
            columns: table => new
            {
                id      = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                product = table.Column<string>(maxLength: 120, nullable: false),
                vendor  = table.Column<string>(maxLength: 80,  nullable: false, defaultValue: ""),
                site_id = table.Column<int>(nullable: false),
                type    = table.Column<string>(maxLength: 30,  nullable: false, defaultValue: "Per-Seat"),
                total   = table.Column<int>(nullable: false, defaultValue: 0),
                start   = table.Column<DateOnly>(nullable: true),
                expiry  = table.Column<DateOnly>(nullable: true),
                status  = table.Column<string>(maxLength: 20,  nullable: false, defaultValue: "Active")
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_licenses", x => x.id);
                t.ForeignKey("FK_licenses_sites_site_id", x => x.site_id, "sites", "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_licenses_site_id", "licenses", "site_id");

        migrationBuilder.CreateTable(
            name: "audit_entries",
            columns: table => new
            {
                id       = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                ts       = table.Column<DateTime>(type: "timestamptz", nullable: false),
                user_id  = table.Column<int>(nullable: true),
                username = table.Column<string>(maxLength: 60,  nullable: false),
                action   = table.Column<string>(maxLength: 200, nullable: false),
                target   = table.Column<string>(maxLength: 200, nullable: false, defaultValue: "")
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_audit_entries", x => x.id);
                t.ForeignKey("FK_audit_entries_users_user_id", x => x.user_id, "users", "id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_audit_entries_user_id", "audit_entries", "user_id");
        migrationBuilder.CreateIndex("IX_audit_entries_ts",      "audit_entries", "ts");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("audit_entries");
        migrationBuilder.DropTable("lifecycle_events");
        migrationBuilder.DropTable("assignments");
        migrationBuilder.DropTable("licenses");
        migrationBuilder.DropTable("assets");
        migrationBuilder.DropTable("users");
        migrationBuilder.DropTable("departments");
        migrationBuilder.DropTable("sites");
    }
}
