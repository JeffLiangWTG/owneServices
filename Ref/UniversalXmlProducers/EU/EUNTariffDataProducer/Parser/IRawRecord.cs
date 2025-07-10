using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRawRecord
	{
		IEnumerable<IRawRateRecord> RateRecords { get; }
		IEnumerable<IRawMeasureExclusionRecord> MeasureExclusionRecords { get; }
		IEnumerable<IRawMeasureConditionRecord> MeasureConditionRecords { get; }
		IEnumerable<IRawRateRecord> UomRecords { get; }
		List<string> GetDailyTariffHeadersToProcess();

		void Parse(IEnumerable<IWebFileInfo> webFileInfos);

		void ParseSEFileAndAttachTaricDailyRecords();
	}
}
