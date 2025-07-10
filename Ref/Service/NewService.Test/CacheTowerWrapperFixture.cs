using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheTower.Providers.FileSystem;
using CacheTower.Serializers.SystemTextJson;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test;

[TestFixture]
public class CacheTowerWrapperFixture
{
	[Test]
	public async Task Add_GetAndRemove()
	{
		await cacheTowerWrapper.Add("key1", "value1");
		await cacheTowerWrapper.Add("key2", "value2");
		Assert.That(cacheTowerWrapper.Get<string>("key1").Result, Is.EqualTo("value1"));
		Assert.That(cacheTowerWrapper.Get<string>("key2").Result, Is.EqualTo("value2"));
		var files = Directory.GetFiles(cacheRoot).Except([Path.Combine(cacheRoot, "manifest")]);
		Assert.That(files.Count(), Is.EqualTo(3));

		await cacheTowerWrapper.Remove("key1");
		await Task.Delay(100);
		Assert.That(cacheTowerWrapper.Get<string>("key1").Result, Is.Null);
		files = Directory.GetFiles(cacheRoot).Except([Path.Combine(cacheRoot, "manifest")]);
		Assert.That(files.Count(), Is.EqualTo(2));
	}

	[SetUp]
	public async Task SetUp()
	{
		ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.NewService.Test.config.json");
		logHelper = new Mock<ILogHelper>();
		userService = new Mock<IUserService>();
		userService.Setup(x => x.GetUserId()).Returns("AA");
		cacheRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApplicationConfig.CacheFolder);

		fileCacheLayer =
			new FileCacheLayer(new FileCacheLayerOptions(cacheRoot, SystemTextJsonCacheSerializer.Instance));
		cacheTowerProvider = new CacheTowerProvider(logHelper.Object, fileCacheLayer);
		await cacheTowerProvider.InitializeManifestAsync();
		cacheTowerWrapper = new CacheTowerWrapper(logHelper.Object, userService.Object, cacheTowerProvider);
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(cacheRoot))
		{
			Directory.Delete(cacheRoot, true);
		}
	}

	string cacheRoot;
	Mock<ILogHelper> logHelper;
	Mock<IUserService> userService;
	FileCacheLayer fileCacheLayer;
	CacheTowerProvider cacheTowerProvider;
	ICacheTowerWrapper cacheTowerWrapper;
}
