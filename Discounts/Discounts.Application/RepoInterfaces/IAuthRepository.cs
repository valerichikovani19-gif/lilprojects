// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Domain.Entities;
namespace Discounts.Application.RepoInterfaces
{
    public interface IAuthRepository
    {
        Task<bool> RegisterUserAsync(string email, string password, string firstName, string lastName, string role, CancellationToken cancellationToken);

        // checks credentials
        Task<bool> ValidateUserAsync(string email, string password, CancellationToken cancellationToken);
       //helper
        Task<string> GetUserRoleAsync(string email, CancellationToken cancellationToken);
        Task<string> GetUserIdAsync(string email, CancellationToken cancellationToken);
        Task<List<ApplicationUser>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<bool> BlockUserAsync(string email, CancellationToken cancellationToken);
        Task<bool> UnblockUserAsync(string email, CancellationToken cancellationToken);
        Task<List<ApplicationUser>> GetUsersByIdsAsync(List<string> userIds, CancellationToken cancellationToken);
        Task<ApplicationUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
        Task UpdateRefreshTokenAsync(string email, string refreshToken, DateTime expiryTime, CancellationToken cancellationToken);
    }
}
