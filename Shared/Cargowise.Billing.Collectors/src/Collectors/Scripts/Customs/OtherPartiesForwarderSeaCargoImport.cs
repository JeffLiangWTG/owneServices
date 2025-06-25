namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class OtherPartiesForwarderSeaCargoImport : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "SCI";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Forwarder/CFS/CTO Functions";
		public override string FeatureName => "Import/Export Sea Cargo Report/Port Community Messaging (Import)";
		public override string CompanyCode => "OceanBill.GC_Code";
		public override string BranchCode => "OceanBill.GB_Code";
		public override string TransactionDateUtc => "OceanBill.CB_SystemCreateTimeUtc";
		public override string BillingReference1 => "('OBL: ' + OceanBill.CB_OceanBill)";
		public override string BillingReference2 => "OceanBill.CA_HouseBill";
		public override string GuidReference => "OceanBill.CA_PK";
		public override string CreatingUserCode => "OceanBill.CB_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
(
	SELECT
		DISTINCT GC_Code, CB_SystemCreateTimeUtc, CB_OceanBill, CA_HouseBill, CA_PK, GB_Code, CB_SystemCreateUser
	FROM
		dbo.CusScaOceanBill cb
		INNER JOIN dbo.CusScaHouse ca on ca.CA_CB = cb.CB_PK AND ca.CA_IsHVLV = 0
		INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = cb.CB_GB
		INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
	WHERE
		cb.CB_SystemCreateTimeUtc >= @StartDateTimeInclusive
		AND cb.CB_SystemCreateTimeUtc < @EndDateTimeExclusive
		AND cb.CB_ApplicationCode = 'CMR'
		AND gc.GC_RN_NKCountryCode = 'AU'
)OceanBill
";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "22.12.8.541";
	}
	#endregion
}
