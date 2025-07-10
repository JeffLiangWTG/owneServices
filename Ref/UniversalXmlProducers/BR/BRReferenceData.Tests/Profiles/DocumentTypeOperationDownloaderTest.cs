using System.IO;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class DocumentTypeOperationDownloaderTest
	{
		[TestCase(true)]
		[TestCase(false)]
		public void TestDownload(bool isProd)
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var mockHttp = new MockHttpMessageHandler())
			{
				MockHttp(mockHttp, isProd, "LPCO");
				MockHttp(mockHttp, isProd, "DUIMP");
				MockHttp(mockHttp, isProd, "CATP");

				using (var expectedJsonStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.DocumentTypeOperation_result.txt"))
				using (var reader = new StreamReader(expectedJsonStream))
				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new DocumentTypeOperationDownloader();
					var list = downloader.Download(client, isProd);
					var json = JsonConvert.SerializeObject(list);
					var expectedJson = reader.ReadToEnd();
					Assert.AreEqual(expectedJson, json);
				}
			}
		}

		public static void MockHttp(MockHttpMessageHandler mock, bool isProd, string typeOperation) => mock.When(HttpMethod.Get, ConfigurationProvider.Configuration[GetParameterURL(isProd)].Replace("{tipoOperacao}", typeOperation)).Respond("Application/file", Utils.GetManifestResourceStream(GetExpectedJsonPath(typeOperation)));

		static string GetParameterURL(bool isProd) => isProd ? "URL_PORTAL_UNICO_TIPOS_DOCUMENTOS_OPERACAO" : "URL_PORTAL_UNICO_TIPOS_DOCUMENTOS_OPERACAO_TEST";

		static string GetExpectedJsonPath(string type) => ExpectedJsonPath.Replace("{replace}", type);

		static string ExpectedJsonPath = "CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.DocumentTypeOperation_{replace}.json";
	}
}
