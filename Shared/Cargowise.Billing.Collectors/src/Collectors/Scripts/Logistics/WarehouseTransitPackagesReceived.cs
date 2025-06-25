namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class WarehouseTransitPackagesReceived : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "WTP";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Transit Warehouse";
		public override string FeatureName => "Packages Received";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "wps.WPS_UnloadedTime";
		public override string BillingReference1 => "pid.KPH_PackageID";
		public override string BillingReference2 => "ISNULL(wrc.WRC_JobID, rtu.WRH_ReferenceNumber)";
		public override string GuidReference => "kp.KP_PK";
		public override string CreatingUserCode => "rtu.WRH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
			WhsItemReceiveTransportationUnit rtu
			JOIN dbo.WhsItemPackageState wps ON wps.WPS_WRH_TransitReceiveHeader = rtu.WRH_PK
			JOIN dbo.WhsWarehouse ww ON ww.WW_PK = rtu.WRH_WW_Warehouse
			JOIN dbo.GlbBranch gb ON gb.GB_PK = ww.WW_GB_RelatedCompanyBranch
			JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
			JOIN dbo.PkgPackage kp ON kp.KP_PK = WPS_KP_Package
			JOIN dbo.PkgPackageHeader pid ON kp.KP_KPH_PackageHeader = pid.KPH_PK
			LEFT JOIN dbo.WhsItemReceiveConsignment wrc ON wrc.WRC_PK = wps.WPS_WRC_TransitReceiveConsignment";
		public override string WhereClause => "";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTimeOffset;
		public override string MinCW1Version => "23.2.24.897";
	}

	#endregion
}
