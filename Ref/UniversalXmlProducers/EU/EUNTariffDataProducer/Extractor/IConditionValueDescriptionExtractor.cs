namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IConditionValueDescriptionExtractor
	{
		string GetComment(string measureConditionCode);
	}
}
