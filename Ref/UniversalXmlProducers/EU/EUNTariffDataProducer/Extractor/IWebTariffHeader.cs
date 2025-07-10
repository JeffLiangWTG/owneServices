using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IWebTariffHeader
	{
		string TariffCode { get; }
		string Description { get; }
		string CompositeKey { get; set; }
		DateTime StartDate { get; set; }
		IEnumerable<RefCusTariffLanguage> TariffLanguage { get; set; }
	}
}
