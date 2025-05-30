using Azure.Core;
using IdempotencyDemo.API.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IdempotencyDemo.API.Filters
{
	public class IdempotentBasicFilter : IEndpointFilter
	{
		private readonly ILogger _logger;
		private static readonly ConcurrentDictionary<Guid, IdempotentRequestAttempt> _processedRequests = [];
		private static readonly string[] NoIdempotentMethods = [HttpMethods.Post, HttpMethods.Patch];
		private const int MinutesCleanUpDuration = 60;

		public IdempotentBasicFilter(ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<IdempotentBasicFilter>();
		}

		public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
		{
			var request = context.HttpContext.Request;
			if (!NoIdempotentMethods.Contains(request.Method))
			{
				return await next(context);
			}

			if (!request.Headers.TryGetValue("Idempotency-Key", out var keyValues)
					|| string.IsNullOrWhiteSpace(keyValues))
			{
				return Results.BadRequest("Idempotency-Key header is required.");
			}

			if (!Guid.TryParse(keyValues.ToString(), out var idempotencyKey))
			{
				return Results.BadRequest("Idempotency-Key header is not a valid GUID.");
			}

			var json = "";

			var model = context.Arguments.FirstOrDefault();
			if (model is not null)
			{
				json = JsonSerializer.Serialize(model);
			}

			var hash = ComputeHash(json);

			if (_processedRequests.TryGetValue(idempotencyKey, out IdempotentRequestAttempt? storedRequest) && storedRequest.HashedRequestBody == hash)
			{
				storedRequest.AttempNumber++;
				_processedRequests[idempotencyKey] = storedRequest;

				_logger.LogWarning(
				   "Idempotent request detected. Key: {IdempotencyKey}, Endpoint: {EndpointTarget}, Attempt: {AttemptNumber}, Original Request At: {RequestedAt}",
				   idempotencyKey,
				   storedRequest.EndpointTarget,
				   storedRequest.AttempNumber,
				   storedRequest.RequestedAt
				);

				CleanUp();

				return Results.Conflict("This request has already been processed.");
			}
			else 
			{
				var newRequest = new IdempotentRequestAttempt(hash, request.Path);
				_processedRequests[idempotencyKey] = newRequest;
			}

			CleanUp();

			return await next(context);
		}

		private static string ComputeHash(string input)
		{
			var bytes = Encoding.UTF8.GetBytes(input);
			var hashBytes = SHA256.HashData(bytes);
			return Convert.ToBase64String(hashBytes);
		}

		private static void CleanUp()
		{
			var requestsToCleanUp = _processedRequests.Where(r => r.Value.RequestedAt < DateTime.UtcNow.AddMinutes(-(MinutesCleanUpDuration)));
			foreach (var requestToClean in requestsToCleanUp)
			{
				_processedRequests.TryRemove(requestToClean.Key, out _);
			}
		}
	}
}
