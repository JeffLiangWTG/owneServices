using System;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class ExchangeRateUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.CurrenciesFileName = "DEVISES.xml";
			ApplicationConfig.Instance.CurrencyPricesFileName = "COURS_DEVISES.xml";
			var error = Errors.No;
			var generator = new ExchangeRateUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2021, 05, 25), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRExchangeOutputFile);
			Assert.True(File.Exists(outputFileName));
			var expectedFileContent = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.FRExchangeOutputFile));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.AreEqual(expectedFileContent, actualFileContent);
			File.Delete(outputFileName);
		}

		[Test]
		public void TestDataWithZeroRateSkipped()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.CurrenciesFileName = @"DEVISES.xml";
			ApplicationConfig.Instance.CurrencyPricesFileName = @"COURS_DEVISES_WITH_ZERO_RATES.xml";

			var error = Errors.No;
			var generator = new ExchangeRateUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2021, 05, 25), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRExchangeOutputFile);
			Assert.True(File.Exists(outputFileName));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.True(actualFileContent.Contains("<ZZN_RX_NKExCurrency>AFN</ZZN_RX_NKExCurrency>"), "AFN has non zero rate and should be written out.");
			Assert.False(actualFileContent.Contains("<ZZN_RX_NKExCurrency>AED</ZZN_RX_NKExCurrency>"), "AED has zero rate and should be skipped.");
			File.Delete(outputFileName);
		}

		[Test]
		public void TestDataWithInconsistentDatesSkipped()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			ApplicationConfig.Instance.CurrenciesFileName = @"InconsistentDatesTest\DEVISES_FORTEST.xml";
			ApplicationConfig.Instance.CurrencyPricesFileName = @"InconsistentDatesTest\COURS_DEVISES_FORTEST.xml";

			var error = Errors.No;
			var generator = new ExchangeRateUniversalReferenceDataFileGenerator();
			generator.GenerateFiles(new DateTime(2021, 05, 25), ref error);

			var outputFileName = Path.Combine(ApplicationConfig.Instance.OutputDirectory, ApplicationConfig.Instance.FRExchangeOutputFile);
			Assert.True(File.Exists(outputFileName));
			var actualFileContent = File.ReadAllText(outputFileName);
			Assert.True(actualFileContent.Contains("<ZZN_RX_NKExCurrency>CHF</ZZN_RX_NKExCurrency>"), "CHF has valid start date and end date.");
			Assert.False(actualFileContent.Contains("<ZZN_RX_NKExCurrency>USD</ZZN_RX_NKExCurrency>"), "USD's start date is greater than end date.");
			File.Delete(outputFileName);
		}
	}
}
