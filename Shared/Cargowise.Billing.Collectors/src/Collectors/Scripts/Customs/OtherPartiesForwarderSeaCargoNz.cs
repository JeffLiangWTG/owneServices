namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class OtherPartiesForwarderSeaCargoNz : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "SNZ";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Forwarder/CFS/CTO Functions";
		public override string FeatureName => "Import/Export Sea Cargo Report (NZ)";
		public override string CompanyCode => "ISNULL(gc.GC_Code, '')";
		public override string BranchCode => "ISNULL(gb.GB_Code, '')";
		public override string TransactionDateUtc => "ca.CA_SystemCreateTimeUtc";
		public override string BillingReference1 => "RTRIM(LEFT('MOBL: ' + cb.CB_OceanBill, 50))";
		public override string BillingReference2 => "RTRIM(LEFT('HOBL: ' + ca.CA_HouseBill, 50))";
		public override string GuidReference => "RTRIM(convert(varchar(36), ca.CA_PK))";
		public override string CreatingUserCode => "ISNULL(ca.CA_SystemCreateUser, '')";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					CusSCAOceanBill cb
					INNER JOIN dbo.CusSCAHouse ca on ca.CA_CB = cb.CB_PK
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = cb.CB_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
					cb.CB_ApplicationCode = 'TSW'
					AND gc.GC_RN_NKCountryCode = 'NZ'
					AND ca.CA_IsHVLV = 0";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.8.541";
	}
	#endregion
}
