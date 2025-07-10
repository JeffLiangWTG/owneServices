using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	class ParsingResultTest
	{
		[TestCase("", ExpectedResult = false)]
		[TestCase("Unable to retrieve National section", ExpectedResult = false)]
		[TestCase("Critical error message", ExpectedResult = true)]
		public bool IsCriticalError(string errorMessage) => new ParsingResult(string.Empty, errorMessage).IsCriticalError;
	}
}
