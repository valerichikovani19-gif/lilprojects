// Copyright (C) TBC Bank. All Rights Reserved.
using Asp.Versioning;
using Discounts.Application.DTOs.Auth;
using Discounts.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Discounts.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [EnableRateLimiting("AuthLimit")] //es chemgan bonusad
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user and returns an authentication token
        /// </summary>
        /// <param name="registerDto">User registration details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Auth response containing JWT token</returns>
        /// <response code="200">Registration successful</response>
        /// <response code="400">Validation failed or user already exists</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken cancellationToken)
        {

            var result = await _authService.RegisterAsync(registerDto, cancellationToken).ConfigureAwait(false);
            return Ok(result);

        }

        /// <summary>
        /// Logs in an existing user and returns an authentication token
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Auth response containing JWT token</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid credentials</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(loginDto, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }
        /// <summary>
        /// Uses a Refresh Token to generate a brand new JWT Access Token
        /// </summary>
        /// <param name="requestDto">The expired access token and the valid refresh token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Auth response containing new JWT and Refresh tokens</returns>
        /// <response code="200">Tokens refreshed successfully</response>
        /// <response code="401">Invalid or expired tokens</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto requestDto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(requestDto, cancellationToken).ConfigureAwait(false);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
