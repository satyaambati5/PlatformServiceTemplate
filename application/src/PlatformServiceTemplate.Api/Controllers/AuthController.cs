using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using PlatformServiceTemplate.Api.Contracts.Auth;
using PlatformServiceTemplate.Api.Contracts.Common;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Application.Common.Options;
using PlatformServiceTemplate.Domain.Entities;

namespace PlatformServiceTemplate.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenStore refreshTokenStore,
    ICurrentUserService currentUserService,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.RegisterAsync(request.Email, request.Password, cancellationToken);
        if (result.Succeeded)
        {
            logger.LogInformation("Registration succeeded for {Email}. correlation={CorrelationId}", request.Email, HttpContext.TraceIdentifier);
            return Ok(ApiResponse<object?>.Ok(null, "Registration completed.", HttpContext.TraceIdentifier));
        }

        logger.LogWarning(
            "Registration failed for {Email}. errorCount={ErrorCount} errors={Errors} correlation={CorrelationId}",
            request.Email,
            result.Errors.Length,
            result.Errors,
            HttpContext.TraceIdentifier);

        return BadRequest(ApiResponse<object?>.Fail("Registration failed.", HttpContext.TraceIdentifier, result.Errors));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await identityService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (user is null)
        {
            return Unauthorized(ApiResponse<object?>.Fail("Invalid credentials.", HttpContext.TraceIdentifier, "Email or password is incorrect."));
        }

        var accessToken = tokenService.CreateAccessToken(user.Id, user.Email, user.Roles);
        var refreshTokenRaw = tokenService.CreateRefreshToken();
        var refreshHash = tokenService.HashToken(refreshTokenRaw);
        await refreshTokenStore.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshTokenOptions.Value.ExpirationDays)
        }, cancellationToken);
        await refreshTokenStore.SaveChangesAsync(cancellationToken);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        return Ok(ApiResponse<object>.Ok(new
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenRaw,
            AccessTokenExpiresAt = jwt.ValidTo
        }, "Login successful.", HttpContext.TraceIdentifier));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var hash = tokenService.HashToken(request.RefreshToken);
        var current = await refreshTokenStore.GetByHashAsync(hash, cancellationToken);
        if (current is null || current.IsRevoked || current.IsExpired)
        {
            return Unauthorized(ApiResponse<object?>.Fail("Invalid refresh token.", HttpContext.TraceIdentifier, "Token is missing, expired, or revoked."));
        }

        var user = await identityService.GetUserByIdAsync(current.UserId, cancellationToken);
        if (user is null)
        {
            return Unauthorized(ApiResponse<object?>.Fail("Invalid refresh token.", HttpContext.TraceIdentifier, "User not found for token."));
        }

        var newRefreshRaw = tokenService.CreateRefreshToken();
        var newRefreshHash = tokenService.HashToken(newRefreshRaw);
        current.RevokedAt = DateTimeOffset.UtcNow;
        current.ReplacedByTokenHash = newRefreshHash;

        await refreshTokenStore.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshTokenOptions.Value.ExpirationDays)
        }, cancellationToken);

        await refreshTokenStore.SaveChangesAsync(cancellationToken);
        var accessToken = tokenService.CreateAccessToken(user.Id, user.Email, user.Roles);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        return Ok(ApiResponse<object>.Ok(new
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshRaw,
            AccessTokenExpiresAt = jwt.ValidTo
        }, "Token refreshed.", HttpContext.TraceIdentifier));
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(RevokeRequest request, CancellationToken cancellationToken)
    {
        var hash = tokenService.HashToken(request.RefreshToken);
        var current = await refreshTokenStore.GetByHashAsync(hash, cancellationToken);
        if (current is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Refresh token not found.", HttpContext.TraceIdentifier, "No matching token record."));
        }

        if (current.UserId != currentUserService.UserId)
        {
            return Forbid();
        }

        current.RevokedAt = DateTimeOffset.UtcNow;
        await refreshTokenStore.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Token revoked.", HttpContext.TraceIdentifier));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var id = currentUserService.UserId;
        if (id is null)
        {
            return Unauthorized(ApiResponse<object?>.Fail("Unauthorized.", HttpContext.TraceIdentifier, "User context not found."));
        }

        var user = await identityService.GetUserByIdAsync(id.Value, cancellationToken);
        return user is null
            ? NotFound(ApiResponse<object?>.Fail("User not found.", HttpContext.TraceIdentifier, "Authenticated user record does not exist."))
            : Ok(ApiResponse<object>.Ok(user, "Current user fetched.", HttpContext.TraceIdentifier));
    }
}
