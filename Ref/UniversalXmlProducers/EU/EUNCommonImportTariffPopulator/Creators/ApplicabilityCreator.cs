using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.Common.SafeDataClient;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public static class ApplicabilityCreator
	{
		public static RefCusApplicability Get(measure measure)
		{
			return new RefCusApplicability
			{
				ZZT_ZZA_NKTradeGroup = measure.geographicalAreaId,
				ZZT_OrderNumber = measure.quotaOrderNumber,
				ZZT_AdditionalCode = measure.additionalCodeType + measure.additionalCodeId,
				ZZT_StartDate = measure.dateStartSpecified ? measure.dateStart : new DateTime(1900, 01, 01),
				ZZT_EndDate = measure.dateEndSpecified ? measure.dateEnd.MidnightToEndOfDay() : new DateTime(2079, 06, 06, 23, 59, 0)
			};
		}
	}
}
