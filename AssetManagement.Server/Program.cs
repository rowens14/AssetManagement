/*
 * FILE: Program.cs
 * PROJECT: AssetManagement.Server
 * PURPOSE: Application entry point configured for IN-MEMORY mode.
 *          No PostgreSQL, no EF Core, no connection string required.
 *          Registers InMemoryDataStore as a singleton so all controllers share
 *          the same in-memory data for the lifetime of the server process.
 *          All API routes, Blazor WASM hosting, and three-tier architecture
 *          remain identical to the production configuration — only the data
 *          source backing the controllers has changed.
 *
 *          TO RESTORE POSTGRESQL: Replace this file with the PostgreSQL version
 *          of Program.cs (re-add UseNpgsql, MigrateAsync, and DbSeeder calls)
 *          and swap InMemoryDataStore injections back to AssetManagementDbContext.
 */

using AssetManagement.Server.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddRazorPages();

// Register the in-memory data store as a singleton.
// All controllers inject this instead of AssetManagementDbContext.
// Data persists for the lifetime of this server process only.
builder.Services.AddSingleton<InMemoryDataStore>();

// ── App ───────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Logger.LogInformation(
    "AssetManagement.Server running in IN-MEMORY mode. " +
    "No database connection required. Data resets on restart.");

app.Run();
