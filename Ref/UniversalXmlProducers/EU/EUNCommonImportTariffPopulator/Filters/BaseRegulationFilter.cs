using System;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class BaseRegulationFilter : IXmlFilter<baseRegulation>
	{
		public baseRegulation GetValidValue(baseRegulation value)
		{
			return value;
		}

		public bool IsValid(baseRegulation value, DateTime publicationDate)
		{
			return value.nationalSpecified && value.national == 0L
				&& (!value.dateEndSpecified || value.dateEnd >= publicationDate)
				&& (!value.effectiveEndDateSpecified || value.effectiveEndDate >= publicationDate);
		}
	}
}
