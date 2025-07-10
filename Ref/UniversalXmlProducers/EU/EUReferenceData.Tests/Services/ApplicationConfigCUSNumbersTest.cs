using System.Configuration;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	class ApplicationConfigCUSNumbersTest
	{
		[Test]
		public void CUSNumberSoapServiceNumberOfItemsPerCall_Invalid()
		{
			Assert.Multiple(() =>
			{
				Assert.That(ConfigurationManager.AppSettings["CUSNumberSoapServiceNumberOfItemsPerCall"], Is.EqualTo("ABC"), "Not Int value setup");
				Assert.That(ApplicationConfig.Instance.CUSNumberSoapServiceNumberOfItemsPerCall, Is.EqualTo(10), "Default Value returned");
			});
		}

		[Test]
		public void CUSNumberPagesPerBatch_Invalid()
		{
			Assert.Multiple(() =>
			{
				Assert.That(ConfigurationManager.AppSettings["CUSNumberPagesPerBatch"], Is.EqualTo("DEF"), "Not Int value setup");
				Assert.That(ApplicationConfig.Instance.CUSNumberPagesPerBatch, Is.EqualTo(20), "Default Value returned");
			});
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			ConfigurationManager.AppSettings.Set("CUSNumberSoapServiceNumberOfItemsPerCall", "ABC");
			ConfigurationManager.AppSettings.Set("CUSNumberPagesPerBatch", "DEF");
		}
	}
}
