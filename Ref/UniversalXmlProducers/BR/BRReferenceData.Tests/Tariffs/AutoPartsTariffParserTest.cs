using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	class AutoPartsTariffParserTest
	{
		[Test]
		public void TestXmlExport()
		{
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariff_BR_AUTOR.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.AutoPartsList.ListaGeraldeAutopeasNoProduzidasv24032022_simplified.xlsx"))
			{
				var publicationDate = new DateTime(2021, 11, 26, 00, 00, 00);
				var parser = new AutoPartsTariffParser("BR AUTOR Tariff");

				parser.ExportToXMLFile(inputStream, TestOutputFilePath, new DateTime(2021, 11, 26, 00, 00, 00));
				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
				File.Delete(TestOutputFilePath);
			}
		}
		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.BRRefCusTariffAUTOR.xml");
	}
}
