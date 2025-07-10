using System.Configuration;
using CargoWise.RefDbRepo.PLReferenceData.Business;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService;
using CargoWise.RefDbRepo.PLReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Message;

[TestFixture]
sealed class PuescServiceConfigurationTest
{
	[Test]
	public void TestLogin()
	{
		const string appSettingsKey = Constants.AppSettingsKeys.PUESCLogin;

		Assert.Multiple(() =>
		{
			Assert.Throws<ConfigurationErrorsException>(() => { _ = Configuration.Login; }, "No app settings entry");

			SettingsMock.SetupGet(p => p[appSettingsKey]).Returns(string.Empty);
			Assert.Throws<ConfigurationErrorsException>(() => { _ = Configuration.Login; }, "Empty app settings entry");

			const string expectedValue = "Login";
			SettingsMock.SetupGet(p => p[appSettingsKey]).Returns(expectedValue);
			Assert.AreEqual(expectedValue, Configuration.Login, "Entry with value");
		});
	}

	[Test]
	public void TestPassword()
	{
		const string appSettingsKey = Constants.AppSettingsKeys.PUESCPassword;

		Assert.Multiple(() =>
		{
			Assert.Throws<ConfigurationErrorsException>(() => { _ = Configuration.Password; }, "No app settings entry");

			SettingsMock.SetupGet(p => p[appSettingsKey]).Returns(string.Empty);
			Assert.Throws<ConfigurationErrorsException>(() => { _ = Configuration.Password; }, "Empty app settings entry");

			const string expectedValue = "password";
			const string encryptedValue = "cDFhMnMzczR3NW82cjdk";
			SettingsMock.SetupGet(p => p[appSettingsKey]).Returns(encryptedValue);
			Assert.AreEqual(expectedValue, Configuration.Password, "Entry with value");
		});
	}

	[Test]
	public void TestUrl()
	{
		const string appSettingsKey = Constants.AppSettingsKeys.PUESCUrl;

		Assert.Multiple(() =>
		{
			Assert.Throws<ConfigurationErrorsException>(() => { _ = Configuration.Url; }, "No app settings entry");

			SettingsMock.SetupGet(p => p[appSettingsKey]).Returns(string.Empty);
			Assert.Throws<ConfigurationErrorsException>(() => { _ = Configuration.Url; }, "Empty app settings entry");

			const string expectedValue = "https://ws.puesc.gov.pl/seap_wsChannel/DocumentHandlingPort";
			SettingsMock.SetupGet(p => p[appSettingsKey]).Returns(expectedValue);
			Assert.AreEqual(expectedValue, Configuration.Url, "Entry with value");
		});
	}

	[SetUp]
	public void SetUp()
	{
		SettingsMock = new Mock<ISettingsIndexer>();
		Configuration = new PuescServiceConfiguration(SettingsMock.Object);
	}

	Mock<ISettingsIndexer> SettingsMock;
	PuescServiceConfiguration Configuration;
}
