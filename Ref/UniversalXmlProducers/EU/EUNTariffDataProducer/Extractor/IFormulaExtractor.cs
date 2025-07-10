using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IFormulaExtractor
	{
		IEnumerable<IFormulaExtractionResult> GetFormula(string rawRateFormula, string rateCode, string reductionIndicator);
	}
}
