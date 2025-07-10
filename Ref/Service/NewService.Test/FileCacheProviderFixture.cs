using System;
using System.Globalization;
using System.IO;
using System.Runtime.Caching;
using System.Threading;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test;

[TestFixture]
class FileCacheProviderFixture
{
	[Test]
	public void AddAndRemove()
	{
		cacheProvider.Add("key1", "value1");
		cacheProvider.Add("key2", "value2");
		Assert.AreEqual("value1", cacheProvider.Get("key1"));
		Assert.AreEqual("value2", cacheProvider.Get("key2"));
		Assert.True(File.Exists(Path.Combine(cacheRoot, "cache", "key1.dat")));
		Assert.True(File.Exists(Path.Combine(cacheRoot, "cache", "key2.dat")));

		cacheProvider.Remove("key1");
		Sleep();
		Assert.IsNull(cacheProvider.Get("key1"));
		Assert.False(File.Exists(Path.Combine(cacheRoot, "cache", "key1.dat")));
		Assert.True(File.Exists(Path.Combine(cacheRoot, "cache", "key2.dat")));
	}

	[Test]
	public void Clear()
	{
		cacheProvider.Add("key1", "value1");
		cacheProvider.Add("key2", "value2");
		Assert.AreEqual("value1", cacheProvider.Get("key1"));
		Assert.AreEqual("value2", cacheProvider.Get("key2"));
		Assert.True(File.Exists(Path.Combine(cacheRoot, "cache", "key1.dat")));
		Assert.True(File.Exists(Path.Combine(cacheRoot, "cache", "key2.dat")));

		cacheProvider.Clear();
		Sleep();
		Assert.IsNull(cacheProvider.Get("key1"));
		Assert.IsNull(cacheProvider.Get("key2"));
		Assert.False(File.Exists(Path.Combine(cacheRoot, "cache", "key1.dat")));
		Assert.False(File.Exists(Path.Combine(cacheRoot, "cache", "key2.dat")));
	}

	[Test]
	public void CleanExpiredCache()
	{
		cacheProvider.Add("key11", "value1", DateTimeOffset.Now.AddSeconds(3));
		cacheProvider.Add("key12", "value2", DateTimeOffset.Now.AddHours(1));
		cacheProvider.Add("key13", "value3", DateTimeOffset.Now.AddSeconds(3));
		cacheProvider.Add("key14", "value4", DateTimeOffset.Now.AddHours(1));
		Assert.AreEqual("value1", cacheProvider.Get("key11"));
		Assert.AreEqual("value2", cacheProvider.Get("key12"));
		Assert.AreEqual("value3", cacheProvider.Get("key13"));
		Assert.AreEqual("value4", cacheProvider.Get("key14"));
		Assert.True(File.Exists(Path.Combine(cacheRoot, "cache", "key11.dat")));
		Assert.True(File.Exists(Path.Combine(cacheRoot, "cache", "key12.dat")));
		Assert.True(File.Exists(Path.Combine(cacheRoot, "cache", "key13.dat")));
		Assert.True(File.Exists(Path.Combine(cacheRoot, "cache", "key14.dat")));

		Sleep(3000);
		logHelper.Invocations.Clear();
		cacheProvider.CleanExpiredCache();
		Sleep(1000);
		Assert.IsNull(cacheProvider.Get("key11"));
		Assert.NotNull(cacheProvider.Get("key12"));
		Assert.IsNull(cacheProvider.Get("key13"));
		Assert.NotNull(cacheProvider.Get("key14"));
		Assert.False(File.Exists(Path.Combine(cacheRoot, "cache", "key11.dat")));
		Assert.True(File.Exists(Path.Combine(cacheRoot, "cache", "key12.dat")));
		Assert.False(File.Exists(Path.Combine(cacheRoot, "cache", "key13.dat")));
		Assert.True(File.Exists(Path.Combine(cacheRoot, "cache", "key14.dat")));
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Start to clean expired file cache, expiration time: ")), null), Times.Once);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Removed expired file cache, keys: key11, key13")), null), Times.Once);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Clean expired cache finished, total removed size")), null), Times.Once);
		logHelper.Invocations.Clear();
	}

	[Test]
	public void CleanExpiredCacheWithRegionName()
	{
		//clean cache with specific region name
		cacheProvider.Add("key11", "value11", DateTimeOffset.Now.AddSeconds(2), "region11");
		cacheProvider.Add("key12", "value12", DateTimeOffset.Now.AddHours(1), "region11");
		cacheProvider.Add("key13", "value13", DateTimeOffset.Now.AddSeconds(2), "region12");
		Assert.NotNull(cacheProvider.Get("key11", "region11"));
		Assert.NotNull(cacheProvider.Get("key12", "region11"));
		Assert.NotNull(cacheProvider.Get("key13", "region12"));
		Sleep(2000);
		logHelper.Invocations.Clear();

		cacheProvider.CleanExpiredCache("region11");
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Start to clean expired region11 cache, expiration time: ")), null), Times.Once);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Removed expired region11 cache, keys: key11")), null), Times.Once);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("key: key13")), null), Times.Never);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Clean expired cache finished, total removed size")), null), Times.Once);

		logHelper.Invocations.Clear();
		File.Delete(Path.Combine(cacheRoot, "policy\\region12\\key13.policy"));
		var result = cacheProvider.CleanExpiredCache("region12");
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Failed to remove expired region12 cache, key: key13, error: ")), null), Times.Once);
		logHelper.Invocations.Clear();
	}

	[Test]
	public void ShrinkCacheToSize()
	{
		Assert.LessOrEqual(cacheProvider.ShrinkCacheToSize(0), 0);
		cacheProvider.Add("key21", new byte[200]);
		Sleep();
		var size1 = cacheProvider.CurrentCacheSize;
		cacheProvider.Add("key22", new byte[300]);
		Sleep();
		var size2 = cacheProvider.CurrentCacheSize - size1;
		cacheProvider.Add("key23", new byte[400]);
		Sleep();
		var size3 = cacheProvider.CurrentCacheSize - size2 - size1;
		cacheProvider.Add("key24", new byte[500]);
		Sleep();
		var size4 = cacheProvider.CurrentCacheSize - size3 - size2 - size1;

		logHelper.Invocations.Clear();
		// Shrink to the size of the last 3 items (should remove item1 because it's the oldest, keeping the other 3)
		var newSize = cacheProvider.ShrinkCacheToSize(size2 + size3 + size4);
		Sleep(1000);
		Assert.LessOrEqual(newSize, size2 + size3 + size4);
		// Shrink to size 1 (should delete everything)
		newSize = cacheProvider.ShrinkCacheToSize(1);
		Sleep(1000);
		Assert.AreEqual(0, newSize);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Start to shrink cache size, current size: ") && s.Contains("target size: ")), null));
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Removed file cache, keys: key21")), null), Times.Once);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Removed file cache, keys: ") && s.Contains("key22") && s.Contains("key23") && s.Contains("key24")), null), Times.Once);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Shrink cache finished, new cache size: ")), null));

		//put in one test because DAT run tests in parallel and it might fail.
		logHelper.Invocations.Clear();
		cacheProvider.Clear();
		cacheProvider.Add("key21", new byte[100], "Region21");
		cacheProvider.Add("key22", new byte[150], "Region22");
		cacheProvider.Add("key23", new byte[200], "Region22");
		cacheProvider.ShrinkCacheToSize(300, "Region22");
		cacheProvider.ShrinkCacheToSize(10, "Region22");
		Sleep(1000);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("key21")), null), Times.Never);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Removed file cache, keys: Region22\\key22, total size: ")), null), Times.Once);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Removed file cache, keys: Region22\\key23, total size: ")), null), Times.Once);

		logHelper.Invocations.Clear();
		File.Delete(Path.Combine(cacheRoot, "policy\\Region21\\key21.policy"));
		cacheProvider.ShrinkCacheToSize(10, "Region21");
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Failed to get Region21 cache file info, key: key21, error: ")), null), Times.Once);
		logHelper.Invocations.Clear();
	}

	[Test]
	public void SyncAndShrinkCacheSizeAsync()
	{
		var cacheSize = 0L;
		var i = 0;
		var maxCacheSize = long.Parse(ApplicationConfig.CacheMaxSize, CultureInfo.InvariantCulture);
		Assert.AreEqual(1000, maxCacheSize);

		while (cacheSize < 300)
		{
			i++;
			cacheProvider.Add($"key4{i}", new byte[100]);
			cacheSize = cacheProvider.CurrentCacheSize;
		}
		logHelper.Invocations.Clear();
		cacheProvider.ShrinkCacheSizeAsync();
		Sleep();
		Assert.True(cacheProvider.Contains("key41"));
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Start to shrink cache size")), null), Times.Never);
		cacheProvider.Add("key43", new byte[200]);
		cacheProvider.ShrinkCacheSizeAsync();
		Sleep(1000);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Start to shrink cache size")), null), Times.Never);
		cacheProvider.WriteLastCacheSizeSyncFile(DateTime.Now - TimeSpan.FromMinutes(10 + int.Parse(ApplicationConfig.CacheSizeSyncIntervalInMinutes, CultureInfo.InvariantCulture)));
		cacheProvider.Add("key49", new byte[400]);
		var previousSize = cacheProvider.CurrentCacheSize;
		cacheProvider.CurrentCacheSize = 499;
		cacheProvider.ShrinkCacheSizeAsync();
		Sleep(1000);
		Assert.True(cacheProvider.Contains("key41"));
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Start to shrink cache size")), null), Times.Never);

		cacheProvider.CurrentCacheSize = 699;
		cacheProvider.ShrinkCacheSizeAsync();
		cacheProvider.CurrentCacheSize = 0;
		Sleep(10000);
		Assert.True(previousSize == cacheProvider.CurrentCacheSize);
		cacheProvider.ShrinkCacheSizeAsync();
		Sleep(1000);
		Assert.False(cacheProvider.Contains("key41"));
		Assert.LessOrEqual(cacheProvider.CurrentCacheSize, maxCacheSize * 0.75);
		logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Start to shrink cache size")), null), Times.Once);
		logHelper.Invocations.Clear();
	}

	[Test]
	public void LogInfoIfGetCleaningLockThrowException()
	{
		cacheProvider.Add("key111", "value111");
		using (var stream = new FileStream(Path.Combine(cacheRoot, "cache.sem"), FileMode.OpenOrCreate))
		{
			logHelper.Invocations.Clear();
			cacheProvider.CleanExpiredCache();
			Sleep();
			logHelper.Verify(x => x.LogInfo(null, It.Is<string>(s => s.Contains("Failed to get cleaning lock for cache file, error message: ")), null), Times.Once);
		}
	}

	void Sleep(int milliSeconds = 100)
	{
		// Added because it takes time to add/delete cache files and cache provider can't get lock
		Thread.Sleep(milliSeconds);
	}

	void DeleteCacheFolder(string cacheFolder)
	{
		if (Directory.Exists(cacheFolder))
		{
			Directory.Delete(cacheFolder, true);
		}
	}

	[SetUp]
	public void SetUp()
	{
		ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.NewService.Test.config.json");
		logHelper = new Mock<ILogHelper>();
		cacheRoot = TestContext.CurrentContext.Test.MethodName;
		DeleteCacheFolder(cacheRoot);
		FileCache.DefaultCacheManager = FileCacheManagers.Basic;
		cacheProvider = new FileCacheProvider(logHelper.Object, cacheRoot);
	}

	[TearDown]
	public void TearDown()
	{
		DeleteCacheFolder(cacheRoot);
	}

	Mock<ILogHelper> logHelper;
	FileCacheProvider cacheProvider;
	string cacheRoot;
}
