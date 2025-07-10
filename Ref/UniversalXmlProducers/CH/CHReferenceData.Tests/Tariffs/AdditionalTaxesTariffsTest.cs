using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	[TestFixture]
	class AdditionalTaxesTariffsTest
	{
		[Test]
		public void TestDownloadTariffsAndConvert()
		{
			using (var expectedTestStream = classType.GetTestStream("TestFiles.Output.edecTariffMasterData_1_0_additionalTaxes.xml"))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				using var edecTariffMasterDataZip = classType.GetZippedTestStream("TestFiles.Input.edecTariffMasterData_1_0.xml");
				mockHttp.When(MasterDataDownloadUrl).WithUserAgent().Respond("application/zip", edecTariffMasterDataZip);
				mockHttp.When(KeyStructureAdditionalTaxesDownloadUrl).WithUserAgent().Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", classType.GetTestStream("TestFiles.Input.statistical_keys_import_with_Zusabg_Edec.xlsx"));

				var client = mockHttp.ToHttpClient();

				var masterDataDownload = DownloadMasterdata.DownloadAndUnzip(client);
				var keyStructureAdditionalTaxesDownload = DownloadKeyStructureAdditionalTaxes.Download(client);

				using (var outputFile = new TemporaryOutputFile(@"AdditionalTaxesTariffs\TestDownloadTariffsAndConvert.xml"))
				{
					var parser = new AdditionalTaxesTariffsParser(masterDataDownload, keyStructureAdditionalTaxesDownload, AdditionalTaxesTariffsParserTest.DefaultTestDate);
					parser.ConvertToRefXML(outputFile.FullPath);
					
					using (var converterResultStream = new FileStream(outputFile.FullPath, FileMode.Open))
					{
						var expectedXml = XDocument.Load(expectedTestStream);
						var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(converterResultStream));
						Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml), $"File content does not match");
					}
				}
			}
		}

		Type classType => GetType();

		const string MasterDataDownloadUrl = "https://edec.douane.swiss/data/edecTariffMasterData_1_0.zip";
		const string KeyStructureAdditionalTaxesDownloadUrl = "https://www.bazg.admin.ch/dam/bazg/de/dokumente/archiv/a5/tares_datenlieferungen/statistical_keys_import_with_Zusabg.xlsx.download.xlsx/statistical_keys_import_with_Zusabg.xlsx";
	}
}
