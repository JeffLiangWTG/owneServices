using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class AdditionalCodesData
	{
		public string Code { get; set; }
		public Dictionary<string, string> MultilingualDescriptions { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}
}
