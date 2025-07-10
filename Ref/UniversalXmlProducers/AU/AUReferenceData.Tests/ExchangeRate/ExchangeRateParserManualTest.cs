using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate
{
	[Explicit]
	public class ExchangeRateParserManualTest
	{
		[Test]
		public void TestGetLastModifiedDate()
		{
			var lastModifiedDate = DateTime.MinValue;
			var parser = new MockXCHAGRATEParser();

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate.TestFiles._reference_production_main_.html"))
			using (var streamTxt = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate.TestFiles.XCHGRATE-P1-EDMAIN-2002220144.txt"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			using (var readerTxt = new StreamReader(streamTxt, Encoding.UTF8))
			{
				parser.MockHttpClientHelper.Setup(x => x.GetWebPageAsync("https://www.ccf.customs.gov.au/reference/production/main/")).Returns(Task.FromResult(reader.ReadToEnd()));

				var remoteFileUrl = parser.GetSourceLocation(parser.WebPageUrlForExplictTest);
				Assert.IsNotEmpty(remoteFileUrl);
	
				var stringContent = new StringContent(readerTxt.ReadToEnd());
				stringContent.Headers.Add("last-modified", DateTime.Today.ToUniversalTime().ToString("R"));
				using (var reponse = new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = stringContent
				})
				{
					_ = parser.MockHttpMessageHandler.Protected()
						.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri == new Uri(remoteFileUrl)), ItExpr.IsAny<CancellationToken>())
						.ReturnsAsync(reponse);

					Assert.DoesNotThrow(() => lastModifiedDate = parser.GetLastModifiedDate(remoteFileUrl));
					Assert.IsTrue(lastModifiedDate != DateTime.MinValue);
				}
			}
		}
	}
}
