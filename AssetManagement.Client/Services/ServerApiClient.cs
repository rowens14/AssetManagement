/*
 * FILE: ServerApiClient.cs
 * PROJECT: AssetManagement.Client / Services
 * PURPOSE: Typed HTTP client that wraps every REST API endpoint exposed by
 *          AssetManagement.Server. This is the sole point of communication between
 *          the Blazor WASM frontend (Tier 1) and the ASP.NET Core API (Tier 2).
 *          All methods are async and return either a DTO or a (data, errorMessage) tuple.
 *          Uses System.Text.Json with .NET 10's native DateOnly support — no custom
 *          converters required. The X-User-Id header is injected automatically on
 *          requests that need to identify the acting user for audit purposes.
 */

using System.Net.Http.Json;
using System.Text.Json;
using AssetManagement.Shared.Models;

namespace AssetManagement.Client.Services;

public class ServerApiClient(HttpClient http)
{
    // .NET 10: DateOnly supported natively — no custom converters needed
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private int _currentUserId;
    public void SetCurrentUser(int id) => _currentUserId = id;

    private HttpRequestMessage WithUserId(HttpMethod method, string url)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Add("X-User-Id", _currentUserId.ToString());
        return req;
    }

    // ── Auth ──────────────────────────────────────────────────────────────
    public async Task<LoginResponse?> LoginAsync(LoginRequest req)
    {
        var res = await http.PostAsJsonAsync("api/auth/login", req, JsonOpts);
        return await res.Content.ReadFromJsonAsync<LoginResponse>(JsonOpts);
    }

    // ── Reference data ────────────────────────────────────────────────────
    public Task<List<SiteDto>?>       GetSitesAsync()       => http.GetFromJsonAsync<List<SiteDto>>("api/sites", JsonOpts);
    public Task<List<DepartmentDto>?> GetDepartmentsAsync() => http.GetFromJsonAsync<List<DepartmentDto>>("api/departments", JsonOpts);

    // ── Assets ────────────────────────────────────────────────────────────
    public Task<List<AssetDto>?> GetAssetsAsync() => http.GetFromJsonAsync<List<AssetDto>>("api/assets", JsonOpts);

    public async Task<(AssetDto? data, string? error)> CreateAssetAsync(SaveAssetRequest req)
    {
        var res = await http.PostAsJsonAsync("api/assets", req, JsonOpts);
        if (res.IsSuccessStatusCode) return (await res.Content.ReadFromJsonAsync<AssetDto>(JsonOpts), null);
        return (null, await res.Content.ReadAsStringAsync());
    }

    public async Task<(AssetDto? data, string? error)> UpdateAssetAsync(int id, SaveAssetRequest req)
    {
        var res = await http.PutAsJsonAsync($"api/assets/{id}", req, JsonOpts);
        if (res.IsSuccessStatusCode) return (await res.Content.ReadFromJsonAsync<AssetDto>(JsonOpts), null);
        return (null, await res.Content.ReadAsStringAsync());
    }

    public async Task<bool> DeleteAssetAsync(int id)
    {
        var res = await http.DeleteAsync($"api/assets/{id}");
        return res.IsSuccessStatusCode;
    }

    // ── Assignments ───────────────────────────────────────────────────────
    public Task<List<AssignmentDto>?> GetAssignmentsAsync() => http.GetFromJsonAsync<List<AssignmentDto>>("api/assignments", JsonOpts);

    public async Task<(AssignmentDto? data, string? error)> CreateAssignmentAsync(CreateAssignmentRequest req)
    {
        var msg = WithUserId(HttpMethod.Post, "api/assignments");
        msg.Content = JsonContent.Create(req, options: JsonOpts);
        var res = await http.SendAsync(msg);
        if (res.IsSuccessStatusCode) return (await res.Content.ReadFromJsonAsync<AssignmentDto>(JsonOpts), null);
        return (null, await res.Content.ReadAsStringAsync());
    }

    public async Task<bool> ReturnAssignmentAsync(int id, ReturnAssignmentRequest req)
    {
        var res = await http.PutAsJsonAsync($"api/assignments/{id}/return", req, JsonOpts);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAssignmentAsync(int id)
    {
        var res = await http.DeleteAsync($"api/assignments/{id}");
        return res.IsSuccessStatusCode;
    }

    // ── Lifecycle Events ──────────────────────────────────────────────────
    public Task<List<LifecycleEventDto>?> GetLifecycleEventsAsync() => http.GetFromJsonAsync<List<LifecycleEventDto>>("api/lifecycleevents", JsonOpts);

    public async Task<(LifecycleEventDto? data, string? error)> CreateLifecycleEventAsync(CreateLifecycleEventRequest req)
    {
        var msg = WithUserId(HttpMethod.Post, "api/lifecycleevents");
        msg.Content = JsonContent.Create(req, options: JsonOpts);
        var res = await http.SendAsync(msg);
        if (res.IsSuccessStatusCode) return (await res.Content.ReadFromJsonAsync<LifecycleEventDto>(JsonOpts), null);
        return (null, await res.Content.ReadAsStringAsync());
    }

    // ── Licenses ──────────────────────────────────────────────────────────
    public Task<List<LicenseDto>?> GetLicensesAsync() => http.GetFromJsonAsync<List<LicenseDto>>("api/licenses", JsonOpts);

    public async Task<(LicenseDto? data, string? error)> CreateLicenseAsync(SaveLicenseRequest req)
    {
        var res = await http.PostAsJsonAsync("api/licenses", req, JsonOpts);
        if (res.IsSuccessStatusCode) return (await res.Content.ReadFromJsonAsync<LicenseDto>(JsonOpts), null);
        return (null, await res.Content.ReadAsStringAsync());
    }

    public async Task<(LicenseDto? data, string? error)> UpdateLicenseAsync(int id, SaveLicenseRequest req)
    {
        var res = await http.PutAsJsonAsync($"api/licenses/{id}", req, JsonOpts);
        if (res.IsSuccessStatusCode) return (await res.Content.ReadFromJsonAsync<LicenseDto>(JsonOpts), null);
        return (null, await res.Content.ReadAsStringAsync());
    }

    public async Task<bool> DeleteLicenseAsync(int id)
    {
        var res = await http.DeleteAsync($"api/licenses/{id}");
        return res.IsSuccessStatusCode;
    }

    // ── Users ─────────────────────────────────────────────────────────────
    public Task<List<AppUserDto>?> GetUsersAsync() => http.GetFromJsonAsync<List<AppUserDto>>("api/users", JsonOpts);

    public async Task<(AppUserDto? data, string? error)> CreateUserAsync(CreateUserRequest req)
    {
        var res = await http.PostAsJsonAsync("api/users", req, JsonOpts);
        if (res.IsSuccessStatusCode) return (await res.Content.ReadFromJsonAsync<AppUserDto>(JsonOpts), null);
        return (null, await res.Content.ReadAsStringAsync());
    }

    public async Task<bool> ToggleUserAsync(int id)
    {
        var msg = WithUserId(HttpMethod.Put, $"api/users/{id}/toggle");
        var res = await http.SendAsync(msg);
        return res.IsSuccessStatusCode;
    }

    // ── Audit ─────────────────────────────────────────────────────────────
    public Task<List<AuditEntryDto>?> GetAuditAsync() => http.GetFromJsonAsync<List<AuditEntryDto>>("api/audit", JsonOpts);

    public Task AddAuditAsync(string username, string action, string target) =>
        http.PostAsJsonAsync("api/audit", new AuditEntryDto { Username = username, Action = action, Target = target }, JsonOpts);
}
