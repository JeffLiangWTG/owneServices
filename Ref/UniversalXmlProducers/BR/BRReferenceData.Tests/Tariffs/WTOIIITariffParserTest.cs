using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class WTOIIITariffParserTest
	{
		[Test]
		public void TestSplitAndCollect()
		{

			Assert.AreEqual("DECRETO", WTOIIITariffParser.SplitAndCollect("DECRETO/EXEC 1355/1994", '/', 0));
			Assert.AreEqual("EXEC 1355", WTOIIITariffParser.SplitAndCollect("DECRETO/EXEC 1355/1994", '/', 1));
			Assert.AreEqual("1994", WTOIIITariffParser.SplitAndCollect("DECRETO/EXEC 1355/1994", '/', 2));
		}

		[Test]
		public void TestExportToXMLFile()
		{
			var assembly = Assembly.GetExecutingAssembly();

			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariff_BR_WTOL3.xml"))
			using (var inputStreamWTO = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.WTO.omc_perfuracoes_tec_com_ex.xlsx"))
			{
				new WTOIIITariffParser("BR WTOIII Tariff").ExportToXMLFile(inputStreamWTO, TestOutputFilePath, new DateTime(2022, 08, 17, 00, 00, 00));
				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
				File.Delete(TestOutputFilePath);
			}
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusTariff_BR_WTOL3.xml");
	}
}

