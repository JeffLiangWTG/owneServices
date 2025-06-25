namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class OtherPartiesForwarderAirCargoReportImport : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "ACM";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Forwarder/CFS/CTO Functions";
		public override string FeatureName => "Import/Export Air Cargo Report (Import Air)";
		public override string CompanyCode => "hawb.GC_Code";
		public override string BranchCode => "hawb.GB_Code";
		public override string TransactionDateUtc => "hawb.CS_SystemCreateTimeUtc";
		public override string BillingReference1 => "'MAWB: ' + hawb.CM_MAWB";
		public override string BillingReference2 => "'HAWB: ' + hawb.CS_HAWB";
		public override string BillingReference3 => "hawb.CM_MasterHouseBill";
		public override string BillingReference4 => "hawb.CS_MessageReference";
		public override string GuidReference => "hawb.CS_PK";
		public override string CreatingUserCode => "hawb.CS_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					(
						SELECT
							cs.CS_PK, cs.CS_HAWB, cs.CS_MessageReference, cs.CS_SystemCreateTimeUtc, cs.CS_SystemCreateUser,
							cm.CM_MAWB, cm.CM_GB, cm.CM_MasterHouseBill,
							gc.GC_Code, gb.GB_Code
						FROM
							dbo.CusMawb cm
							INNER JOIN dbo.CusHawb cs ON cs.CS_CM = cm.CM_PK
							INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = cm.CM_GB
							INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
						WHERE
							cs.CS_SystemCreateTimeUtc >= @StartDateTimeInclusive
							AND cs.CS_SystemCreateTimeUtc < @EndDateTimeExclusive
							AND cm.CM_IsCtoMawb = 0
							AND cm.CM_ApplicationCode = 'CMR'
							AND gc.GC_RN_NKCountryCode = 'AU'
							AND cs.CS_IsHVLV = 0
					) AS hawb";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "22.12.8.541";
	}
	#endregion
}
