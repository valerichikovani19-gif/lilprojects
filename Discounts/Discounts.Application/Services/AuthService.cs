// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.DTOs.Admin;
using Discounts.Application.DTOs.Auth;
using Discounts.Application.Exceptions;
using Discounts.Application.RepoInterfaces;
using Discounts.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Identity;
using Discounts.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Discounts.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        //Automapper da microsoft.Extension, Configurationshi arsebul
        //Iconfigs naming conflictis gamo
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager; 
        public AuthService(IUnitOfWork unitOfWork, Microsoft.Extensions.Configuration.IConfiguration configuration,UserManager<ApplicationUser>userManager)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken)
        {
            await _unitOfWork.Auth.RegisterUserAsync(
                registerDto.Email,
                registerDto.Password,
                registerDto.FirstName,
                registerDto.LastName,
                registerDto.Role,
                cancellationToken).ConfigureAwait(false);

            var response = await GenerateJwtToken(registerDto.Email, cancellationToken).ConfigureAwait(false);

            var refreshToken = GenerateRefreshToken();
            await _unitOfWork.Auth.UpdateRefreshTokenAsync(registerDto.Email, refreshToken, DateTime.UtcNow.AddDays(7), cancellationToken).ConfigureAwait(false);

            response.RefreshToken = refreshToken;
            return response;
        }
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken)
        {
            var isValidUser = await _unitOfWork.Auth.ValidateUserAsync(loginDto.Email, loginDto.Password, cancellationToken).ConfigureAwait(false);
            if (!isValidUser) throw new UnauthorizedAccessException("Invalid email or password");

            var response = await GenerateJwtToken(loginDto.Email, cancellationToken).ConfigureAwait(false);

            var refreshToken = GenerateRefreshToken();
            await _unitOfWork.Auth.UpdateRefreshTokenAsync(loginDto.Email, refreshToken, DateTime.UtcNow.AddDays(7), cancellationToken).ConfigureAwait(false);
            
            response.RefreshToken = refreshToken;
            return response;
        }
        //helper
        private async Task<AuthResponseDto> GenerateJwtToken(string email, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Auth.GetUserByEmailAsync(email, cancellationToken).ConfigureAwait(false);
            var role = await _unitOfWork.Auth.GetUserRoleAsync(email, cancellationToken).ConfigureAwait(false);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"]!);

            var key = Encoding.ASCII.GetBytes(secretKey!);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user!.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponseDto
            {
                Token = tokenHandler.WriteToken(token),
                Email = user.Email!,
                Role = role
            };
        }

        public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.Auth.GetAllUsersAsync(cancellationToken).ConfigureAwait(false);
            var userDtos = new List<UserDto>();
            foreach (var u in users)
            {
                var role = await _unitOfWork.Auth.GetUserRoleAsync(u.Email!, cancellationToken).ConfigureAwait(false);
                 userDtos.Add(new UserDto
                {
                    Id = u.Id,
                    Email = u.Email!,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsBlocked = u.IsBlocked,
                    Role = role
                });
            }
            return userDtos;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token signature");
            }

            return principal;
        }
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto requestDto, CancellationToken cancellationToken)
        {
            var principal = GetPrincipalFromExpiredToken(requestDto.AccessToken);
            if (principal == null) throw new UnauthorizedAccessException("Invalid access token");

            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (email == null) throw new UnauthorizedAccessException("Token does not contain an email payload");

            var user = await _unitOfWork.Auth.GetUserByEmailAsync(email, cancellationToken).ConfigureAwait(false);

            if (user == null || user.RefreshToken != requestDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var response = await GenerateJwtToken(email, cancellationToken).ConfigureAwait(false);
            var newRefreshToken = GenerateRefreshToken();
            await _unitOfWork.Auth.UpdateRefreshTokenAsync(email, newRefreshToken, DateTime.UtcNow.AddDays(7), cancellationToken).ConfigureAwait(false);

            response.RefreshToken = newRefreshToken;
            return response;
        }
        public async Task BlockUserAsync(string email, CancellationToken cancellationToken)
        {
            var success = await _unitOfWork.Auth.BlockUserAsync(email, cancellationToken).ConfigureAwait(false);
            if (!success) throw new UserNotFoundException(email);
        }
        public async Task UnblockUserAsync(string email, CancellationToken cancellationToken)
        {
            var success = await _unitOfWork.Auth.UnblockUserAsync(email, cancellationToken).ConfigureAwait(false);
            if (!success) throw new UserNotFoundException(email);
        }

        public async Task CreateUserByAdminAsync(CreateUserDto dto, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new UserOperationException(errors);
            }

            var role = dto.Role == "Merchant" ? "Merchant" : "Customer";
            var roleResult = await _userManager.AddToRoleAsync(user, role).ConfigureAwait(false);

            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description);
                throw new UserOperationException(errors);
            }
        }

        public async Task UpdateUserAsync(string email, UpdateUserDto dto, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user == null)
            {
                throw new UserNotFoundException(email);
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;

            var result = await _userManager.UpdateAsync(user).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new UserOperationException(errors);
            }
        }

        public async Task DeleteUserAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user == null)
            {
                throw new UserNotFoundException(email);
            }
            var hasAttachedOffers = await _unitOfWork.Offers.ExistsAsync(o => o.MerchantId == user.Id, cancellationToken).ConfigureAwait(false);
            if (hasAttachedOffers)
            {
                 throw new UserOperationException(new List<string>{
            "Cannot delete this Merchant because they have existing offers"
            });
            }
            var result = await _userManager.DeleteAsync(user).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new UserOperationException(errors);
            }
        }
    }
}
