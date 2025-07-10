using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class WebClientWrapperTest
	{
		[Test]
		public void TestUserAgent()
		{
			var webClient = new WebClientWrapper();

			Assert.AreEqual("Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)", webClient.Headers.Get("User-Agent"));
			Assert.AreEqual(true, webClient.UseDefaultCredentials);
		}

	}
}
