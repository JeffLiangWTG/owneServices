using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CAReferenceData.Model
{
	public class CAHarmonizedTariff
	{
		public string Tariff { get; set; }
		public string Description { get; set; }
		public DateTime EffectiveDate { get; set; }
		public string Uom { get; set; }
		public Dictionary<string, string> TariffRates { get; set; }
	}
}
