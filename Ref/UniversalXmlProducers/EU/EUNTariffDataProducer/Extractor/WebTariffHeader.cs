using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class WebTariffHeader : IWebTariffHeader
	{
		public string TariffCode { get; }
		public string Description { get; }
		public DateTime StartDate { get; set; }
		public string CompositeKey { get; set; }
		public IEnumerable<RefCusTariffLanguage> TariffLanguage { get; set; }

		public WebTariffHeader(string tariffCode, string description)
		{
			Argument.NotNullOrEmpty(tariffCode, nameof(tariffCode));

			TariffCode = tariffCode;
			Description = description;
		}
	}
}
