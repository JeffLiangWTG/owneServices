using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class MeasureTypeFilter : IXmlFilter<measureType1>
	{
		public measureType1 GetValidValue(measureType1 value)
		{
			return value;
		}

		public bool IsValid(measureType1 value, DateTime publicationDate)
		{
			return value.nationalSpecified && value.national == 0L && (!value.dateEndSpecified || value.dateEnd >= publicationDate);
		}
	}
}
