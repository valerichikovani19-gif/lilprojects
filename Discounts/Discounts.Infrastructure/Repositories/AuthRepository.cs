// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.RepoInterfaces;
using Discounts.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<bool> RegisterUserAsync(string email, string password, string firstName, string lastName, string role, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            //user crt
            var result = await _userManager.CreateAsync(user, password).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            await _userManager.AddToRoleAsync(user, role).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> ValidateUserAsync(string email, string password, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user == null) return false;

            return await _userManager.CheckPasswordAsync(user, password).ConfigureAwait(false);
        }
        public async Task<List<ApplicationUser>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            return await _userManager.Users.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BlockUserAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user == null) return false;

            user.IsBlocked = true;
            await _userManager.UpdateAsync(user).ConfigureAwait(false);
            return true;
        }
        public async Task<bool> UnblockUserAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user == null) return false;

            user.IsBlocked = false;
            await _userManager.UpdateAsync(user).ConfigureAwait(false);
            return true;
        }
        public async Task<string> GetUserRoleAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user == null) return string.Empty;

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            return roles.FirstOrDefault() ?? "Customer";
        }
        public async Task<List<ApplicationUser>> GetUsersByIdsAsync(List<string> userIds, CancellationToken cancellationToken)
        {
            return await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetUserIdAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            return user?.Id ?? string.Empty;
        }
        public async Task<ApplicationUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
        }

        public async Task UpdateRefreshTokenAsync(string email, string refreshToken, DateTime expiryTime, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user != null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryTime;
                await _userManager.UpdateAsync(user).ConfigureAwait(false);
            }
        }
    }
}
