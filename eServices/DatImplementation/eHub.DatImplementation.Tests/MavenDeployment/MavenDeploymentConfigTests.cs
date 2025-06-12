using NUnit.Framework;

namespace eHub.DatImplementation.MavenDeployment.Tests
{
	[TestFixture]
	public class MavenDeploymentConfigTests
	{
		[Test]
		public void TestMavenDeploymentConfig()
		{
			var deploymentConfiguration = @"BAT=$(BinPath)\mhaccess\MHAccessGateway.zip;Profile=test;Command=mvn install:install-file deploy";
			var config = MavenDeploymentConfig.Parse(deploymentConfiguration);

			Assert.AreEqual("test", config.SettingsByPrefix("Profile"));
			Assert.AreEqual("$(BinPath)\\mhaccess\\MHAccessGateway.zip", config.SettingsByPrefix("BAT"));
			Assert.AreEqual("mvn install:install-file deploy", config.SettingsByPrefix("Command"));

			deploymentConfiguration = @"BAT=$(BinPath)\mhaccess\MHAccessGateway.zip;Command=mvn install:install-file deploy";
			config = MavenDeploymentConfig.Parse(deploymentConfiguration);

			Assert.That(config.SettingsByPrefix("Profile"), Is.Null);
		}
	}
}
