using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class SteelNomenclatureTest
	{
		[TestCase(25, 02, 2019)]
		public void OGA_OneDataFile(int day, int month, int year)
		{
			var publicationDate = new DateTime(year, month, day);

			var inputDataPath = @"Res\OGA\SteelNomenclature\SteelNomenclature_2019DataFile.xlsx";

			var inputConfigPathExport = @"Res\OGA\SteelNomenclature\SteelNomenclature_2019Configuration.xml";
			var outputFileExport = Path.Combine(TestHelper.BaseTestFilePath, @"OGA\Output\SteelNomenclature\SteelNomenclature_2019.xml");
			var expectedXMLExport = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.OGA.Output.SteelNomenclature.SteelNomenclature_2019.xml");
			new SteelNomenclaturesParser(inputConfigPathExport, inputDataPath).ConvertToXMLFile(outputFileExport, publicationDate);
			Assert.That(File.ReadAllText(outputFileExport), Is.EqualTo(expectedXMLExport));
		}
	}
}
