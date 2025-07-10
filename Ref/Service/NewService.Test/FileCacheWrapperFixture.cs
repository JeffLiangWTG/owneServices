using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Models;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test;

[TestFixture]
class FileCacheWrapperFixture
{
	[Test]
	public void GetCachePaths()
	{
		var keyMock1 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock1.Setup(x => x.GetKey()).Returns("1");
		var keyMock2 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock2.Setup(x => x.GetKey()).Returns("2");
		var keyMock3 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock3.Setup(x => x.GetKey()).Returns("3");
		keyMock1.Setup(x => x.CaculateNextKey(It.IsAny<string>())).Returns(keyMock2.Object);
		keyMock2.Setup(x => x.CaculateNextKey(It.IsAny<string>())).Returns(keyMock3.Object);

		var helper = new DataBlockCacheHelper(fileCacheWrapper);
		helper.WriteToCache(new[] { new DummyInt(0),
					new DummyInt(1), new DummyInt(2) }, keyMock1.Object, x => true);
		var results = helper.GetCachePaths(keyMock1.Object, int.MaxValue).ToList();
		Assert.AreEqual(3, results.Count);
		Assert.AreEqual(fileCacheWrapper.GetCachePath("1"), results[0].Item1);
		Assert.AreEqual(fileCacheWrapper.GetCachePath("2"), results[1].Item1);
		Assert.AreEqual(fileCacheWrapper.GetCachePath("3"), results[2].Item1);
		logHelper.Verify(x => x.LogInfo("AA", It.Is<string>(s => s.StartsWith("Cache file exists, key: 1, path: ") && s.Contains("size")), null));

		fileCacheWrapper.GetCachePath("4");
		logHelper.Verify(x => x.LogInfo("AA", "Cache file does not exist, key: 4.", null));
	}

	[Test]
	public void GetData()
	{
		var keyMock = new Mock<IDataBlockKey<DummyInt>>();
		keyMock.Setup(x => x.GetKey()).Returns("1");

		var helper = new DataBlockCacheHelper(fileCacheWrapper);
		helper.WriteToCache(new[] { new DummyInt(0), new DummyInt(1) }, keyMock.Object, x => x.Value == 1);
		CollectionAssert.AreEqual(helper.GetData(keyMock.Object).ToArray(), new[] { new DummyInt(0), new DummyInt(1) });
		logHelper.Verify(x => x.LogInfo("AA", "Getting data cache. Key: 1", null));
	}

	[Test]
	public void GetData_Null()
	{
		var cache = new Mock<IFileCacheWrapper>();
		var key = new Mock<IDataBlockKey<DummyInt>>();
		var helper = new DataBlockCacheHelper(cache.Object);
		Assert.DoesNotThrow(() => helper.GetData(key.Object).ToArray());
	}

	[Test]
	public void GetSequentialKeys()
	{
		var keyMock1 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock1.Setup(x => x.GetKey()).Returns("1");
		var keyMock2 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock2.Setup(x => x.GetKey()).Returns("2");
		var keyMock3 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock3.Setup(x => x.GetKey()).Returns("3");
		keyMock1.Setup(x => x.CaculateNextKey(It.IsAny<string>())).Returns(keyMock2.Object);
		keyMock2.Setup(x => x.CaculateNextKey(It.IsAny<string>())).Returns(keyMock3.Object);

		var helper = new DataBlockCacheHelper(fileCacheWrapper);
		helper.WriteToCache(new[] { new DummyInt(0), new DummyInt(1), new DummyInt(2), new DummyInt(3) }, keyMock1.Object, x => x.Value == 1 || x.Value == 3);
		Assert.That(helper.GetSequentialKeys(keyMock1.Object).Select(x => x.GetKey()).ToArray(), Is.EqualTo(new[] { "1", "2" }));
	}

	[Test]
	public void GetSequentialKeysWithNullData()
	{
		var keyMock1 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock1.Setup(x => x.GetKey()).Returns("1");
		var keyMock2 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock2.Setup(x => x.GetKey()).Returns("2");
		var keyMock3 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock3.Setup(x => x.GetKey()).Returns("3");
		keyMock1.Setup(x => x.CaculateNextKey(It.IsAny<string>())).Returns(keyMock2.Object);
		keyMock2.Setup(x => x.CaculateNextKey(null)).Returns((IDataBlockKey<DummyInt>)null);
		var wrapper = new Mock<IFileCacheWrapper>();
		wrapper.Setup(x => x.Contains("1", null)).Returns(true);
		wrapper.Setup(x => x.Contains("1", DataBlockCacheHelper.CheckpointRegion)).Returns(true);
		wrapper.Setup(x => x.Get("1", null)).Returns(new byte[0]);
		wrapper.Setup(x => x.Get("1", DataBlockCacheHelper.CheckpointRegion)).Returns("1");
		wrapper.Setup(x => x.Contains("2", null)).Returns(true);
		wrapper.Setup(x => x.Contains("2", DataBlockCacheHelper.CheckpointRegion)).Returns(true);
		wrapper.Setup(x => x.Get("2", null)).Returns(null);
		wrapper.Setup(x => x.Get("2", DataBlockCacheHelper.CheckpointRegion)).Returns(null);
		var helper = new DataBlockCacheHelper(wrapper.Object);

		Assert.That(helper.GetSequentialKeys(keyMock1.Object).Select(x => x.GetKey()).ToArray(), Is.EqualTo(new[] { "1", "2" }));
	}

	[Test]
	public void WriteToCache_CheckpointAndDataOutOfSync()
	{
		var keyMock = new Mock<IDataBlockKey<RefDataSet>>();
		keyMock.Setup(x => x.GetKey()).Returns("1");
		var checkpoint = string.Empty;
		var data = string.Empty;
		var noOfCall = 0;
		var wrapper = new Mock<IFileCacheWrapper>();
		wrapper.Setup(x => x.Add("1", It.IsAny<object>(), null)).Callback(() =>
		{
			noOfCall++;
			if (noOfCall == 1)
			{ data = "AAAA"; }
			else
			{ data = "BBBB"; }
		});
		wrapper.Setup(x => x.Add("1", "CK1", "Checkpoint")).Callback(() =>
		{
			checkpoint = "CK1";
		});
		wrapper.Setup(x => x.Add("1", "CK2", "Checkpoint")).Callback(() =>
		{
			throw new Exception("Exception happens");
		});
		wrapper.Setup(x => x.Remove("1", "Checkpoint")).Callback(() => { checkpoint = string.Empty; });
		var helper = new DataBlockCacheHelper(wrapper.Object);
		helper.WriteToCache(new[] { new RefDataSet { Checkpoint = "CK1" } }, keyMock.Object, x => false);
		Assert.AreEqual("AAAA", data);
		Assert.AreEqual("CK1", checkpoint);

		data = string.Empty; // let's say data is cleaned up and we need to populate cache again
		Assert.Throws<Exception>(() => helper.WriteToCache(new[] { new RefDataSet { Checkpoint = "CK2" } }, keyMock.Object, x => false), "2nd time causes exception");
		Assert.AreEqual("BBBB", data);
		Assert.AreNotEqual("CK1", checkpoint, "The checkpoint should not equals to previous data since data has changed");
	}

	[Test]
	public void WriteToCache()
	{
		var keyMock1 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock1.Setup(x => x.GetKey()).Returns("1");
		var keyMock2 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock2.Setup(x => x.GetKey()).Returns("2");
		var keyMock3 = new Mock<IDataBlockKey<DummyInt>>();
		keyMock3.Setup(x => x.GetKey()).Returns("3");
		keyMock1.Setup(x => x.CaculateNextKey(It.IsAny<string>())).Returns(keyMock2.Object);
		keyMock2.Setup(x => x.CaculateNextKey(It.IsAny<string>())).Returns(keyMock3.Object);

		var helper = new DataBlockCacheHelper(fileCacheWrapper);
		helper.WriteToCache(new[] { new DummyInt(0), new DummyInt(1), new DummyInt(2), new DummyInt(3) }, keyMock1.Object, x => x.Value == 1 || x.Value == 3);
		CollectionAssert.AreEqual(helper.GetData(keyMock1.Object).ToArray(), new[] { new DummyInt(0), new DummyInt(1) });
		CollectionAssert.AreEqual(helper.GetData(keyMock2.Object).ToArray(), new[] { new DummyInt(2), new DummyInt(3) });
		Assert.False(fileCacheWrapper.Contains("3"));
		logHelper.Verify(x => x.LogInfo("AA", "Adding data cache. Key: 1", null));
		logHelper.Verify(x => x.LogInfo("AA", "Adding Checkpoint cache. Key: 1", null));
		logHelper.Verify(x => x.LogInfo("AA", "Adding data cache. Key: 2", null));
		logHelper.Verify(x => x.LogInfo("AA", "Adding Checkpoint cache. Key: 2", null));
	}

	[Test]
	public void WatingForWritingToCache()
	{
		var cache = new Mock<IFileCacheWrapper>();
		var containsKey = 0;
		cache.Setup(x => x.Contains("1", null)).Returns(() => containsKey > 0);
		cache.Setup(x => x.Contains("1", DataBlockCacheHelper.CheckpointRegion)).Returns(() => containsKey > 0);
		cache.Setup(x => x.Add("1", It.IsAny<object>(), null)).Callback(() =>
		{
			Thread.Sleep(100);
			containsKey++;
		});

		var keyMock1 = new Mock<IDataBlockKey<int>>();
		keyMock1.Setup(x => x.GetKey()).Returns("1");
		var keyMock2 = new Mock<IDataBlockKey<int>>();
		keyMock2.Setup(x => x.GetKey()).Returns("2");
		var keyMock3 = new Mock<IDataBlockKey<int>>();
		keyMock3.Setup(x => x.GetKey()).Returns("3");
		keyMock1.Setup(x => x.CaculateNextKey(It.IsAny<string>())).Returns(keyMock2.Object);
		keyMock2.Setup(x => x.CaculateNextKey(It.IsAny<string>())).Returns(keyMock3.Object);
		var wrapper = new DataBlockCacheHelper(cache.Object);
		Parallel.For(1, 10, idx => wrapper.WriteToCache(new[] { 0, 1, 2, 3 }, keyMock1.Object, y => y == 3));
		Assert.That(containsKey, Is.EqualTo(1));
	}

	[Test]
	public void WritingToCacheShouldCallShrinkCache()
	{
		var cacheWrapper = new Mock<IFileCacheWrapper>();
		cacheWrapper.Setup(x => x.Contains("1", null)).Returns(() => true);
		var keyMock = new Mock<IDataBlockKey<int>>();
		keyMock.Setup(x => x.GetKey()).Returns("1");
		keyMock.Setup(x => x.CaculateNextKey(It.IsAny<string>())).Returns(new Mock<IDataBlockKey<int>>().Object);
		var wrapper = new DataBlockCacheHelper(cacheWrapper.Object);
		wrapper.WriteToCache(new[] { 0, 1, 2, 3 }, keyMock.Object, y => y == 1);
		cacheWrapper.Verify(x => x.ShrinkCacheSize());

		cacheWrapper.Invocations.Clear();
		cacheWrapper.Setup(x => x.Contains("1", null)).Returns(() => false);
		wrapper = new DataBlockCacheHelper(cacheWrapper.Object);
		wrapper.WriteToCache(new[] { 0, 1, 2, 3 }, keyMock.Object, y => y == 1);
		cacheWrapper.Verify(x => x.ShrinkCacheSize());
	}

	[Test]
	public void FileCacheSlidingAndExpirationTime()
	{
		var expirationTimeSpan = TimeSpan.FromHours(int.Parse(ApplicationConfig.CacheExpirationInHours, CultureInfo.InvariantCulture));
		fileCacheWrapper.Add("1", new DummyInt(1));
		var cachePolicy = fileCacheWrapper.GetCacheItemPolicy("1");
		var absoluteEcpiration = cachePolicy.AbsoluteExpiration;
		Assert.AreEqual(TimeSpan.Zero, cachePolicy.SlidingExpiration);
		Assert.True(absoluteEcpiration <= DateTimeOffset.Now.Add(expirationTimeSpan));

		fileCacheWrapper.Get("1");
		cachePolicy = fileCacheWrapper.GetCacheItemPolicy("1");
		Assert.AreEqual(absoluteEcpiration, cachePolicy.AbsoluteExpiration);
		fileCacheWrapper.Remove("1");
		logHelper.Verify(x => x.LogInfo("AA", "Removing data cache. Key: 1", null));
		fileCacheWrapper.Remove("1", "Checkpoint");
		logHelper.Verify(x => x.LogInfo("AA", "Removing Checkpoint cache. Key: 1", null));
	}

	[Test]
	public void FileCacheHasSingletonSettings()
	{
		var fileCacheWrapper2 = new FileCacheWrapper(logHelper.Object, userService.Object, fileCacheProvider);
		Assert.False(fileCacheWrapper2.Contains("1"));

		fileCacheWrapper.Add("1", new DummyInt(1));
		Assert.True(fileCacheWrapper2.Contains("1"));
	}

	[SetUp]
	public void SetUp()
	{
		ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.NewService.Test.config.json");
		logHelper = new Mock<ILogHelper>();
		userService = new Mock<IUserService>();
		userService.Setup(x => x.GetUserId()).Returns("AA");
		fileCacheProvider = new FileCacheProvider(logHelper.Object);
		tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApplicationConfig.CacheFolder);
		fileCacheWrapper = new FileCacheWrapper(logHelper.Object, userService.Object, fileCacheProvider);
	}

	[TearDown]
	public void TearDown()
	{
		fileCacheProvider.Clear();
		if (Directory.Exists(tempPath))
		{
			Directory.Delete(tempPath, true);
		}
	}

	string tempPath;
	Mock<ILogHelper> logHelper;
	Mock<IUserService> userService;
	IFileCacheProvider fileCacheProvider;
	FileCacheWrapper fileCacheWrapper;
}

[Serializable]
public class DummyInt
{
	public DummyInt(int value) { Value = value; }
	public int Value { get; set; }

	public override bool Equals(object obj)
	{
		return obj != null && obj.GetType() == typeof(DummyInt) && ((DummyInt)obj).Value == Value;
	}
	public override int GetHashCode()
	{
		return Value;
	}
}
