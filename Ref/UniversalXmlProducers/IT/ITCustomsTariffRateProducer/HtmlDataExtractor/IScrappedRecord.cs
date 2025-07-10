using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor
{
	public interface IScrappedRecord
	{
		MeasureData Measure { get; }
		IEnumerable<RequirementData> Requirement { get; }
	}
}
