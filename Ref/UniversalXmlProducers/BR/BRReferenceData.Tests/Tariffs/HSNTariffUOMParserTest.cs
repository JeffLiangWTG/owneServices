using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class HSNTariffUOMParserTest
	{
		[Test]
		public void TestConvertCustomsTariffUnityOfMeasureToRefDataRepoXml()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariff_BR_HSN_UOM.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NotaFiscalEletronica.Table_NCM.xlsx"))
			{
				var parser = new HSNTariffUOMParser("BR HSN Tariff UOM");
				parser.ExportToXMLFile(inputStream, TestOutputFilePath, new DateTime(2020, 10, 14, 09, 50, 00));

				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
			}
		}

		[TearDown]
		public void TestCleanup()
		{
			File.Delete(TestOutputFilePath);
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@"TariffUnitOfMeasure\TestFiles\Output\RefCusTariff_BR_HSN_UOM.xml");
	}
}
