using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.DEReferenceData.Testing;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.CodeLists.Testing
{
	[TestFixture]
	class CodeListsDownloaderXMLTests
	{
		[Test]
		public void TestGetAndMergeDownloadLinks()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var streamPrimary = assembly.GetManifestResourceStream($"{TestFilesManifestBasePath}.EXPORT_CODELISTS_DOWNLOAD_LINKS_PRIMARY.uri");
				var streamSecondary = assembly.GetManifestResourceStream($"{TestFilesManifestBasePath}.EXPORT_CODELISTS_DOWNLOAD_LINKS_SECONDARY.uri");
				mockHttp.When("http://www.exportPrimary.de").Respond("application/text", streamPrimary);
				mockHttp.When("http://www.exportSecondary.de").Respond("application/text", streamSecondary);
				var client = mockHttp.ToHttpClient();

				var actual = CodeListsDownloaderXML.GetDownloadLinks(client, "http://www.exportPrimary.de", "http://www.exportSecondary.de");

				Assert.That(string.Join(System.Environment.NewLine, actual), Is.EqualTo(TestHelper.ReadManifestResourceContent($"{TestFilesManifestBasePath}.EXPORT_CODELISTS_DOWNLOAD_LINKS_UNION.uri")));
			}
		}

		[Test]
		public void TestGetAndMergeDownloadLinksWithRetry()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var url = "http://test.url";
				var mockContent = "AAA\r\nBBB";
				var stream = new MemoryStream(Encoding.UTF8.GetBytes(mockContent));
				mockHttp.Expect(url).Throw(new System.Net.Sockets.SocketException(10060));
				mockHttp.Expect(url).Respond("application/text", stream);
				var client = mockHttp.ToHttpClient();

				var actual = CodeListsDownloaderXML.GetDownloadLinks(client, url);

				Assert.That(actual, Is.EqualTo(new string[] { "AAA", "BBB" }));
			}
		}

		[Test]
		public async Task TestDownloadWithRetryAsync()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var url = "http://test.url";
				var mockContent = "<Test></Test>";
				var stream = new MemoryStream(Encoding.UTF8.GetBytes(mockContent));
				mockHttp.Expect(url).Throw(new System.Net.Sockets.SocketException(10060));
				mockHttp.Expect(url).Respond("text/xml", stream);
				var client = mockHttp.ToHttpClient();

				var actual = await CodeListsDownloaderXML.Download(client, url);

				Assert.That(actual.Content, Is.EqualTo(mockContent));
			}
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}

		protected Assembly assembly;

		const string TestFilesManifestBasePath = "CargoWise.RefDbRepo.DEReferenceData.Tests.Services.TestFiles";
	}
}
