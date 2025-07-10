using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ImportRawRecord : RawRecord
	{
		public ImportRawRecord()
		{
		}

		public ImportRawRecord(IDailyTariffRateParser dailyTariffRateParser, ISEMeasureParser seMeasureParser) : base(dailyTariffRateParser, seMeasureParser)
		{
		}

		protected override bool IsDutyFile(string fileName) => fileName.Contains("import") || fileName.Contains("export");

		protected override IEnumerable<string> ValidMeasureTypeIdsForMeasureConditionInRate => ApplicationConfig.ValidMeasureTypeIdsForMeasureConditionInImportRate.Concat(ApplicationConfig.ValidMeasureTypeIdsForMeasureConditionInExportRate);
	}
}
