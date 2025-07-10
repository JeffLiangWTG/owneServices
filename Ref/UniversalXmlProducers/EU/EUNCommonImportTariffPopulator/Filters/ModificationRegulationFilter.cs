using System;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class ModificationRegulationFilter : IXmlFilter<modificationRegulation>
	{
		public modificationRegulation GetValidValue(modificationRegulation value)
		{
			return value;
		}

		public bool IsValid(modificationRegulation value, DateTime publicationDate)
		{
			return value.nationalSpecified && value.national == 0L
				&& (!value.dateEndSpecified || value.dateEnd >= publicationDate)
				&& (!value.effectiveEndDateSpecified || value.effectiveEndDate >= publicationDate);
		}
	}
}
