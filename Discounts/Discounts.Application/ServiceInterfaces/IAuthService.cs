// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.DTOs.Admin;
using Discounts.Application.DTOs.Auth;

namespace Discounts.Application.ServiceInterfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
        Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task BlockUserAsync(string email, CancellationToken cancellationToken);
        Task UnblockUserAsync(string email, CancellationToken cancellationToken);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto requestDto, CancellationToken cancellationToken);
        Task CreateUserByAdminAsync(CreateUserDto dto, CancellationToken cancellationToken);
        Task UpdateUserAsync(string email, UpdateUserDto dto, CancellationToken cancellationToken);
        Task DeleteUserAsync(string email, CancellationToken cancellationToken);
    }
}
