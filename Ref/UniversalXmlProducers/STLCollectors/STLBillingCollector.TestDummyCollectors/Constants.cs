namespace CargoWise.RefDbRepo.STLBillingCollector.TestDummyCollectors
{
	public static class Constants
	{
		public const string StartDateTimeInclusiveParamName = "@StartDateTimeInclusive";
		public const string EndDateTimeExclusiveParamName = "@EndDateTimeExclusive";
		public const string MonthAsDateVariableName = "@MonthAsDate";

		public static string MonthAsDateVariableScript => $@"
DECLARE @MonthRangeStart DATETIME = {StartDateTimeInclusiveParamName};
DECLARE @MonthRangeEnd DATETIME = {EndDateTimeExclusiveParamName};

-- Validates range. For performance reasons it should not be greater than one month
IF (DATEDIFF(dd, @MonthRangeStart, @MonthRangeEnd) > 31)
BEGIN
	RAISERROR ('Date range cannot be greater than one month.', 16, 1);
	RETURN;
END;

DECLARE @DayBeforeEndDate DATE = DATEADD(day, -1, @MonthRangeEnd);
DECLARE {MonthAsDateVariableName} DATE = DATEFROMPARTS(YEAR(@DayBeforeEndDate), MONTH(@DayBeforeEndDate), 1);";
	}
}
