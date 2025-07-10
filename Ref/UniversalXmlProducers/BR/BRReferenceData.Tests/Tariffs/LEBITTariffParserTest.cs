using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	class LEBITTariffParserTest
	{
		[Test]
		public void TestXmlExport()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariff_BR_LEBIT.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.LEBIT.anexo_vi_lebit_bk_simplified.xlsx"))
			{
				var publicationDate = new DateTime(2021, 11, 26, 00, 00, 00);
				var parser = new LEBITTariffParser("LEBIT BR RefCusTariff");

				parser.ExportToXMLFile(inputStream, TestOutputFilePath, new DateTime(2022, 07, 06, 00, 00, 00));
				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
				File.Delete(TestOutputFilePath);
			}
		}
		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.BRRefCusTariffLEBIT.xml");
	}
}
