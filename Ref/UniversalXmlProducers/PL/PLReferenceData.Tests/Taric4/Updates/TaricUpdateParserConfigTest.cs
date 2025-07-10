using System.Configuration;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using CargoWise.RefDbRepo.PLReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4;

[TestFixture]
sealed class TaricUpdateParserConfigTest
{
	[Test]
	public void TestTariffUpdateFolder()
	{
		const string expectedValue = "test";

		ConfigurationMock.SetupGet(p => p[Business.Constants.AppSettingsKeys.TariffUpdateFolder]).Returns(expectedValue);
		var result = TaricConfig.TariffUpdateFolder;

		Assert.That(result, Is.EqualTo(expectedValue));
	}

	[Test]
	public void TestTariffUpdateFolderNoSettings()
	{
		Assert.Throws<ConfigurationErrorsException>(() =>
		{
			_ = TaricConfig.TariffUpdateFolder;
		}, "No settings entry");
	}

	[Test]
	public void TestTariffUpdateFolderEmptySettings()
	{
		ConfigurationMock.SetupGet(p => p[Business.Constants.AppSettingsKeys.TariffUpdateFolder]).Returns(string.Empty);
		Assert.Throws<ConfigurationErrorsException>(() =>
		{
			_ = TaricConfig.TariffUpdateFolder;
		}, "Empty settings entry");
	}

	[SetUp]
	public void SetUp()
	{
		ConfigurationMock = new Mock<ISettingsIndexer>();
		TaricConfig = new TaricUpdateParserConfig(ConfigurationMock.Object);
	}

	Mock<ISettingsIndexer> ConfigurationMock;
	TaricUpdateParserConfig TaricConfig;
}
