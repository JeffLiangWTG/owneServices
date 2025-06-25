namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class eTailImportAirFreightManifestConsignments : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "ETA";
		public override string RoleName => "eTail Import Air Freight Consignments";
		public override string ModuleName => "eTail Import Air Freight Manifest Consignments";
		public override string FunctionName => "eTail Portal and Customs Functions";
		public override string FeatureName => "eTail Import Air Cargo Reporting";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "hvc.HVC_SystemCreateTimeUtc";
		public override string BillingReference1 => "CASE WHEN hvc.HVC_ConsignmentId = '' THEN convert(varchar(36), hvc.HVC_PK) ELSE hvc.HVC_ConsignmentId END";
		public override string BillingReference2 => "CASE WHEN hvc.HVC_ShipperReference = '' THEN convert(varchar(36), hvc.HVC_PK) ELSE hvc.HVC_ShipperReference END";
		public override string BillingReference3 => "jk.JK_UniqueConsignRef";
		public override string BillingReference4 => "js.JS_UniqueConsignRef";
		public override string GuidReference => "hvc.HVC_PK";
		public override string CreatingUserCode => "hvc.HVC_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"HVLVConsignment hvc
INNER JOIN dbo.HVLVBookingHeader hvh ON hvh.HVH_PK = hvc.HVC_HVH_BookingHeader
INNER JOIN dbo.HVLVItem hvi ON hvi.HVI_HVC_Consignment = hvc.HVC_PK
INNER JOIN dbo.JobShipment js ON js.JS_PK = hvi.HVI_JS_LoadedOnShipment
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
		js.JS_ShipmentType = 'HVL'
		AND js.JS_TransportMode = 'AIR'
		AND js.JS_RL_NKDestination like 'AU%'
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
