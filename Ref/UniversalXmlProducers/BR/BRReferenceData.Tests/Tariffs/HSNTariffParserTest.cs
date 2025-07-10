using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class HSNTariffParserTest
	{
		[Test]
		public void TestGenerateCustomsTariffRefDataRepoXml()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.RefCusTariff_BR_HSN.xml"))
			using (var inputStreamXml = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.itemNcm.xml"))
			using (var inputStreamXls = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.Table_NCM.xls"))
			{
				new BRTariffSubitemParserForTesting("BR HSN Tariff").ExportToXMLFile(inputStreamXls, inputStreamXml, TestOutputFilePath, new DateTime(2021, 04, 19, 16, 00, 00));

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

		class BRTariffSubitemParserForTesting : HSNTariffParser
		{
			public BRTariffSubitemParserForTesting(string dataSource) : base(dataSource)
			{
			}

			public override DateTime GetDefaultStartDate()
			{
				return DateTime.ParseExact($"01/01/2021T00:00:00", "dd/MM/yyyyThh:mm:ss", null);
			}
		}

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusTariff_BR_HSN.xml");
	}
}
