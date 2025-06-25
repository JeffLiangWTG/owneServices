namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class WarehouseTransitPackagesReceivedAcrossWarehouses_Pre_24_8_10_159 : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "WTU";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Transit Warehouse";
		public override string FeatureName => "Packages Received (Across Warehouses)";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string TransactionDateUtc => "TransactionTime";
		public override string BillingReference1 => "KPH_PackageID";
		public override string BillingReference2 => "Reference";
		public override string BillingReference3 => "KPH_PK";
		public override string BillingReference4 => "NoOfWarehouses";
		public override string GuidReference => "KP_PK";
		public override string CreatingUserCode => "WPS_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => $@"
			(
				SELECT
					KP_PK,
					NoOfWarehouses = ROW_NUMBER() OVER (PARTITION BY KPH_PackageID ORDER BY ISNULL(WPS_UnloadedTime, CAST(WPS_SystemCreateTimeUtc AS DATETIMEOFFSET))),
					KPH_PK = FIRST_VALUE(KPH_PK) OVER(PARTITION BY KPH_PackageID ORDER BY ISNULL(WPS_UnloadedTime, CAST(WPS_SystemCreateTimeUtc AS DATETIMEOFFSET))),
					KPH_PackageID,
					GC_Code,
					GB_Code,
					TransactionTime = ISNULL(WPS_UnloadedTime, CAST(WPS_SystemCreateTimeUtc AS DATETIMEOFFSET)),
					WPS_SystemCreateUser,
					ISNULL(WRC_JobID, WRH_ReferenceNumber) AS Reference
				FROM
					dbo.WhsItemPackageState
					JOIN dbo.WhsWarehouse ON WW_PK = WPS_WW_Warehouse
					JOIN dbo.GlbBranch ON GB_PK = WW_GB_RelatedCompanyBranch
					JOIN dbo.GlbCompany ON GC_PK = GB_GC
					JOIN dbo.PkgPackage on KP_PK = WPS_KP_Package
					JOIN dbo.PkgPackageHeader ON KPH_PK = KP_KPH_PackageHeader
					LEFT JOIN dbo.WhsItemReceiveTransportationUnit ON WRH_PK = WPS_WRH_TransitReceiveHeader
					LEFT JOIN dbo.WhsItemReceiveConsignment ON WRC_PK = WPS_WRC_TransitReceiveConsignment
				WHERE KPH_PackageID IN
				(
					SELECT KPH_PackageID 
					FROM dbo.WhsItemPackageState
					JOIN dbo.PkgPackage ON KP_PK = WPS_KP_Package 
					JOIN dbo.PkgPackageHeader ON KP_KPH_PackageHeader = KPH_PK 
					WHERE ISNULL(WPS_UnloadedTime, CAST(WPS_SystemCreateTimeUtc AS DATETIMEOFFSET)) >= {Constants.StartDateTimeInclusiveParamName} AND ISNULL(WPS_UnloadedTime, CAST(WPS_SystemCreateTimeUtc AS DATETIMEOFFSET)) < {Constants.EndDateTimeExclusiveParamName} AND WPS_WL_LastLocation IS NOT NULL
				)
				AND WPS_WL_LastLocation IS NOT NULL
			) AS PackageIDGroups";
		public override string WhereClause => "";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string DateType => RefStlDateType.DateTimeOffset;
		public override string MaxCW1Version => "24.8.10.158";
	}

	#endregion
}
