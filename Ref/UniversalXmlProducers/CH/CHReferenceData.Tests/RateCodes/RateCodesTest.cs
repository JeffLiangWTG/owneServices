using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.RateCodes;
using CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.RateCodes
{
	internal class RateCodesTest
	{
		[Test]
		public void TestDownloadTariffsAndConvert()
		{
			using (var expectedFeeOutputStream = classType.GetTestStream("TestFiles.Output.edecTariffMasterData_1_0_feerates.xml"))
			using (var expectedAdtOutputStream = classType.GetTestStream("TestFiles.Output.edecTariffMasterData_1_0_adtrates.xml"))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				using var edecTariffMastzerDataZip = typeof(TariffsParserTest).GetZippedTestStream("TestFiles.Input.edecTariffMasterData_1_0.xml");
				mockHttp.When(MasterDataDownloadUrl).WithUserAgent().Respond("application/zip", edecTariffMastzerDataZip);
				var client = mockHttp.ToHttpClient();

				var masterDataDownload = DownloadMasterdata.DownloadAndUnzip(client);

				string feeOutputFileName = @"RateCodes\TestDownloadTariffsAndConvert-FEE.xml";
				string adtOutputFileName = @"RateCodes\TestDownloadTariffsAndConvert-ADT.xml";

				using (var feeOutputFile = new TemporaryOutputFile(feeOutputFileName))
				using (var adtOutputFile = new TemporaryOutputFile(adtOutputFileName))
				{
					new FeeRateCodesParser(masterDataDownload).ConvertToRefXML(feeOutputFile.FullPath);
					new AdditionalTaxRateCodesParser(masterDataDownload).ConvertToRefXML(adtOutputFile.FullPath);

					using (var converterResultStream = new FileStream(feeOutputFile.FullPath, FileMode.Open))
					{
						FileStreamAreEqual(feeOutputFileName, expectedFeeOutputStream, converterResultStream);
					}

					using (var converterResultStream = new FileStream(adtOutputFile.FullPath, FileMode.Open))
					{
						FileStreamAreEqual(adtOutputFileName, expectedAdtOutputStream, converterResultStream);
					}
				}
			}
		}

		void FileStreamAreEqual(string fileName, Stream expectedStream, Stream actualStream)
		{
			var expectedXml = XDocument.Load(expectedStream);
			var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));
			Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml), $"File content does not match: {fileName}");
		}


		[SetUp]
		public void SetUp()
		{
			TestHelper.DeleteLogFiles();
		}

		[TearDown]
		public void TearDown()
		{
			TestHelper.DeleteLogFiles();
		}

		Type classType => GetType();

		const string MasterDataDownloadUrl = "https://edec.douane.swiss/data/edecTariffMasterData_1_0.zip";
	}
}
