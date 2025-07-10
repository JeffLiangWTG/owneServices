namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IConditionFormulaExtractor
	{
		IFormulaExtractionResult GetFormula(string rawRateFormula, string rateCode);
	}
}
