using System;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class MeasureConditionCodeFilter : IXmlFilter<measureConditionCode>
	{
		public measureConditionCode GetValidValue(measureConditionCode value)
		{
			return value;
		}

		public bool IsValid(measureConditionCode value, DateTime publicationDate)
		{
			return value.nationalSpecified && value.national == 0L && (!value.dateEndSpecified || value.dateEnd >= publicationDate);
		}
	}
}
