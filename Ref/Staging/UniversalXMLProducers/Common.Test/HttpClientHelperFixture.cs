using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test
{
	[TestFixture]
	class HttpClientHelperFixture
	{
		[Test]
		public void CloseConnection()
		{
			var httpClientHelper = new HttpClientHelper();
			_ = httpClientHelper.GetAsync("http://www.google.com");
			Assert.That(httpClientHelper.client.DefaultRequestHeaders.ConnectionClose, Is.Not.True);
			httpClientHelper.CloseConnection();
			Assert.That(httpClientHelper.client.DefaultRequestHeaders.ConnectionClose, Is.True);
		}
	}
}
