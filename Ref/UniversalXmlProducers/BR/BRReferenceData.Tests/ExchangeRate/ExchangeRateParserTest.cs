using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class ExchangeRateParserTest
	{
		[Test]
		public void TestExportToXMLFile_CUS()
		{
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.ExchangeRate.TestFiles.Output.RefExchangeRateZZ_BR_CUS.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.ExchangeRate.TestFiles.Input.cotacaoTodasAsMoedas_10112020.csv"))
			{
				new ExchangeRateParser("BR Customs Exchange Rate").ExportToXMLFile(inputStream, TestOutputFilePathCUS, Constants.ExchangeRateTypes.Customs, new DateTime(2020, 11, 10, 09, 50, 00));
				using (var outputStream = new FileStream(TestOutputFilePathCUS, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
			}
		}

		[Test]
		public void TestExportToXMLFile_CUE()
		{
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.ExchangeRate.TestFiles.Output.RefExchangeRateZZ_BR_CUE.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.ExchangeRate.TestFiles.Input.cotacaoTodasAsMoedas_10112020.csv"))
			{
				new ExchangeRateParser("BR Customs Exchange Rate").ExportToXMLFile(inputStream, TestOutputFilePathCUE, Constants.ExchangeRateTypes.CustomsExport, new DateTime(2020, 11, 10, 09, 50, 00));
				using (var outputStream = new FileStream(TestOutputFilePathCUE, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
			}
		}

		[TearDown]
		public void TestCleanup()
		{
			File.Delete(TestOutputFilePathCUE);
			File.Delete(TestOutputFilePathCUS);
		}


		readonly string TestOutputFilePathCUS = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.ExchangeRate.TestFiles.Output.Temp.RefExchangeRateZZ_BR_CUS.xml");
		readonly string TestOutputFilePathCUE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.ExchangeRate.TestFiles.Output.Temp.RefExchangeRateZZ_BR_CUE.xml");
	}
}
