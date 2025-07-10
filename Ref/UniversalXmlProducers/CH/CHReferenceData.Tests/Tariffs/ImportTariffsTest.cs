using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services.TradeGroups;
using CargoWise.RefDbRepo.CHReferenceData.Tests.TradeGroups;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	[TestFixture]
	class ImportTariffsTest
	{
		[Test]
		public void TestDownloadTariffsAndConvert()
		{
			using (var expectedTestStream = classType.GetTestStream("TestFiles.Output.edecTariffMasterData_1_0_download.xml"))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				using var edecTariffMasterDataZip = classType.GetZippedTestStream("TestFiles.Input.edecTariffMasterData_1_0.xml");
				using var edecCountryCodesZip = typeof(TradeGroupsTest).GetZippedTestStream("TestFiles.Input.edecCountryCodes_1_0.xml");
				mockHttp.When(MasterDataDownloadUrl).WithUserAgent().Respond("application/zip", edecTariffMasterDataZip);
				mockHttp.When(TariffStructureDownloadUrl).WithUserAgent().Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", classType.GetTestStream("TestFiles.Input.Tarifstruktur.xlsx"));
				mockHttp.When(KeyStructureDownloadUrl).WithUserAgent().Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", classType.GetTestStream("TestFiles.Input.key_structure_import_latest_format.xlsx"));
				mockHttp.When(PermitInformationDownloadUrl).WithUserAgent().Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", classType.GetTestStream("TestFiles.Input.statistical_keys_import_with_Bew_and_Tol_latest_format.xlsx"));
				mockHttp.When(NonCustomsLawInformationDownloadUrl).WithUserAgent().Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", classType.GetTestStream("TestFiles.Input.statistical_keys_import_with_nze_latest_format.xlsx"));
				mockHttp.When(CustomsFacilitiesDownloadUrl).WithUserAgent().Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", classType.GetTestStream("TestFiles.Input.customs_facilities.xlsx"));
				mockHttp.When(TradeGroupsDownloadUrl).WithUserAgent().Respond("application/zip", edecCountryCodesZip);
				var client = mockHttp.ToHttpClient();

				var masterDataDownload = DownloadMasterdata.DownloadAndUnzip(client);
				var tariffStructureDownload = DownloadTariffStructure.Download(client);
				var keyStructureDownload = DownloadKeyStructureImport.Download(client);
				var permitInformationDownload = DownloadPermitInformationImport.Download(client);
				var nonCustomsLawInformationDownload = DownloadNonCustomsLawInformationImport.Download(client);
				var customsFacilitiesDownload = DownloadCustomsFacilities.Download(client);
				var tradeGroupsDownload = DownloadTradeGroups.DownloadAndUnzip(client);

				using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestDownloadTariffsAndConvert.xml"))
				{
					var parser = new ImportTariffsParser(masterDataDownload, tariffStructureDownload, keyStructureDownload, permitInformationDownload, nonCustomsLawInformationDownload, customsFacilitiesDownload, tradeGroupsDownload, TariffsParserTest.DefaultTestDate);

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
		const string TariffStructureDownloadUrl = "https://www.ezv.admin.ch/dam/ezv/de/dokumente/archiv/a5/tares_datenlieferungen/Tarifstruktur.xlsx.download.xlsx/Tarifstruktur.xlsx";
		const string KeyStructureDownloadUrl = "https://www.ezv.admin.ch/dam/ezv/de/dokumente/archiv/a5/tares_datenlieferungen/key_structure_import.xlsx.download.xlsx/key_structure_import.xlsx";
		const string PermitInformationDownloadUrl = "https://www.bazg.admin.ch/dam/bazg/de/dokumente/archiv/a5/tares_datenlieferungen/statistical_keys_import_with_Bew_and_Tol.xlsx.download.xlsx/statistical_keys_import_with_Bew_and_Tol.xlsx";
		const string NonCustomsLawInformationDownloadUrl = "https://www.bazg.admin.ch/dam/bazg/de/dokumente/archiv/a5/tares_datenlieferungen/statistical_keys_import_with_nze.xlsx.download.xlsx/statistical_keys_import_with_nze.xlsx";
		const string CustomsFacilitiesDownloadUrl = "https://www.bazg.admin.ch/dam/bazg/de/dokumente/archiv/a5/tares_datenlieferungen/customs_facilities.xlsx.download.xlsx/customs_facilities.xlsx";
		const string TradeGroupsDownloadUrl = "https://edec.douane.swiss/data/edecCountryCodes_1_0.zip";
	}
}
