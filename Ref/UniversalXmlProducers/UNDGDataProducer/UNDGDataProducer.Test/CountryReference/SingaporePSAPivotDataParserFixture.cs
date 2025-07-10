using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using NUnit.Framework;
namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	class SingaporePSAPivotDataParserFixture
	{
		[Test]
		public void HeaderMap()
		{
			var headerMap = new SingaporePSAPivotParser(new FileDownloaderWrapper(), string.Empty).HeaderMap;

			Assert.That(headerMap[nameof(SingaporePSACountryReferencePivotRecord.UNNO)], Is.EqualTo(0));
			Assert.That(headerMap[nameof(SingaporePSACountryReferencePivotRecord.Variant)], Is.EqualTo(1));
			Assert.That(headerMap[nameof(SingaporePSACountryReferencePivotRecord.Standard)], Is.EqualTo(2));
			Assert.That(headerMap[nameof(SingaporePSACountryReferencePivotRecord.Type)], Is.EqualTo(3));
			Assert.That(headerMap[nameof(SingaporePSACountryReferencePivotRecord.Country)], Is.EqualTo(4));
			Assert.That(headerMap[nameof(SingaporePSACountryReferencePivotRecord.Code)], Is.EqualTo(5));
			Assert.That(headerMap[nameof(SingaporePSACountryReferencePivotRecord.HasFlashPointLower)], Is.EqualTo(6));
			Assert.That(headerMap[nameof(SingaporePSACountryReferencePivotRecord.FlashPointLowerCentigrade)], Is.EqualTo(7));
		}
	}
}
