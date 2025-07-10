using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Mapping
{
	public class MappedCountry
	{
		public MappedCountry(string name, List<MappedSubdivisions> mappedSubdivisions)
		{
			Name = name;
			MappedSubdivisions = mappedSubdivisions;
		}

		public string Name { get; set; }
		public List<MappedSubdivisions> MappedSubdivisions { get; set; }
	}
}
