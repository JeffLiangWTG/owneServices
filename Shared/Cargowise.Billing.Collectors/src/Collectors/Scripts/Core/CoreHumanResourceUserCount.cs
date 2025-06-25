namespace CargoWise.Billing.Collectors.Core
{
	#region SuppressResourceStringsCheckRegion

	public class CoreHumanResourceUserCount : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "HRU";
		public override string RoleName => "Core Engine";
		public override string ModuleName => "Core Pack";
		public override string FunctionName => "Active Staff";
		public override string FeatureName => "HR User Count";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "aspm.MonthAsDate";
		public override string BillingReference1 => "gs.GS_Code";
		public override string BillingReference2 => "gs.GS_LoginName";
		public override string BillingReference3 => "gs.GS_FullName";
		public override string GuidReference => "gs.GS_PK";
		public override string CreatingUserCode => "gs.GS_Code";
		public override string PreparationScript => @"
DECLARE @UstEvent CHAR(3) = 'UST';
DECLARE @MonthRangeStart DATETIME = @StartDateTimeInclusive;
DECLARE @MonthRangeEnd DATETIME = @EndDateTimeExclusive;

-- Validates range. For performance reasons it should not be greater than one month
IF (DATEDIFF(dd, @MonthRangeStart, @MonthRangeEnd) > 31)
BEGIN
	RAISERROR ('Date range cannot be greater than one month.', 16, 1);
	RETURN;
END;

DECLARE @DayBeforeEndDate DATE = DATEADD(day, -1, @MonthRangeEnd);
DECLARE @MonthAsDate DATE = DATEFROMPARTS(YEAR(@DayBeforeEndDate), MONTH(@DayBeforeEndDate), 1);
DECLARE @ActiveStaffPerMonth TABLE (   MonthAsDate date not null,   StaffCode varchar(3) not null);

WITH  CurrentStaffCte AS (
	SELECT GS_PK, GS_Code, GS_IsActive, GS_IsDevice, GS_CanLogin, GS_IsRobot
	FROM GlbStaff gs
	WHERE gs.GS_IsResource = 0 AND gs.GS_IsSystemAccount = 0
),
RankedUstLogCte AS (
	SELECT staff.GS_Code, ustLog.SL_Reference, LogSequence = ROW_NUMBER() OVER(PARTITION BY ustLog.SL_Parent ORDER BY ustLog.SL_PostedTimeUtc)
	FROM
	StmALog ustLog
	INNER JOIN CurrentStaffCte staff ON staff.GS_PK = ustLog.SL_Parent
	WHERE ustLog.SL_PostedTimeUtc >= @MonthRangeEnd AND ustLog.SL_PostedTimeUtc <= GETUTCDATE() AND ustLog.SL_SE_NKEvent = @UstEvent
)

INSERT @ActiveStaffPerMonth -- Add staff with UserSeat events within date range where Reference like ('___>>%' or '%>>___')
SELECT @MonthAsDate, staff.GS_Code
FROM StmALog ustLog
INNER JOIN CurrentStaffCte staff ON staff.GS_PK = ustLog.SL_Parent
WHERE ustLog.SL_PostedTimeUtc >= @MonthRangeStart AND ustLog.SL_PostedTimeUtc < @MonthRangeEnd AND ustLog.SL_SE_NKEvent = @UstEvent AND ustLog.SL_Reference like '%HRU%'
UNION

-- Add staff who first UST event after date range has a Reference value like '___>>%'
SELECT @MonthAsDate, staff.GS_Code
FROM CurrentStaffCte staff
INNER JOIN RankedUstLogCte rankedUst ON staff.GS_Code = rankedUst.GS_Code
WHERE rankedUst.LogSequence = 1 AND rankedUst.SL_Reference like 'HRU%'
UNION

-- Add staff with no UST events after date range and whose current flags match the staff status (IsActive=1, IsDevice=?, CanLogin=?)
SELECT @MonthAsDate, staff.GS_Code
FROM CurrentStaffCte staff
LEFT JOIN RankedUstLogCte rankedUst ON staff.GS_Code = rankedUst.GS_Code
WHERE rankedUst.LogSequence is null AND staff.GS_IsActive = 1 AND staff.GS_CanLogin = 0;";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
@ActiveStaffPerMonth aspm
INNER JOIN GlbStaff gs ON gs.GS_Code = aspm.StaffCode
LEFT JOIN GlbBranch gb ON gb.GB_PK = isnull(gs.GS_GB_HomeBranch, gs.GS_GB_LastLogonBranch)
LEFT JOIN GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => "";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.11.28.133";
	}

	#endregion
}
