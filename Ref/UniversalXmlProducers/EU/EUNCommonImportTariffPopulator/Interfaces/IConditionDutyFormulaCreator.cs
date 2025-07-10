using System.Collections.Generic;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface IConditionDutyFormulaCreator
	{
		string Get(string conditionTypeId, IEnumerable<measureCondition> conditions, string reduceIndicator);
	}
}
