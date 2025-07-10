using System.Collections.Generic;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffAttributesDTO
	{
		public IEnumerable<Atributo> Attributes { get; set; }

		public IEnumerable<NCM> Tariffs { get; set; }
	}
}
