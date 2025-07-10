using System;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Validation
{
	public class NoEffectiveDateRangeChecker
	{
		public bool IsValid(object safeObj)
		{
			var entityType = safeObj.GetEntityType();
			var tablePrefix = entityType.GetTablePrefix();
			if (tablePrefix == "S01")
			{
				var rateApp = (Safe.RefCusRateApplicability)safeObj;
				if (rateApp.RefCusApplicability != null && rateApp.S01_StartDate > rateApp.S01_EndDate)
				{
					throw CreateNoEffectiveDateRangeRefDataProcessingException(nameof(Safe.RefCusApplicability), rateApp.RefCusApplicability.ZZT_PK, new DateTimeRange(rateApp.RefCusApplicability.ZZT_StartDate, rateApp.RefCusApplicability.ZZT_EndDate), new DateTimeRange(rateApp.RefCusRate.ZZ2_StartDate, rateApp.RefCusRate.ZZ2_EndDate));
				}
			}
			else if (tablePrefix == "S07")
			{
				var conditionApp = (Safe.RefCusConditionApplicability)safeObj;
				if (conditionApp.RefCusApplicability != null && conditionApp.S07_StartDate > conditionApp.S07_EndDate)
				{
					throw CreateNoEffectiveDateRangeRefDataProcessingException(nameof(Safe.RefCusApplicability), conditionApp.RefCusApplicability.ZZT_PK, new DateTimeRange(conditionApp.RefCusApplicability.ZZT_StartDate, conditionApp.RefCusApplicability.ZZT_EndDate), new DateTimeRange(conditionApp.RefCusCondition.ZX1_StartDate, conditionApp.RefCusCondition.ZX1_EndDate));
				}
			}
			return true;
		}

		static RefDataProcessingException CreateNoEffectiveDateRangeRefDataProcessingException(string typeName, Guid safeObjectPK, DateTimeRange currentDateTimeRange, DateTimeRange parentDateTimeRange)
		{
			return new RefDataProcessingException($@"Incorrect Safe Date Range: no effective date range for the following safeObject.
SafeObject type: {typeName}, SafeObjectPK: {safeObjectPK};
SafeObject date range: {currentDateTimeRange.StartDate.GetFormatString()}~{currentDateTimeRange.EndDate.GetFormatString()}.
Parent date range: {parentDateTimeRange.StartDate.GetFormatString()}~{parentDateTimeRange.EndDate.GetFormatString()}."
, ErrorCodes.IncorrectSafeDateRange);
		}
	}
}
