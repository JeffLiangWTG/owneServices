using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CacheTower.Providers.FileSystem;
using CacheTower.Serializers.SystemTextJson;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test;

[TestFixture]
class CacheTowerProviderFixture
{
	[Test]
	public async Task Add_GetAndRemove()
	{
		await cacheProvider.Add("key1", "value1");
		await cacheProvider.Add("key2", "value2");
		Assert.That(cacheProvider.Get<string>("key1").Result, Is.EqualTo("value1"));
		Assert.That(cacheProvider.Get<string>("key2").Result, Is.EqualTo("value2"));
		var files = Directory.GetFiles(cacheRoot).Except([Path.Combine(cacheRoot, "manifest")]);
		Assert.That(files.Count(), Is.EqualTo(2));

		await cacheProvider.Remove("key1");
		await Task.Delay(100);
		Assert.That(cacheProvider.Get<string>("key1").Result, Is.Null);
		files = Directory.GetFiles(cacheRoot).Except([Path.Combine(cacheRoot, "manifest")]);
		Assert.That(files.Count(), Is.EqualTo(1));
	}

	[SetUp]
	public void SetUp()
	{
		ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.NewService.Test.config.json");
		logHelper = new Mock<ILogHelper>();
		cacheRoot = ApplicationConfig.CacheFolder;
		fileCacheLayer =
			new FileCacheLayer(new FileCacheLayerOptions(cacheRoot, SystemTextJsonCacheSerializer.Instance));
		cacheProvider = new CacheTowerProvider(logHelper.Object, fileCacheLayer);
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(cacheRoot))
		{
			Directory.Delete(cacheRoot, true);
		}
	}

	Mock<ILogHelper> logHelper;
	FileCacheLayer fileCacheLayer;
	ICacheTowerProvider cacheProvider;
	string cacheRoot;
}
