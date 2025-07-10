using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class IPIExTariffParserTest
	{
		[Test]
		public void TestExportToXml()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariff_BR_ExTariffIPI.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.ExTariff_IPI.TIPI 2022 - Atualizada ADE 005-2022.xlsx"))
			{
				var parser = new IPIExTariffParser($"BR IPI Ex Tariff");

				parser.ExportToXMLFile(inputStream, TestOutputFilePath, new DateTime(2022, 10, 04, 00, 00, 00));
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

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusTariff_BR_ExTariffIPI.xml");
	}
}
