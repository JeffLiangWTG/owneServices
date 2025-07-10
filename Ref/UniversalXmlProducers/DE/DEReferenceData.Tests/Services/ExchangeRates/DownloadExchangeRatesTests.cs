using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.ExchangeRates.Testing
{
	[TestFixture]
	class DownloadExchangeRatesTests
	{
		[Test]
		public async Task TestDownloadWithRetryAsync()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var mockContent = "<Test></Test>";
				var stream = new MemoryStream(Encoding.UTF8.GetBytes(mockContent));
				mockHttp.Expect("*").Throw(new System.Net.Sockets.SocketException(10060));
				mockHttp.Expect("*").Respond("text/xml", stream);
				var client = mockHttp.ToHttpClient();
				var dateTimeProvider = new Mock<IDateTimeProvider>().Object;

				var actual = await DownloadExchangeRates.Download(DownloadExchangeRates.UnListedKursartValue, new DateTime(2021, 11, 01), new DateTime(2021, 12, 01), client, dateTimeProvider);

				Assert.That(actual.Content, Is.EqualTo(mockContent));
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
