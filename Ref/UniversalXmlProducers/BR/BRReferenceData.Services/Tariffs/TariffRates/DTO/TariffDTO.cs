
using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffDTO
	{
		public string Code { get; set; }

		public IEnumerable<TariffRateDTO> TariffRates { get; set; }
	}
}
