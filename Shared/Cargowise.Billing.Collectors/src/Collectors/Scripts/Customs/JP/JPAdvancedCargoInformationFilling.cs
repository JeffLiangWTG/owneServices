namespace CargoWise.Billing.Collectors.Customs
{
	public class JPAdvancedCargoInformationFilling : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override string FeatureCode => "AFR";
		public override string FeatureName => "Advanced Cargo Information Filing - JP";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Pre-Departure, Advanced Filing and Embargo";
		public override string FunctionName => "JP Customs AFR";
		public override string FromClause => @"
(
	SELECT sl2.SL_Parent, MIN(sl2.SL_PostedTimeUtc) AS MinDate
	FROM 
		dbo.StmALog sl2
	WHERE
		sl2.SL_Parent IN (
			SELECT sl0.SL_Parent
			FROM
				dbo.StmALog sl0
			WHERE
				sl0.SL_PostedTimeUtc >= @StartDateTimeInclusive
				AND sl0.SL_PostedTimeUtc < @EndDateTimeExclusive
				AND sl0.SL_Parent is not null
				AND sl0.SL_SE_NKEvent = 'MSC'
				AND sl0.SL_Reference  = 'AHR-ACCEPTED'
				AND sl0.SL_Table = 'JPAFRHeader'
		)
		AND sl2.SL_PostedTimeUtc >= DATEADD(DAY, -60, @StartDateTimeInclusive)
		AND sl2.SL_PostedTimeUtc < @EndDateTimeExclusive
		AND sl2.SL_Parent is not null
		AND sl2.SL_SE_NKEvent = 'MSC'
		AND sl2.SL_Reference = 'AHR-ACCEPTED'
		AND sl2.SL_Table = 'JPAFRHeader'
		GROUP BY sl2.SL_Parent
) AS sl

INNER JOIN dbo.JPAFRHeader as jph ON jph.JPH_PK = sl.SL_Parent 
INNER JOIN dbo.JPAFRBills AS jpb ON jpb.JPB_JPH_Header = jph.JPH_PK
INNER JOIN dbo.GlbBranch as gb ON gb.GB_PK = jph.JPH_GB_Branch
INNER JOIN dbo.GlbCompany as gc ON gc.GC_PK = gb.GB_GC
";
		public override string TransactionCount => "1";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string BillingReference1 => "isnull(jph.JPH_JobReference, '')";
		public override string BillingReference2 => "isnull(jph.JPH_CarrierCode, '')";
		public override string BillingReference3 => "isnull(jph.JPH_MasterBillNumber, '')";
		public override string BillingReference4 => "isnull(jpb.JPB_BillNumber, '')";
		public override string WhereClause => "(jpb.JPB_IsMaterBill = 0)";
		public override string CreatingUserCode => "isnull(jph.JPH_SystemCreateUser, '')";
		public override string TransactionDateUtc => "sl.MinDate";
		public override string GuidReference => "jpb.JPB_PK";
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;

		#endregion
	}
}
