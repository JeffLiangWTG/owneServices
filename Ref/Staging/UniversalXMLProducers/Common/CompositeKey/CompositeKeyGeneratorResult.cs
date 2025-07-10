using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey
{
	public class CompositeKeyGeneratorResult : ICompositeKeyGeneratorResult
	{
		public CompositeKeyGeneratorResult()
		{
			NomenclatureGroups = new List<RefCusNomenclatureGroup>();
			Tariffs = new List<RefCusTariff>();
		}

		public ICollection<RefCusNomenclatureGroup> NomenclatureGroups { get; }
		public ICollection<RefCusTariff> Tariffs { get; }
	}
}
