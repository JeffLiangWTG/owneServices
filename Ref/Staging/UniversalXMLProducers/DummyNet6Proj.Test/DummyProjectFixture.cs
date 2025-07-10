using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.DummyNet6Proj.Test
{
	[TestFixture]
	public class DummyProjectFixture
	{
		[Test]
		public void GetApplicationConfig()
		{
			Assert.AreEqual("http://localhost:47472/", ApplicationConfig.DeliveryServiceBaseUrl);
			Assert.AreEqual("ConfigValueFromRepoForTest", ApplicationConfig.ConfigFromRepoForTest);

			ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.UniversalXMLProducers.DummyNet6Proj.Test.config.json");
			Assert.AreEqual("http://localhost:8080/", ApplicationConfig.DeliveryServiceBaseUrl);
		}
	}
}
