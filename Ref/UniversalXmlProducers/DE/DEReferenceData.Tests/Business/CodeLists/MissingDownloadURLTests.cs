using System.Net.Http;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Emcs;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing
{
	[TestFixture]
	public class MissingDownloadURLTests
	{
		[Test]
		public void MissingTSVDownloadURLTest()
		{
			using (var httpClient = new HttpClient())
			{
				var downloadLinks = CodeListsTestHelper.EMCSDownloadCodeList($"{CodeListsTestHelper.TestFilesManifestBasePath}.Emcs.TestFiles.EMCS_CODELISTS_DOWNLOAD_PAGE_FAULTY.html");
				var errors = new EmcsCnCodesEMCCNTransformer(downloadLinks).DownloadAndConvertToRefCusCodeListXML(httpClient, string.Empty, null);
				Assert.That(errors, Does.Contain("No download link found for CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Emcs.EmcsCnCodesEMCCNTransformer code list: CL0037"));
			}
		}

		[Test]
		public void MissingXMLDownloadURLTest()
		{
			using (var httpClient = new HttpClient())
			{
				var errors = new ExportCountryCodesCommunityEX15Transformer(new string[] { "https://www.zoll.de/FAULTY.xml" }).DownloadAndConvertToRefCusCodeListXML(httpClient, string.Empty).Result;
				Assert.That(errors, Does.Contain("No download link found for CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.ExportCountryCodesCommunityEX15Transformer code list: C0010"));
			}
		}
	}
}
