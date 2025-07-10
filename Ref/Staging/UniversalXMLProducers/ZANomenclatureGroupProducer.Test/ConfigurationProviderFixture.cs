using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer.Test
{
	[TestFixture]
	public class ConfigurationProviderFixture
	{
		[Test]
		public void OutputFolder()
		{
			Assert.That(ConfigurationProvider.OutputFilePath, Is.EqualTo(@"..\..\UxmlFiles"));
		}

		[Test]
		public void PDFDownloadUrl()
		{
			Assert.That(ConfigurationProvider.PDFDownloadUrl, Is.EqualTo(@"https://www.sars.gov.za/legal-lprim-ce-sch1p1chpt1-to-99-schedule-no-1-part-1-chapters-1-to-99"));
		}
	}
}
