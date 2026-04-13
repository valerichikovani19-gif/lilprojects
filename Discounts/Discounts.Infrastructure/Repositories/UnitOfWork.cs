// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Domain.Entities;
using Discounts.Application.RepoInterfaces;
using Discounts.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Discounts.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UnitOfWork(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;

            //initing repos
            Categories = new CategoryRepository(_context);
            Offers = new OfferRepository(_context);
            Coupons = new CouponRepository(_context);
            Auth = new AuthRepository(_userManager, _roleManager);
            GlobalSettings = new GlobalSettingRepository(_context);
        }

        public ICategoryRepository Categories { get; private set; }
        public IOfferRepository Offers { get; private set; }
        public ICouponRepository Coupons { get; private set; }
        public IAuthRepository Auth { get; private set; }
        public IGlobalSettingRepository GlobalSettings { get; private set; }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
