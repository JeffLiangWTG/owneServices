using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface INomenclatureRecord
	{
		string TariffHeader { get; }
		DateTime DeclarableStartDate { get; }
		DateTime EndDate { get; }
		int HierarchyPosition { get; }
		int Level { get; }
		string Description { get; }
		bool IsTariff { get; }
		ICollection<(string language, string description)> Language { get; }
	}
}
