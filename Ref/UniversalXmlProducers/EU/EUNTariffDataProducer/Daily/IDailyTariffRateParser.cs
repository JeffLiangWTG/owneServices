using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IDailyTariffRateParser
	{
		void Parse(string filePath);
		IEnumerable<IRawRateRecord> GetParsedRecords();
		bool IsImport { get; set; }
	}
}
