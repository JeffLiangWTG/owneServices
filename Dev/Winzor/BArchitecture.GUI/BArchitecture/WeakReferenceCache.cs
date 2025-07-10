using System.Collections.Concurrent;

namespace WinzorFramework;

public class WeakReferenceCache<TKey, TValue> where TKey : class
{
	public bool TryGet(TKey key, out TValue? value)
	{
		return cache.TryGetValue(new WeakReferenceKey<TKey>(key), out value);
	}

	public void Put(TKey key, TValue value)
	{
		PurgeStale();
		cache.TryAdd(new WeakReferenceKey<TKey>(key), value);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Baseline")]
	void PurgeStale()
	{
		foreach (var stale in cache.Where(kp => !kp.Key.IsAlive).ToArray())
		{
			cache.TryRemove(stale);
		}
	}

	public int Count => cache.Count;

	readonly ConcurrentDictionary<WeakReferenceKey<TKey>, TValue> cache = new ConcurrentDictionary<WeakReferenceKey<TKey>, TValue>();

	class WeakReferenceKey<T> where T : class
	{
		public WeakReferenceKey(T key)
		{
			this.key = new WeakReference<T>(key);
			hashCode = key.GetHashCode();
		}

		public override int GetHashCode() => hashCode;

		public override bool Equals(object? obj)
		{
			return obj is WeakReferenceKey<T> objWr
				&& (object.ReferenceEquals(this, objWr)
					|| (objWr.key.TryGetTarget(out var objT) && key.TryGetTarget(out var target) && objT.Equals(target)));
		}

		public bool IsAlive => key.TryGetTarget(out _);

		readonly WeakReference<T> key;
		readonly int hashCode;
	}
}
