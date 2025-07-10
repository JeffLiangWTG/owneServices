using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRawNomenclatureRecord : IExcelDataRecord
	{
		DateTime EndDate { get; }
		int HierarchyPosition { get; }
		int Level { get; }
		string Description { get; }
		string Language { get; }
	}
}
