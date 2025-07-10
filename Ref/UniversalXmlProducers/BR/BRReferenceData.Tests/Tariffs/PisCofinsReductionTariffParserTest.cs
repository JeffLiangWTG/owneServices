using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class PisCofinsReductionTariffParserTest
	{
		[Test]
		public void TestXmlParser()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariff_BR_PisCofinsReduction.xml"))
			using (var ncmInputStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.itemNcm.xml"))
			using (var pisCofinsInputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.PIS_COFINS.FundamentoLegalReducaoPisCofins.xml"))
			{
				var publicationDate = new DateTime(2021, 11, 26, 00, 00, 00);
				var parser = new PisCofinsReductionTariffParser("BR Pis Cofins Reduction Base");

				parser.ExportToXMLFile(pisCofinsInputStream, ncmInputStream, TestOutputFilePath, publicationDate);
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

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusTariff_BR_PisCofinsReduction.xml");
	}
}
