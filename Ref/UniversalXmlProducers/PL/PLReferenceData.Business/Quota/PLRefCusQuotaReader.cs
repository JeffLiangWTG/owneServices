using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Quota;

public static class PLRefCusQuotaReader
{
	public static RefCusQuota ToRefCusQuota(this quotaDefinition quota)
	{
		var result = new RefCusQuota()
		{
			ZXQ_OrderNumber = quota.quotaOrderNumber?.quotaOrderNumberId,
			ZXQ_InitialAmount = quota.initialVolume,
			ZXQ_Balance = quota.quotaBalanceEvent?.OrderBy(x => x.occurrenceTimestamp).LastOrDefault()?.newBalance ?? 0.0m,
			ZXQ_StartDate = quota.validityStartDate,
			ZXQ_EndDate = quota.validityEndDateSpecified ? quota.validityEndDate : Constants.ConstantEndDate,
		};

		if (quota.measurementUnit is measurementUnit unit
			&& !string.IsNullOrEmpty(unit.measurementUnitCode))
		{
			result.ZXQ_UnitOfMeasure = unit.measurementUnitCode + GetMeasurementUnitQualifierCode(quota);
		}

		return result;

	}

	static string GetMeasurementUnitQualifierCode(quotaDefinition quota)
		=> quota.measurementUnitQualifier?.measurementUnitQualifierCode ?? string.Empty;

}
