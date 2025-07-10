using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test
{
	class HttpClientFactoryTest
	{
		[Test]
		public void CreateHttpClient()
		{
			// Arrange
			var factory = new HttpClientFactory();

			// Act
			using var client = factory.CreateClient();

			// Assert
			Assert.That(client, Is.Not.Null);
			Assert.That(client.Timeout.Seconds, Is.EqualTo(10));
			Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Is.EqualTo("RefDbRepo.CarrierMessagingBuss"));
		}
	}
}
