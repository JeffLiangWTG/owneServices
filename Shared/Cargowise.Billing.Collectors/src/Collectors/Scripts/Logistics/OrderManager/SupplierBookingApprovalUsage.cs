namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class SupplierBookingApprovalUsage : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "SBA";
		public override string ModuleName => "Order Manager";
		public override string RoleName => "Order Manager Usage";
		public override string FunctionName => "Supplier Booking Approval Usage Collectors";
		public override string FeatureName => "Order Manager Supplier Booking Approval Usage Collector";
		public override string TransactionDateUtc => "SL_PostedTimeUtc";
		public override string BillingReference1 => "JSB_BookingId";
		public override string GuidReference => "JSB_PK";
		public override string ActiveOn => RefActiveOn.All;
		public override string FromClause => @"
dbo.JobSupplierBooking
INNER JOIN (
	SELECT
		SL_Parent,
		SL_PostedTimeUtc,
		SL_GB_NKBranch,
		SL_GS_NKUser,
		ROW_NUMBER() OVER (PARTITION BY SL_Parent ORDER BY SL_PK) AS rn
	FROM
		dbo.StmALog CurrentLog
	WHERE
		SL_Table = 'JobSupplierBooking'
		AND SL_SE_NKEvent = 'BKC'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND NOT EXISTS (
			SELECT 1
			FROM dbo.StmALog PastLog
			WHERE
				PastLog.SL_Parent = CurrentLog.SL_Parent
				AND SL_SE_NKEvent = 'BKC'
				AND	PastLog.SL_PostedTimeUtc < CurrentLog.SL_PostedTimeUtc)
) AS FirstApproval ON JSB_PK = SL_Parent AND rn = 1
LEFT JOIN dbo.GlbBranch ON GB_Code = SL_GB_NKBranch
LEFT JOIN dbo.GlbCompany ON GC_PK = GB_GC
";
		public override string WhereClause => "";
		public override bool WithOptionRecompile => false;
		public override string DateType => RefStlDateType.DateTime;
		public override string CreatingUserCode => "SL_GS_NKUser";
		public override string BranchCode => "GB_Code";
		public override string CompanyCode => "GC_Code";
	}

	#endregion
}
