using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace WinzorFramework;

class WeakReferenceCacheTest
{
	[Test]
	public void TryGetWithNoMatch()
	{
		var cache = new WeakReferenceCache<object, string>();
		Assert.That(cache.TryGet(new object(), out var value), Is.False);
	}

	[Test]
	public void TryGetWithMatch()
	{
		var cache = new WeakReferenceCache<object, string>();
		var x = new object();
		cache.Put(x, "foo");
		Assert.That(cache.TryGet(x, out var value));
		Assert.That(value, Is.EqualTo("foo"));
	}

	[Test]
	public void CacheIsThreadSafe()
	{
		var cache = new WeakReferenceCache<object, string>();
		Parallel.For(0, 10, i =>
		{
			var x = new object();
			cache.Put(x, i.ToString());
			Assert.That(cache.TryGet(x, out var value), Is.True);
			Assert.That(value, Is.EqualTo(i.ToString()));
		});
	}

	[Test]
	public void CachePurgedWhenObjectIsCollected()
	{
		var cache = new WeakReferenceCache<object, string>();
		AddToCache(cache);
		Assert.That(cache.Count, Is.EqualTo(1));
		AddToCache(cache);
		Assert.That(cache.Count, Is.EqualTo(2));
		AddToCache(cache);
		Assert.That(cache.Count, Is.EqualTo(3));
		GC.Collect();
		AddToCache(cache);
		Assert.That(cache.Count, Is.EqualTo(1));
	}

	void AddToCache(WeakReferenceCache<object, string> cache)
	{
		var obj = new object();
		cache.Put(obj, "whatever");
	}
}
