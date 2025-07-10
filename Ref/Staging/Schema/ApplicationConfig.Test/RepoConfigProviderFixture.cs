using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.ApplicationConfig.Test
{
	[TestFixture]
	public class RepoConfigProviderFixture
	{
		[Test]
		public void GetConfig()
		{
			var provider = new RepoConfigProvider();
			provider.Load();

			Assert.That(provider.TryGet("ConfigFromRepoForTest", out string value));
			Assert.AreEqual("ConfigValueFromRepoForTest", value);
		}
	}
}
