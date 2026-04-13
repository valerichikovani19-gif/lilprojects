// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.RepoInterfaces;
using Discounts.Application.ServiceInterfaces;
using Discounts.Application.Services;
using Discounts.Domain.Entities;
using Discounts.Infrastructure.Persistence;
using Discounts.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.RateLimiting;
using System.Globalization;
var builder = WebApplication.CreateBuilder(args);

//serilog 
builder.Host.UseSerilog((context, services, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.File("Logs/Discounts-Web-Log-.txt",
        rollingInterval: RollingInterval.Day,
        formatProvider: CultureInfo.InvariantCulture);
});

builder.Services.AddControllersWithViews();
//caching
builder.Services.AddMemoryCache();
builder.Services.AddValidatorsFromAssemblyContaining<Discounts.Application.Validators.Offer.CreateOfferDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
//database
//connection resiliency momivida ramdenimejer aserom expliciturad davainvokeb EROF-s
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    }));

//identity configur
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = false;
    }).AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
//DI- repos and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOfferService, OfferService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IGlobalSettingService, GlobalSettingService>();

//automapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
//session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//backgrwork
//builder.Services.AddHostedService<Discounts.Web.BackgroundServices.OfferCleanupWorker>();
builder.Services.AddHostedService<Discounts.Application.BackgroundServices.ReservationCleanupWorker>();
builder.Services.AddHostedService<Discounts.Application.BackgroundServices.OfferExpirationWorker>();

//rate limiter
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(policyName: "AuthLimit", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute( // login pageze wavidet defoltad
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); //defaultad homeze gadasay

using (var scope = app.Services.CreateScope())
{
    //---------------
    //database initializer
    await Discounts.Infrastructure.Persistence.DbInitializer.InitializeAsync(scope.ServiceProvider).ConfigureAwait(false);
    //-------------------
}

try
{
    Log.Information("Starting Web Host . . . ");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated ");
}
finally
{
    Log.CloseAndFlush();
}
