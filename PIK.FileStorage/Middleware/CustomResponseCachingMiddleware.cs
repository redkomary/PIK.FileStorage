namespace PIK.FileStorage.Middleware;

public class CustomResponseCachingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly Dictionary<string, CachedResponse> _cache = new();


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

		if (!string.IsNullOrEmpty(requestPath) && await TryGetResponseFromCache(requestPath, context.Response))
			return;

		MemoryStream originalBodyStream = await GetOriginalResponseBodyMemoryStream(context);
		SaveResponseToCache(requestPath, context.Response, originalBodyStream);
	}

	private void SaveResponseToCache(string responseKey, HttpResponse response, MemoryStream bodyStream)
	{
		var cachedResponse = new CachedResponse
		{
			StatusCode = response.StatusCode,
			Headers = response.Headers
				.ToDictionary(
					header => header.Key,
					header => header.Value.ToArray()),
			Body = bodyStream.ToArray()
		};

		_cache[responseKey] = cachedResponse;
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

	private async Task<bool> TryGetResponseFromCache(string responseKey, HttpResponse response)
	{
		if (!_cache.TryGetValue(responseKey, out CachedResponse? cachedResponse))
			return false;

		response.StatusCode = cachedResponse.StatusCode;

		foreach ((string key, string[] values) in cachedResponse.Headers)
			response.Headers[key] = values;

		await response.BodyWriter.WriteAsync(cachedResponse.Body);

		return true;
	}


	private class CachedResponse
	{
		public int StatusCode { get; init; }

		public Dictionary<string, string[]> Headers { get; init; }

		public byte[] Body { get; init; }
	}
}