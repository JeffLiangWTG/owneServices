using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.TransportCommon.Business
{
	public static class TransportDateRangeValidation
	{
		// tested by DtbConsignmentRunSheetValidationTest + DtbConsignmentRunSheetInstructionValidationTest
		public static void ErrorOnStartIfAfterEnd(ZPropertyInfo<ZDateTimeOffset> startTimeInfo, ZPropertyInfo<ZDateTimeOffset> endTimeInfo)
		{
			if (IsStartTimeLaterThanEndTime(startTimeInfo.Value, endTimeInfo.Value))
			{
				startTimeInfo.AddError(Res.GetString("c4c32b32-7c32-4bf7-a6ae-6b6b7a7bbbfa", "The {0} cannot be after the {1}.", startTimeInfo.HumanReadableName, endTimeInfo.HumanReadableName));
			}
		}

		// tested by DtbConsignmentRunSheetValidationTest + DtbConsignmentRunSheetInstructionValidationTest
		public static void ErrorOnEndIfBeforeStart(ZPropertyInfo<ZDateTimeOffset> startTimeInfo, ZPropertyInfo<ZDateTimeOffset> endTimeInfo)
		{
			if (IsStartTimeLaterThanEndTime(startTimeInfo.Value, endTimeInfo.Value))
			{
				endTimeInfo.AddError(Res.GetString("ab139235-75d3-43ac-8fbd-8c08fdd4ac96", "The {0} cannot be prior to the {1}.", endTimeInfo.HumanReadableName, startTimeInfo.HumanReadableName));
			}
		}

		static bool IsStartTimeLaterThanEndTime(ZDateTimeOffset startTime, ZDateTimeOffset endTime)
		{
			return startTime.IsValid && endTime.IsValid && startTime > endTime;
		}
	}
}
