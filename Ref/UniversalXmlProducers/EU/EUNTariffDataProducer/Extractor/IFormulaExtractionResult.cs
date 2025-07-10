namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IFormulaExtractionResult
	{
		string Formula { get; }
		string RateCode { get; }
		string RateType { get; }
	}
}
