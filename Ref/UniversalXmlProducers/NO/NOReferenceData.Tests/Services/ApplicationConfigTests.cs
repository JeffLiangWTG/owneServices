using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.Tests
{
	sealed class ApplicationConfigTests
	{
		[Test]
		public void TestOutputDirectory()
		{
			Assert.That(ApplicationConfig.OutputDirectory, Is.EqualTo(@"..\..\UXmlFiles"));
		}

		[Test]
		public void TestResourceSearchUrl()
		{
			Assert.That(ApplicationConfig.ResourceSearchUrl, Is.EqualTo("https://data.toll.no/api/3/action/resource_search?query=name:"));
		}

		[Test]
		public void TestResourceExchangeRatesFilename()
		{
			Assert.That(ApplicationConfig.ResourceExchangeRatesFilename, Is.EqualTo("valutakurs.xml"));
		}

		[Test]
		public void TestResourceImportreferenceFilename()
		{
			Assert.That(ApplicationConfig.ResourceImportReferenceFilename, Is.EqualTo("innfoerselsreferanse.xml"));
		}

		[Test]
		public void TestResourceExportreferenceFilename()
		{
			Assert.That(ApplicationConfig.ResourceExportReferenceFilename, Is.EqualTo("utfoerselsreferanse.xml"));
		}

		[Test]
		public void TestResourceRaavaretollsatsFilename()
		{
			Assert.That(ApplicationConfig.ResourceRaavaretollsatsFilename, Is.EqualTo("raavaretollavgiftssats.xml"));
		}
	}
}
