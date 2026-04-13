// Copyright (C) TBC Bank. All Rights Reserved.
using System.Globalization;
using Discounts.API.Infrastructure.Extensions;
using Discounts.API.Infrastructure.Extensions.JWT;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        .WriteTo.File("Logs/Discounts-API-log-.txt",
            rollingInterval: RollingInterval.Day,
            formatProvider: CultureInfo.InvariantCulture);
});

builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicationServices(builder.Configuration);
//2jwt
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.UseMiddleware<Discounts.API.Middlewares.GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health"); //gassatestad ->https://localhost:[port]/health browsershi 

app.MapControllers();
try
{
    Log.Information("Starting Web Host...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexcpectedly");
}
finally
{
    Log.CloseAndFlush();
}
