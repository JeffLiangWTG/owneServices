using NUnit.Framework;

namespace CargoWise.RefDbRepo.SEReferenceData.Services.Tests
{
	[TestFixture]
	class ApplicationConfigTest
	{
		[Test]
		public void OutputDirectory()
		{
			Assert.That(ApplicationConfig.OutputDirectory, Is.EqualTo(@"..\..\UXmlFiles"));
		}

		[Test]
		public void CompleteMonthlyRepositoryUrl()
		{
			Assert.That(ApplicationConfig.CompleteMonthlyRepositoryUrl, Is.EqualTo("https://distr.tullverket.se/tulltaxan/xml/tot/"));
		}

		[Test]
		public void IncrementalDailyRepositoryUrl()
		{
			Assert.That(ApplicationConfig.IncrementalDailyRepositoryUrl, Is.EqualTo("https://distr.tullverket.se/tulltaxan/xml/dif/"));
		}

		[Test]
		public void FilePrefix_MeasureType()
		{
			Assert.That(ApplicationConfig.FilePrefix_MeasureType, Is.EqualTo("MeasureType"));
		}

		[Test]
		public void FilePrefix_GeographicalArea()
		{
			Assert.That(ApplicationConfig.FilePrefix_GeographicalArea, Is.EqualTo("GeographicalArea"));
		}
	}
}
