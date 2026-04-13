// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.Exceptions;
using System.Net;

namespace Discounts.Web.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = (int)HttpStatusCode.InternalServerError;
            switch (ex)
            {
                case NotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    break;
                case BadRequestException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case UnauthorizedActionException:
                    statusCode = (int)HttpStatusCode.Forbidden;
                    break;
                default:_logger.LogError(ex, ex.Message);
                    break;
            }
            context.Items["ErrorMessage"] = ex.Message;
            context.Response.Redirect($"/Home/Error?statusCode={statusCode}");
            return Task.CompletedTask;

        }
    }
}
