using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface IConditionValueTypeCreator
	{
		string Get(measureCondition condition);
	}
}
