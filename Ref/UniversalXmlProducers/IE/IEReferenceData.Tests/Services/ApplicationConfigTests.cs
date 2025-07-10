using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.Services.Tests
{
	[TestFixture]
	class ApplicationConfigTests
	{
		[Test]
		public void OutputDirectory()
		{
			Assert.That(ApplicationConfig.Instance.OutputDirectory, Is.EqualTo(@"..\..\UXmlFiles"));
		}

		[Test]
		public void ExchangeRatesStartURL()
		{
			Assert.That(ApplicationConfig.Instance.ExchangeRatesStartURL, Is.EqualTo("https://www.revenue.ie/en/customs/businesses/importing-exporting/exchange-rates/index.aspx"));
		}

		[Test]
		public void CodeListsRevenueBaseUrl()
		{
			Assert.That(ApplicationConfig.Instance.RevenueBaseUrl, Is.EqualTo("https://www.revenue.ie"));
		}

		[Test]
		public void CodeListsBasePageAES()
		{
			Assert.That(ApplicationConfig.Instance.BasePageAES, Is.EqualTo("https://www.revenue.ie/en/online-services/support/software-developers/technical-specifications-for-ecustoms/ucc-aes/trader-guide.aspx"));
		}

		[Test]
		public void CodeListsCodeListTextAES()
		{
			Assert.That(ApplicationConfig.Instance.CodeListTextAES, Is.EqualTo("AES Codelists"));
		}

		[Test]
		public void CodeListsBasePageAIS()
		{
			Assert.That(ApplicationConfig.Instance.BasePageAIS, Is.EqualTo("https://www.revenue.ie/en/online-services/support/software-developers/technical-specifications-for-ecustoms/ucc-ais/ais-cci-1-2.aspx"));
		}

		[Test]
		public void CodeListsCodeListTextAIS()
		{
			Assert.That(ApplicationConfig.Instance.CodeListTextAIS, Is.EqualTo("AIS and CCI Codelists"));
		}

		[Test]
		public void CodeListsBasePageNCTS()
		{
			Assert.That(ApplicationConfig.Instance.BasePageNCTS, Is.EqualTo("https://www.revenue.ie/en/online-services/support/software-developers/technical-specifications-for-ecustoms/ncts-p5/trader-guides.aspx"));
		}

		[Test]
		public void CodeListsCodeListTextNCTS()
		{
			Assert.That(ApplicationConfig.Instance.CodeListTextNCTS, Is.EqualTo("NCTS-P5 Codelists"));
		}

		[Test]
		public void ROSErrorsListUrl()
		{
			Assert.That(ApplicationConfig.Instance.ROSErrorsListUrl, Is.EqualTo("https://www.revenue.ie/en/customs/documents/electronic/error-spec1.txt"));
		}
	}
}
