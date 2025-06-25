namespace CargoWise.Billing.Collectors.Core
{
	#region SuppressResourceStringsCheckRegion

	public class CoreRobotUserCountDaily : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "RB2";
		public override string RoleName => "Core Engine";
		public override string ModuleName => "Core Pack";
		public override string FunctionName => "Active Staff";
		public override string FeatureName => "Robot User Count Daily";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "aspd.DayAsDate";
		public override string BillingReference1 => "gs.GS_Code";
		public override string BillingReference2 => "aspd.Period";
		public override string BillingReference3 => "gs.GS_FullName";
		public override string BillingReference4 => "gs.GS_LoginName";
		public override string GuidReference => "gs.GS_PK";
		public override string CreatingUserCode => "gs.GS_Code";
		public override string PreparationScript => @"
DECLARE @UstEvent CHAR(3) = 'UST';
DECLARE @DayAsDate DATE = DATEFROMPARTS(YEAR(@EndDateTimeExclusive), MONTH(@EndDateTimeExclusive), DAY(@EndDateTimeExclusive));

DECLARE @ActiveStaffPerDay TABLE (
	DayAsDate date not null,
	Period int not null,
	StaffCode varchar(3) not null);

WITH
CurrentStaffCte AS (
	SELECT GS_PK, GS_Code, GS_IsActive, GS_IsDevice, GS_CanLogin, GS_IsRobot
	FROM dbo.GlbStaff gs
	WHERE gs.GS_IsResource = 0
	AND gs.GS_IsSystemAccount = 0
),
RankedUstLogCte AS (
    SELECT
		staff.GS_Code, 
		ustLog.SL_Reference, 
		LogSequence = ROW_NUMBER() OVER(PARTITION BY ustLog.SL_Parent ORDER BY ustLog.SL_PostedTimeUtc)
	FROM
		dbo.StmALog ustLog
		INNER JOIN CurrentStaffCte staff ON staff.GS_PK = ustLog.SL_Parent
	WHERE
		ustLog.SL_PostedTimeUtc >= @EndDateTimeExclusive
		AND ustLog.SL_PostedTimeUtc <= GETUTCDATE()
		AND ustLog.SL_SE_NKEvent = @UstEvent
)
INSERT @ActiveStaffPerDay
	-- Add staff with UserSeat events within date range where Reference like ('___>>%' or '%>>___')
	SELECT @DayAsDate, (DATEPART(year, @DayAsDate) * 100) + DATEPART(month, @DayAsDate), staff.GS_Code
		FROM dbo.StmALog ustLog
		INNER JOIN CurrentStaffCte staff ON staff.GS_PK = ustLog.SL_Parent
		WHERE ustLog.SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND ustLog.SL_PostedTimeUtc < @EndDateTimeExclusive
		AND ustLog.SL_SE_NKEvent = @UstEvent
		AND ustLog.SL_Reference like '%RBU%'
	UNION
	-- Add staff who first UST event after date range has a Reference value like '___>>%'
	SELECT @DayAsDate, (DATEPART(year, @DayAsDate) * 100) + DATEPART(month, @DayAsDate), staff.GS_Code
		FROM CurrentStaffCte staff
		INNER JOIN RankedUstLogCte rankedUst ON staff.GS_Code = rankedUst.GS_Code
		WHERE rankedUst.LogSequence = 1
		AND rankedUst.SL_Reference like 'RBU%'
	UNION
	-- Add staff with no UST events after date range and whose current flags match the staff status (IsActive=1, IsDevice=?, CanLogin=?)
	SELECT @DayAsDate, (DATEPART(year, @DayAsDate) * 100) + DATEPART(month, @DayAsDate), staff.GS_Code
		FROM CurrentStaffCte staff
		LEFT JOIN RankedUstLogCte rankedUst ON staff.GS_Code = rankedUst.GS_Code
		WHERE rankedUst.LogSequence is null
		AND staff.GS_IsActive = 1
		AND staff.GS_IsRobot = 1
		AND staff.GS_CanLogin = 1
;";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
			@ActiveStaffPerDay aspd
			INNER JOIN dbo.GlbStaff gs ON gs.GS_Code = aspd.StaffCode
			LEFT JOIN dbo.GlbBranch gb ON gb.GB_PK = isnull(gs.GS_GB_HomeBranch, gs.GS_GB_LastLogonBranch)
			LEFT JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => "";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.11.28.133";
	}

	#endregion
}
