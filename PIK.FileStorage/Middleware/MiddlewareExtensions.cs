namespace PIK.FileStorage.Middleware;

public static class MiddlewareExtensions
{
	public static IApplicationBuilder UseCustomResponseCaching(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<CustomResponseCachingMiddleware>();
	}
}