using System.Collections.Generic;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface IDutyFormulaCreator
	{
		string Get(IEnumerable<measureConditionComponent> components, string reduceIndicator, bool applyDifference, bool useCIF);
		string Get(IEnumerable<measureComponent> components, string reduceIndicator);
	}
}
