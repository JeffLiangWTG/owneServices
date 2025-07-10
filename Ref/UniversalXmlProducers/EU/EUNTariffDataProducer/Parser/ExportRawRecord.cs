using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ExportRawRecord : RawRecord
	{
		public ExportRawRecord()
		{
		}

		protected override bool IsDutyFile(string fileName) => fileName.Contains("export");
		protected override IEnumerable<string> ValidMeasureTypeIdsForMeasureConditionInRate => ApplicationConfig.ValidMeasureTypeIdsForMeasureConditionInExportRate;
	}
}
