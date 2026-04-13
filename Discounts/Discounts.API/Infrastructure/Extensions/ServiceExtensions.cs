// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.BackgroundServices;
using Discounts.Application.RepoInterfaces;
using Discounts.Application.ServiceInterfaces;
using Discounts.Application.Services;
using Discounts.Application.Validators.Offer;
using Discounts.Domain.Entities;
using Discounts.Infrastructure.Persistence;
using Discounts.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

namespace Discounts.API.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            //caching 
            services.AddMemoryCache();
            //db
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            //identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //DI- repos & unitofwork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //DI - services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IGlobalSettingService, GlobalSettingService>();

            //autoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //fluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<CreateOfferDtoValidator>();
            //builder.Services.AddFluentValidationClientsideAdapters();

            //api versioning 
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                //default versia
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                //urlidan wakitxva versiis
                options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
            }).AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = options.GroupNameFormat = "'v'VVV"; //v1, v2
                options.SubstituteApiVersionInUrl = true;
            });
            //ratelimiter(bonusad)
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter(policyName: "AuthLimit", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;//zedmeti requestebi queues nacvlad darejectdes
                });
                //Globaluri politikis shemotanac sheidzleba aqmere
                //...
            });

            // workerebi webshimk
            //services.AddHostedService<ReservationCleanupWorker>();
            //services.AddHostedService<OfferExpirationWorker>();

            //health checks
            services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
            // connection stringit sheamowmebs bazis xelmisawvdomobas
            .AddSqlServer(
                connectionString:configuration.GetConnectionString("DefaultConnection")!,
                name: "db-check",
                timeout: TimeSpan.FromSeconds(5)
            );

            return services;

        }
    }
}

