namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class CfsShipmentManagement : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "CFP";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "CFS/Freight Shed Manager";
		public override string FunctionName => "Container Freight Station / Air Freight Shed";
		public override string FeatureName => "Shipment Management";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "jh.JH_SystemCreateTimeUtc";
		public override string BillingReference1 => "jh.JH_JobNum";
		public override string BillingReference2 => "js.JS_UniqueConsignRef";
		public override string GuidReference => "jh.JH_PK";
		public override string CreatingUserCode => "jh.JH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobHeader jh
					INNER JOIN dbo.JobConsol jk ON jk.JK_PK = jh.JH_ParentID
					INNER JOIN dbo.JobConShipLink jn ON jn.JN_JK = jk.JK_PK
					INNER JOIN dbo.JobShipment js ON js.JS_PK = jn.JN_JS
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = jh.JH_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
					LEFT OUTER JOIN dbo.JobShipment ms ON ms.JS_PK = js.JS_JS_ColoadMasterShipment";
		public override string WhereClause => @"
					jk.JK_IsCFS = 1
					AND js.JS_IsCFSRegistered = 1
					AND js.JS_ShipmentType <> 'ASM'
					AND (js.JS_JS_ColoadMasterShipment IS NULL OR ms.JS_ShipmentType NOT IN ('CLD', 'CLB'))";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.3.16.344";
	}

	#endregion
}
