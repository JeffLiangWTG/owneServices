namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class WarehouseTransitPackagesDispatched : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "WTD";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Transit Warehouse";
		public override string FeatureName => "Packages Dispatched";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "wps.WPS_LoadedTime";
		public override string BillingReference1 => "pid.KPH_PackageID";
		public override string BillingReference2 => "dtu.WDH_ReferenceNumber";
		public override string GuidReference => "kp.KP_PK";
		public override string CreatingUserCode => "dtu.WDH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
			WhsItemDispatchTransportationUnit dtu
			JOIN dbo.WhsItemPackageState wps ON wps.WPS_WDH_TransitDispatchHeader = dtu.WDH_PK
			JOIN dbo.WhsWarehouse ww ON ww.WW_PK = dtu.WDH_WW_Warehouse
			JOIN dbo.GlbBranch gb ON gb.GB_PK = ww.WW_GB_RelatedCompanyBranch
			JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
			JOIN dbo.PkgPackage kp ON kp.KP_PK = WPS_KP_Package
			JOIN dbo.PkgPackageHeader pid ON kp.KP_KPH_PackageHeader = pid.KPH_PK";
		public override string WhereClause => "";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTimeOffset;
		public override string MinCW1Version => "23.2.24.897";
	}

	#endregion
}
