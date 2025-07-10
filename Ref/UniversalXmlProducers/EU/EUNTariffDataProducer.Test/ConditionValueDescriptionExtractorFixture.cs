using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class ConditionValueDescriptionExtractorFixture
	{
		[Test]
		public void GetDescription()
		{
			var conditionValueDescriptionExtractor = new ConditionValueDescriptionExtractor();
			var measureConditionCode = "B";

			var result = conditionValueDescriptionExtractor.GetComment(measureConditionCode);
			Assert.AreEqual("Condition B:Presentation of a certificate/licence/document", result);
		}
	}
}
