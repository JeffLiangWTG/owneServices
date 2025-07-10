using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class FileDownloaderHttpClientFactoryFixture
	{
		[Test]
		public void TestHttpClientTimeout()
		{
			using (var httpClient = FileDownloaderHttpClientFactory.CreateHttpClient())
			{
				Assert.That(httpClient.Timeout, Is.EqualTo(new TimeSpan(0, 10, 0)));
			}
		}
	}
}
