// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace Discounts.API.Middlewares
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

        public async Task Invoke(HttpContext context)
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

        private async Task HandleExceptionAsync(HttpContext context, Exception error)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new { message = error.Message };
            var statusCode = (int)HttpStatusCode.InternalServerError;

            switch (error)
            {
                //not found (404)
                //catches OfferNotFound, CategoryNotFound, UserNotFound
                case NotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    break;

                //bad request (400) - business rules
                //catches OfferNotActive, StockDepleted, InvalidCredentials, UserAlreadyExists
                case BadRequestException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    break;

                //forbidden (403) -security
                //catches UnauthorizedAction
                case UnauthorizedActionException:
                    statusCode = (int)HttpStatusCode.Forbidden;
                    break;

                //system errors (500)
                default:
                    _logger.LogError(error, error.Message);
                    errorResponse = new { message = "An internal error occurred" };
                    break;
            }

            response.StatusCode = statusCode;
            await response.WriteAsync(JsonSerializer.Serialize(errorResponse)).ConfigureAwait(false);
        }
    }
}
