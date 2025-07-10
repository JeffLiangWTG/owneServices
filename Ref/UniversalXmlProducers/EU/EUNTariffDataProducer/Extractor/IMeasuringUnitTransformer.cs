namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IMeasuringUnitTransformer
	{
		string Transform(string rawRateFormula);
		string ReplaceUom(string rawRateFormula, string replacementValue);
		string[] GetUoms(string rawRateFormula);
	}
}
