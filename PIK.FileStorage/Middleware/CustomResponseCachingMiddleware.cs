using PIK.FileStorage.Auxiliary;

namespace PIK.FileStorage.Middleware;

public class CustomResponseCachingMiddleware
{
	private readonly MemoryCache<string, CachedResponse> _cache = new();
	private readonly RequestDelegate _next;


	public CustomResponseCachingMiddleware(RequestDelegate next)
	{
		_next = next;
	}


	public async Task Invoke(HttpContext context)
	{
		if (context.Request.Method == "GET")
			await InvokeWithCaching(context);
		else
			await _next(context);
	}

	private async Task InvokeWithCaching(HttpContext context)
	{
		string requestPath = context.Request.Path.Value!;

		if (!string.IsNullOrEmpty(requestPath) && _cache.TryGet(requestPath, out CachedResponse? cachedResponse))
		{
			await SetResponseFromCacheSettings(context.Response, cachedResponse!);
			return;
		}

		CachedResponse cachingResponse = await GetCachedResponseSettings(context);
		_cache.Set(requestPath, cachingResponse);
	}

	private async Task<CachedResponse> GetCachedResponseSettings(HttpContext context)
	{
		HttpResponse response = context.Response;
		MemoryStream bodyMemoryStream = await GetOriginalResponseBodyMemoryStream(context);

		return new CachedResponse
		(
			response.StatusCode,
			response.Headers
				.ToDictionary(
					header => header.Key,
					header => header.Value.ToArray()),
			bodyMemoryStream.ToArray()
		);
	}

	private async Task<MemoryStream> GetOriginalResponseBodyMemoryStream(HttpContext context)
	{
		Stream originalBodyStream = context.Response.Body;
		await using var memoryStream = new MemoryStream();
		context.Response.Body = memoryStream;

		await _next(context);

		memoryStream.Position = 0;
		await memoryStream.CopyToAsync(originalBodyStream);
		return memoryStream;
	}

	private async Task SetResponseFromCacheSettings(HttpResponse response, CachedResponse cachedResponse)
	{
		response.StatusCode = cachedResponse.StatusCode;

		foreach ((string key, string[] values) in cachedResponse.Headers)
			response.Headers[key] = values;

		await response.BodyWriter.WriteAsync(cachedResponse.Body);
	}
}