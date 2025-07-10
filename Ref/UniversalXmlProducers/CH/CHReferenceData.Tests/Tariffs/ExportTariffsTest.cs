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
	class ExportTariffsTest
	{
		[Test]
		public void TestDownloadTariffsAndConvert()
		{
			using (var expectedTestStream = classType.GetTestStream("TestFiles.Output.passarTariffMasterData_v3_export.xml"))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				using var passarTariffMasterDataZip = classType.GetZippedTestStream("TestFiles.Input.passarTariffMasterData_v3.xml");
				mockHttp.When(MasterDataDownloadUrl).WithUserAgent().Respond("application/zip", passarTariffMasterDataZip);
				mockHttp.When(TariffStructureDownloadUrl).WithUserAgent().Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", classType.GetTestStream("TestFiles.Input.Tarifstruktur_latest_format.xlsx"));
				mockHttp.When(KeyStructureDownloadUrl).WithUserAgent().Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", classType.GetTestStream("TestFiles.Input.key_structure_export_latest_format.xlsx"));
				mockHttp.When(PermitInformationDownloadUrl).WithUserAgent().Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", classType.GetTestStream("TestFiles.Input.statistical_keys_export_with_Bew_and_Tol_latest_format.xlsx"));
				mockHttp.When(NonCustomsLawInformationDownloadUrl).WithUserAgent().Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", classType.GetTestStream("TestFiles.Input.statistical_keys_export_with_nze_latest_format.xlsx"));

				var client = mockHttp.ToHttpClient();

				var masterDataDownload = DownloadPassarMasterData.DownloadAndUnzip(client);
				var tariffStructureDownload = DownloadTariffStructure.Download(client);
				var keyStructureDownload = DownloadKeyStructureExport.Download(client);
				var permitInformationDownload = DownloadPermitInformationExport.Download(client);
				var nonCustomsLawInformationDownload = DownloadNonCustomsLawInformationExport.Download(client);

				using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestDownloadTariffsAndConvert.xml"))
				{
					var parser = new ExportTariffsParser(masterDataDownload, tariffStructureDownload, keyStructureDownload, permitInformationDownload, nonCustomsLawInformationDownload, TariffsParserTest.DefaultTestDate);
					parser.ConvertToRefXML(outputFile.FullPath, ExportTariffsParserTest.TariffTestFilter);
					
					using (var converterResultStream = new FileStream(outputFile.FullPath, FileMode.Open))
					{
						var expectedXml = XDocument.Load(expectedTestStream);
						var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(converterResultStream));
						Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml), $"File content does not match");
					}
				}

				mockHttp.Dispose();
			}
		}

		Type classType => GetType();

		const string MasterDataDownloadUrl = "https://datahub.bazg.admin.ch/TariffMasterData_v3.zip";
		const string TariffStructureDownloadUrl = "https://www.ezv.admin.ch/dam/ezv/de/dokumente/archiv/a5/tares_datenlieferungen/Tarifstruktur.xlsx.download.xlsx/Tarifstruktur.xlsx";
		const string KeyStructureDownloadUrl = "https://www.ezv.admin.ch/dam/ezv/de/dokumente/archiv/a5/tares_datenlieferungen/key_structure_export.xlsx.download.xlsx/key_structure_export.xlsx";
		const string PermitInformationDownloadUrl = "https://www.bazg.admin.ch/dam/bazg/de/dokumente/archiv/a5/tares_datenlieferungen/statistical_keys_export_with_Bew_and_Tol.xlsx.download.xlsx/statistical_keys_export_with_Bew_and_Tol.xlsx ";
		const string NonCustomsLawInformationDownloadUrl = "https://www.bazg.admin.ch/dam/bazg/de/dokumente/archiv/a5/tares_datenlieferungen/statistical_keys_export_with_nze.xlsx.download.xlsx/statistical_keys_export_with_nze.xlsx";
	}
}
