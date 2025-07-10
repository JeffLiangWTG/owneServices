using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test
{
	internal class CommonApplicationConfigFixture
	{
		[Test]
		public void GetConfigValue()
		{
			Assert.AreEqual("", CommonApplicationConfig.SecurityProtocols);
			Assert.AreEqual("http://127.0.0.1:4444/wd/hub", CommonApplicationConfig.SeleniumServerURL);
			Assert.AreEqual("180", CommonApplicationConfig.SeleniumTimeOutInSeconds);

			CommonApplicationConfig.AddJsonFile("CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test.config.json");
			Assert.AreEqual("Tls12", CommonApplicationConfig.SecurityProtocols);
			Assert.AreEqual("http://127.0.0.1:4444/wd/hub", CommonApplicationConfig.SeleniumServerURL);
			Assert.AreEqual("180", CommonApplicationConfig.SeleniumTimeOutInSeconds);
		}
	}
}
