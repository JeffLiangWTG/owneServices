namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class EXLVExportAirSeaFreightManifestConsignments : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "ELV";
		public override string RoleName => "EXLV Export Air/Sea Freight Consignments";
		public override string ModuleName => "HVLV Export Air Sea Freight Manifest Consignments ";
		public override string FunctionName => "EXLV Customs Functions Only";
		public override string FeatureName => "EXLV Export Sub Manifest";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "dl.DL_SystemCreateTimeUtc";
		public override string BillingReference1 => "CASE WHEN dl.DL_ConsigneeReference = '' THEN convert(varchar(36), dl.DL_PK) ELSE dl.DL_ConsigneeReference END";
		public override string BillingReference2 => "js.JS_UniqueConsignRef";
		public override string BillingReference3 => "jk.JK_UniqueConsignRef";
		public override string BillingReference4 => "js.JS_TransportMode";
		public override string GuidReference => "dl.DL_PK";
		public override string CreatingUserCode => "dl.DL_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"SupplierBookingLine dl
INNER JOIN dbo.JobShipment js ON js.JS_PK = dl.DL_JS_ApprovedShipment
LEFT JOIN (
		JobConShipLink jn
		INNER JOIN dbo.JobConsol jk ON jk.JK_PK = jn.JN_JK
	) ON jn.JN_JS = js.JS_PK

LEFT JOIN (
		JobHeader jh 
		INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = jh.JH_GB
		INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
	) ON jh.JH_ParentID = js.JS_PK AND jh.JH_ParentTableCode = 'JS' 
";
		public override string WhereClause => @"
		(
		js.JS_ShipmentType = 'HLS'
		AND js.JS_RL_NKOrigin like 'AU%'
		AND js.JS_IsForwardRegistered = 1
		)
		AND (gc.GC_PK is NUll OR gc.GC_RN_NKCountryCode = 'AU')
";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.13.467";
	}

	#endregion
}
