using IdempotencyDemo.API.DTOs;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IdempotencyDemo.API.Filters
{
	public class IdempotentCachedFilter : IEndpointFilter
	{
		private readonly HybridCache _cache;
		private readonly ILogger _logger;
		private static readonly string[] NoIdempotentMethods = [HttpMethods.Post, HttpMethods.Patch];
		private const int MinutesCacheDuration = 60;

		public IdempotentCachedFilter(HybridCache cache, ILoggerFactory loggerFactory)
		{
			_cache = cache;
			_logger = loggerFactory.CreateLogger<IdempotentCachedFilter>();
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

			var wasCashedBefore = true;
			var hash = ComputeHash(json);
			var cacheKey = $"idempotent:{idempotencyKey}";

			var idempotentRequestAttempt = new IdempotentRequestAttempt(hash, request.Path);

			var cachedRequest = await _cache.GetOrCreateAsync<IdempotentRequestAttempt>(cacheKey, async _ =>
				{
					wasCashedBefore = false;
					return await Task.FromResult(idempotentRequestAttempt);
				}, new HybridCacheEntryOptions
				{
					Expiration = TimeSpan.FromMinutes(MinutesCacheDuration)
				}, tags: ["idempotency"]
			);

			if (wasCashedBefore && cachedRequest.HashedRequestBody == hash)
			{
				cachedRequest.AttempNumber++;

				_logger.LogWarning(
				   "Idempotent request detected. Key: {IdempotencyKey}, Endpoint: {EndpointTarget}, Attempt: {AttemptNumber}, Original Request At: {RequestedAt}",
				   idempotencyKey,
				   cachedRequest.EndpointTarget,
				   cachedRequest.AttempNumber,
				   cachedRequest.RequestedAt
				);

				await _cache.SetAsync(cacheKey, cachedRequest, new HybridCacheEntryOptions
				{
					Expiration = TimeSpan.FromMinutes(MinutesCacheDuration)
				}, tags: ["idempotency"]);

				return Results.Conflict("This request has already been processed.");
			}

			return await next(context);
		}

		private static string ComputeHash(string input)
		{
			var bytes = Encoding.UTF8.GetBytes(input);
			var hashBytes = SHA256.HashData(bytes);
			return Convert.ToBase64String(hashBytes);
		}

	}
}
