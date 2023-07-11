namespace PIK.FileStorage.Auxiliary;

internal class MemoryCache<TKey, TValue>
	where TKey : notnull
{
	private readonly Dictionary<TKey, TValue> _storage = new();


	public bool TryGet(TKey key, out TValue? value)
	{
		return _storage.TryGetValue(key, out value);
	}

	public void Set(TKey key, TValue value, TimeSpan validFor)
	{
		_storage[key] = value;
		RegisterItemDeletion(key, validFor);
	}


	private async Task RegisterItemDeletion(TKey key, TimeSpan delay)
	{
		await Task.Delay(delay);
		_storage.Remove(key);
	}
}