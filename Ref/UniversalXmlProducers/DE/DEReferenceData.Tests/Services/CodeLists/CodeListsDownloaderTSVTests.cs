using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.CodeLists.Testing
{
	[TestFixture]
	class CodeListsDownloaderTSVTests
	{
		[Test]
		public void TestGetEMCSDownloadLinksWithRetry()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var url = "http://test.url/downloadlinks";
				var mockContent = "<html><head><base href=\"http://test.url/\"/></head><body><a class=\"c-link is-download-link\" href=\"/link1\">\r\n    item1<span class=\"c-link__meta\">description</span></a></body></html>";
				var stream = new MemoryStream(Encoding.UTF8.GetBytes(mockContent));
				mockHttp.Expect(url).Throw(new System.Net.Sockets.SocketException(10060));
				mockHttp.Expect(url).Respond("application/text", stream);
				var client = mockHttp.ToHttpClient();

				var actual = CodeListsDownloaderTSV.GetEMCSDownloadLinks(client, url);

				Assert.That(actual, Is.EqualTo(new Dictionary<string, string> { { "item1", "http://test.url/link1" } }));
			}
		}

		[Test]
		public void TestGetImportDownloadLinksWithRetry()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var url = "http://test.url";
				var mockContent = "#\r\nAAA.BBB";
				var stream = new MemoryStream(Encoding.UTF8.GetBytes(mockContent));
				mockHttp.Expect(url).Throw(new System.Net.Sockets.SocketException(10060));
				mockHttp.Expect(url).Respond("application/text", stream);
				var client = mockHttp.ToHttpClient();

				var actual = CodeListsDownloaderTSV.GetImportDownloadLinks(client, url);

				Assert.That(actual, Is.EqualTo(new Dictionary<string, string> { { "AAA", "AAA.BBB" } }));
			}
		}

		[Test]
		public async Task TestDownloadWithRetryAsync()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var url = "http://test.url";
				var mockContent = "ABC";
				var stream = new MemoryStream(Encoding.UTF8.GetBytes(mockContent));
				mockHttp.Expect(url).Throw(new System.Net.Sockets.SocketException(10060));
				mockHttp.Expect(url).Respond("text/xml", stream);
				var client = mockHttp.ToHttpClient();
				var targetPath = Path.GetTempFileName();
				var actual = string.Empty;

				try
				{
					await CodeListsDownloaderTSV.Download(client, url, targetPath);
					actual = File.ReadAllText(targetPath);
				}
				finally
				{
					if (File.Exists(targetPath))
					{
						File.Delete(targetPath);
					}
				}

				Assert.That(actual, Is.EqualTo(mockContent));
			}
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}

		protected Assembly assembly;
	}
}
