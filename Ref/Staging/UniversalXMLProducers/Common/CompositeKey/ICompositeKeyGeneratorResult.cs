using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey
{
	public interface ICompositeKeyGeneratorResult
	{
		ICollection<RefCusNomenclatureGroup> NomenclatureGroups { get; }
		ICollection<RefCusTariff> Tariffs { get; }
	}
}
