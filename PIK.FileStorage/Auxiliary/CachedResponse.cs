namespace PIK.FileStorage.Auxiliary;

internal class CachedResponse
{
	public CachedResponse(int statusCode,
		Dictionary<string, string[]> headers,
		byte[] body)
	{
		StatusCode = statusCode;
		Headers = headers;
		Body = body;
	}


	public int StatusCode { get; }

	public Dictionary<string, string[]> Headers { get; }

	public byte[] Body { get; }
}