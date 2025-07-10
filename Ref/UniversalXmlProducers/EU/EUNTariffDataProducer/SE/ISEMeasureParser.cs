using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE
{
	public interface ISEMeasureParser
	{
		bool CanDownload();
		void Parse(List<IRawMeasureConditionRecord> measureConditionRecords, List<IRawMeasureExclusionRecord> exclusionConditionRecords);
	}
}
