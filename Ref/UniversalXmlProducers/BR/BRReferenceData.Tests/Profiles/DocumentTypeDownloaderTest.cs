using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class DocumentTypeDownloaderTest
	{
		[TestCase(true)]
		[TestCase(false)]
		public void TestDownload(bool isProd)
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				DocumentTypeOperationDownloaderTest.MockHttp(mockHttp, isProd, Constants.TariffAttributes.OperationTypes.LPCO);
				DocumentTypeOperationDownloaderTest.MockHttp(mockHttp, isProd, Constants.TariffAttributes.OperationTypes.DUIMP);
				DocumentTypeOperationDownloaderTest.MockHttp(mockHttp, isProd, Constants.TariffAttributes.OperationTypes.CATP);

				MockHttp(mockHttp, isProd, "103");
				MockHttp(mockHttp, isProd, "3");
				MockHttp(mockHttp, isProd, "2");

				using (var expectedJsonStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.DocumentType_result.txt"))
				using (var reader = new StreamReader(expectedJsonStream))
				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new DocumentTypeDownloader();
					var list = downloader.Download(client, isProd);
					var json = JsonConvert.SerializeObject(list);
					var expectedJson = reader.ReadToEnd();
					Assert.AreEqual(expectedJson, json);
				}
			}
		}

		public static string GetParameterURL(bool isProd) => isProd ? "URL_PORTAL_UNICO_TIPOS_DOCUMENTOS" : "URL_PORTAL_UNICO_TIPOS_DOCUMENTOS_TEST";

		public static void MockHttp(MockHttpMessageHandler mock, bool isProd, string idDocument) => mock.When(HttpMethod.Get, ConfigurationProvider.Configuration[GetParameterURL(isProd)].Replace("{idTipoDocumento}", idDocument)).Respond("Application/file", Utils.GetManifestResourceStream(ExpectedJsonPath));

		static string ExpectedJsonPath = "CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.DocumentKeywords.json";
	}
}
