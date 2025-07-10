using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRawNomenclatureDailyRecord : IRawNomenclatureRecord, IRawDeclarableCodeRecord
	{
		string Publish { get; }
		int SequenceNumber { get; }
		string FileName { get; }
	}
}
