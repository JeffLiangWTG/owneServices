using System;
using NUnit.Framework;
using CargoWise.eHub.Nudge;

namespace CargoWise.eHub.Nudge.Tests
{
	[TestFixture]
	public class CacheManagerTest
	{
		[Test]
		public void TestAddNewEntryToCache()
		{
			CacheManager<string, string> cache = InitializeCache();
			string value = cache.GetItemFromCacheOrCreateWhenNotExist("newkey", delegate { return "newvalue"; });
			Assert.AreEqual(value, "newvalue");
			Assert.AreEqual(3, cache.Count);
			Assert.AreEqual(value, "newvalue");
			AssertItemInCache(cache, "a", "a");
			AssertItemInCache(cache, "b", "b");
			AssertItemInCache(cache, "newkey", "newvalue");
		}

		[Test]
		public void TestAddAnExistingEntryToCache()
		{
			CacheManager<string, string> cache = InitializeCache();
			string value = cache.GetItemFromCacheOrCreateWhenNotExist("a", delegate { return "newvalue"; });
			Assert.AreEqual(value, "a");
			Assert.AreEqual(2, cache.Count);
			AssertItemInCache(cache, "a", "a");
			AssertItemInCache(cache, "b", "b");
		}

		[Test]
		public void TestPutAndGetAnEntry()
		{
			CacheManager<string, string> cache = new CacheManager<string, string>();
			cache.PutInCache("keya", "valuea");
			cache.PutInCache("keyb", "valueb");
			Assert.AreEqual(2, cache.Count);
			AssertItemInCache(cache, "keya", "valuea");
			AssertItemInCache(cache, "keyb", "valueb");
		}

		CacheManager<string, string> InitializeCache()
		{
			CacheManager<string, string> cache = new CacheManager<string, string>();
			cache.PutInCache("a", "a");
			cache.PutInCache("b", "b");
			return cache;
		}

		void AssertItemInCache(CacheManager<string, string> cache, string key, string value)
		{
			string v = cache.GetItemFromCache(key);
			Assert.AreEqual(v, value);
		}
	}
}
