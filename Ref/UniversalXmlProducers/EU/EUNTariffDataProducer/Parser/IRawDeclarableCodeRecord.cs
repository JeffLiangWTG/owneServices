using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRawDeclarableCodeRecord : IExcelDataRecord
	{
		DateTime DeclarableStartDate { get; }
		bool IsLeaf { get; }
		DateTime EndDate { get; }
	}
}
