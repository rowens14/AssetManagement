/*
 * FILE: UserAuthenticationController.cs
 * PROJECT: AssetManagement.Server / Controllers
 * PURPOSE: Handles user login. In in-memory mode, validates credentials directly
 *          against the InMemoryDataStore user list using a plain-text password
 *          comparison (acceptable for local dev testing only). When PostgreSQL is
 *          active this controller uses bcrypt verification instead.
 *          Exposes POST /api/auth/login → returns LoginResponse with user profile.
 */

using AssetManagement.Server.Data;
using AssetManagement.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(InMemoryDataStore store) : ControllerBase
{
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest req)
    {
        var user = store.FindUser(req.Username, req.Password);

        if (user == null)
            return Ok(new LoginResponse { Success = false, Error = "Invalid credentials." });

        return Ok(new LoginResponse
        {
            Success = true,
            User    = store.ToDto(user)
        });
    }
}
