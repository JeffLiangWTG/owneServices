using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.MXReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.MXReferenceData.Tests
{
	[TestFixture]
	public class ExchangeRatesParserTest
	{
		[TestCase(Constants.ExchangeRateTypes.Customs)]
		[TestCase(Constants.ExchangeRateTypes.CustomsExport)]
		public void TestExportToXMLFile(string rateType)
		{
			var path = rateType == Constants.ExchangeRateTypes.Customs ? TestOutputFilePathCUS : TestOutputFilePathCUE;
			using (var expectedStream = TestUtils.GetManifestResourceStream($"CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_MX_{rateType}.xml"))
			using (var inputStream = TestUtils.GetManifestResourceStream($"CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.{ (rateType == Constants.ExchangeRateTypes.Customs ? "Output.CTARC_DEPAIS" : "Input.CTARC_TIPCAM_simplified") }.xml"))
			{
				new ExchangeRatesParser("MX Customs Exchange Rate").ExportToXMLFile(inputStream, path, rateType, new DateTime(2025, 01, 20, 09, 50, 00));
				using (var outputStream = new FileStream(path, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
			}
		}

		[OneTimeTearDown]
		public void TestCleanup()
		{
			File.Delete(TestOutputFilePathCUS);
			File.Delete(TestOutputFilePathCUE);
		}

		readonly string TestOutputFilePathCUS = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Output.Temp.RefExchangeRateZZ_MX_CUS.xml");
		readonly string TestOutputFilePathCUE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Output.Temp.RefExchangeRateZZ_MX_CUE.xml");
	}
}
