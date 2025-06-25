namespace CargoWise.Billing.Collectors.Logistics
{
	public class WarehouseTransitOuterPackagesReceivedForStatistics : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "WST";
		public override string FeatureName => "Packages Received Statistics";
		public override string TransactionDateUtc => "TransactionTime";
		public override string GuidReference => "KP_PK";
		public override string FromClause => $@"
			(
				SELECT
					KP_PK,
					WW_WarehouseCode,
					ISNULL (
						KPH_PackageID,
						KP_F3_NKPackType + '*' + CAST(KP_PackageQty AS VARCHAR)
					) AS PackageID,
					GC_Code,
					GB_Code,
					KP_PackageQty,
					KP_F3_NKPackType,
					WPS_UnitType,
					WRC_Direction AS Direction,
					ISNULL (
						WDL_TransportMode,
						ISNULL (WDC_TransportMode, WRC_TransportMode)
					) AS TransportMode,
					WRH_UnitType,
					TransactionTime,
					WeightInKG,
					VolumeInM3,
					CASE
						WHEN KP_PK IN (
							SELECT
								CE_ParentID
							FROM
								CusEntryNum
							WHERE
								CE_EntryType = 'CEN'
								AND CE_ParentTable = 'PkgPackage'
						) THEN 1
						ELSE 0
					END AS HasCEN
				FROM
					(
						SELECT
							WPS_KP_Package,
							WPS_UnitType,
							WPS_WW_Warehouse,
							WPS_WRC_TransitReceiveConsignment,
							WPS_WRH_TransitReceiveHeader,
							WPS_WDC_TransitDispatchConsignment,
							WPS_WDL_LoadList,
							WPS_UnloadedTime AS TransactionTime
						FROM
							dbo.WhsItemPackageState
						WHERE
							WPS_WL_LastLocation IS NOT NULL
							AND WPS_UnloadedTime IS NOT NULL
							AND WPS_UnloadedTime >= {Constants.StartDateTimeInclusiveParamName}
							AND WPS_UnloadedTime < {Constants.EndDateTimeExclusiveParamName}
						UNION ALL
						SELECT
							WPS_KP_Package,
							WPS_UnitType,
							WPS_WW_Warehouse,
							WPS_WRC_TransitReceiveConsignment,
							WPS_WRH_TransitReceiveHeader,
							WPS_WDC_TransitDispatchConsignment,
							WPS_WDL_LoadList,
							CAST(WPS_SystemCreateTimeUtc AS DATETIMEOFFSET) AS TransactionTime
						FROM
							dbo.WhsItemPackageState
						WHERE
							WPS_WL_LastLocation IS NOT NULL
							AND WPS_UnloadedTime IS NULL
							AND CAST(WPS_SystemCreateTimeUtc AS DATETIMEOFFSET) >= {Constants.StartDateTimeInclusiveParamName}
							AND CAST(WPS_SystemCreateTimeUtc AS DATETIMEOFFSET) < {Constants.EndDateTimeExclusiveParamName}
					) as PackageStates
					JOIN dbo.PkgPackage ON KP_PK = WPS_KP_Package 
					CROSS APPLY (
						SELECT
							WeightInKG = CAST(ROUND(SUM(Value), 2) as NUMERIC(36, 2))
						FROM
							dbo.ConvertWeight (KP_Weight, KP_WeightUQ, 'KG')
					) AS ConvertedWeight 
					CROSS APPLY (
						SELECT
							VolumeInM3 = CAST(ROUND(SUM(Value), 2) as NUMERIC(36, 2))
						FROM
							dbo.ConvertVolume (KP_Volume, KP_VolumeUQ, 'M3')
					) AS ConvertedVolume
					JOIN dbo.WhsWarehouse ON WW_PK = WPS_WW_Warehouse
					JOIN dbo.GlbBranch ON GB_PK = WW_GB_RelatedCompanyBranch
					JOIN dbo.GlbCompany ON GC_PK = GB_GC
					LEFT JOIN dbo.PkgPackageHeader ON KPH_PK = KP_KPH_PackageHeader
					LEFT JOIN dbo.WhsItemReceiveConsignment ON WRC_PK = WPS_WRC_TransitReceiveConsignment
					LEFT JOIN dbo.WhsItemReceiveTransportationUnit ON WRH_PK = WPS_WRH_TransitReceiveHeader
					LEFT JOIN dbo.WhsItemDispatchConsignment ON WDC_PK = WPS_WDC_TransitDispatchConsignment
					LEFT JOIN dbo.WhsItemDispatchLoadList ON WDL_PK = WPS_WDL_LoadList
				WHERE
					KP_KP_ParentPackage IS NULL
			) AS ReceivedOuterPackages";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Transit Warehouse";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string BillingReference1 => "WW_WarehouseCode";
		public override string BillingReference2 => "KP_PK";
		public override string BillingReference3 => "PackageID";
		public override string WhereClause => "";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string DateType => RefStlDateType.DateTimeOffset;
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
			(
				SELECT
					PkgQty = ReceivedOuterPackages.KP_PackageQty,
					PackageType = ReceivedOuterPackages.KP_F3_NKPackType,
					UnitType = ReceivedOuterPackages.WPS_UnitType,
					Direction = ReceivedOuterPackages.Direction,
					TransportMode = ReceivedOuterPackages.TransportMode,
					TransportUnitType = ReceivedOuterPackages.WRH_UnitType,
					WeightInKG = ReceivedOuterPackages.WeightInKG,
					VolumeInM3 = ReceivedOuterPackages.VolumeInM3,
					HasCEN = ReceivedOuterPackages.HasCEN
				FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
			)))";
		public override string MinCW1Version => "24.8.10.159";
	}
}
