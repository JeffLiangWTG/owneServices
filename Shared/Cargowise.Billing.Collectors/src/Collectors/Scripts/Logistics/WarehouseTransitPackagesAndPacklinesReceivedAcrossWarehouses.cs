using System;

namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class WarehouseTransitPackagesAndPacklinesReceivedAcrossWarehouses : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "WTR";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Transit Warehouse";
		public override string FeatureName => "Packages And Packlines Received (Across Warehouses)";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string TransactionDateUtc => "BillTime";
		public override string BillingReference1 => "PackageID COLLATE SQL_Latin1_General_CP1_CI_AS";
		public override string BillingReference2 => "Reference COLLATE SQL_Latin1_General_CP1_CI_AS";
		public override string BillingReference3 => "OuterPackageUnitType";
		public override string BillingReference4 => "NoOfWarehouses";
		public override string GuidReference => "KP_PK";
		public override string CreatingUserCode => "WPS_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string PreparationScript => $@"
IF OBJECT_ID(N'tempdb..#PackageStatesNeedToBeCollected') IS NOT NULL
BEGIN
DROP TABLE #PackageStatesNeedToBeCollected
END

CREATE TABLE #PackageStatesNeedToBeCollected (
	WPS_KP_Package UNIQUEIDENTIFIER,
	WPS_UnitType varchar(3),
	WPS_SystemCreateUser  varchar(3),
	WPS_WW_Warehouse UNIQUEIDENTIFIER,
	WPS_WRC_TransitReceiveConsignment UNIQUEIDENTIFIER,
	WPS_WRH_TransitReceiveHeader UNIQUEIDENTIFIER,
	TransactionTime datetimeoffset,
	KPH_PackageID varchar(46) COLLATE SQL_Latin1_General_CP1_CI_AS,
	KP_F3_NKPackType varchar(3),
	OuterPackageUnitType varchar(3),
	INDEX RC__WPS_KP_Package CLUSTERED(WPS_KP_Package ASC),
	INDEX RX__KPH_PackageID NONCLUSTERED (KPH_PackageID ASC)
);

WITH PackageStatesInDateRange AS (
	SELECT
		WPS_KP_Package,
		WPS_UnitType,
		WPS_SystemCreateUser,
		WPS_WW_Warehouse,
		WPS_WRC_TransitReceiveConsignment,
		WPS_WRH_TransitReceiveHeader,
		WPS_UnloadedTime AS TransactionTime,
		WPS_ReceivedAs
	FROM WhsItemPackageState
	WHERE
		WPS_UnloadedTime IS NOT NULL
		AND WPS_UnloadedTime >= {Constants.StartDateTimeInclusiveParamName}
		AND WPS_UnloadedTime < {Constants.EndDateTimeExclusiveParamName}
		AND WPS_WL_LastLocation IS NOT NULL

	UNION ALL

	SELECT
		WPS_KP_Package,
		WPS_UnitType,
		WPS_SystemCreateUser,
		WPS_WW_Warehouse,
		WPS_WRC_TransitReceiveConsignment,
		WPS_WRH_TransitReceiveHeader,
		CAST(WPS_SystemCreateTimeUtc AS DATETIMEOFFSET) AS TransactionTime,
		WPS_ReceivedAs
	FROM WhsItemPackageState
	WHERE
		WPS_UnloadedTime IS NULL
		AND WPS_SystemCreateTimeUtc >= CAST({Constants.StartDateTimeInclusiveParamName} AS smalldatetime)
		AND WPS_SystemCreateTimeUtc < CAST({Constants.EndDateTimeExclusiveParamName} AS smalldatetime)
		AND WPS_WL_LastLocation IS NOT NULL
)
INSERT INTO #PackageStatesNeedToBeCollected
SELECT
	WPS_KP_Package,
	WPS_UnitType,
	WPS_SystemCreateUser,
	WPS_WW_Warehouse,
	WPS_WRC_TransitReceiveConsignment,
	WPS_WRH_TransitReceiveHeader,
	TransactionTime,
	KPH_PackageID,
	KP_F3_NKPackType,
	OuterPackageUnitType
FROM
	PackageStatesInDateRange
	JOIN dbo.PkgPackage ON KP_PK = WPS_KP_Package
	LEFT JOIN dbo.PkgPackageHeader ON KP_KPH_PackageHeader = KPH_PK
	OUTER APPLY 
	(
		SELECT 
			TOP 1 KPD_UnpackedTime 
		FROM 
			PkgPackageHandlingUnitDivot 
		WHERE 
			KPD_KP_Package = KP_PK AND KPD_UnpackedTime IS NOT NULL
	) AS UnpackDivot
	OUTER APPLY
	(
		SELECT
			WPS_UnitType as OuterPackageUnitType
		FROM
			PkgPackageHandlingUnitDivot
			JOIN dbo.WhsItemPackageState on WPS_KP_Package = KPD_KP_HandlingUnit
		WHERE
			KPD_KP_Package = KP_PK AND KPD_UnpackedTime IS NULL
	) AS PackDivot
WHERE
	KP_KP_ParentPackage IS NULL
	OR WPS_ReceivedAs = 'SCN'
	OR WPS_ReceivedAs = 'SKP'
	OR UnpackDivot.KPD_UnpackedTime IS NOT NULL

;WITH PackageIDsInDateRange AS (
	SELECT DISTINCT
		KPH_PackageID
	FROM
		#PackageStatesNeedToBeCollected
)
,RCNIDsInDateRange AS (
	SELECT DISTINCT
		WRC_ConsignmentID
	FROM
		#PackageStatesNeedToBeCollected
		JOIN WhsItemReceiveConsignment ON WRC_PK = WPS_WRC_TransitReceiveConsignment
)
, PackagesWithSameIDInAllWarehouses AS (
	SELECT
		KP_PK,
		NoOfVisitsByPkgID = 
			ROW_NUMBER() OVER (
					PARTITION BY
						PkgPackageHeader.KPH_PackageID
					ORDER BY
						ISNULL(WPS_UnloadedTime, CAST(WPS_SystemCreateTimeUtc AS DATETIMEOFFSET))
				)
	FROM
		PackageIDsInDateRange
		JOIN PkgPackageHeader ON PkgPackageHeader.KPH_PackageID = PackageIDsInDateRange.KPH_PackageID COLLATE SQL_Latin1_General_CP1_CI_AS
		JOIN PkgPackage ON KP_KPH_PackageHeader = PkgPackageHeader.KPH_PK
		JOIN WhsItemPackageState ON WPS_KP_Package = KP_PK
	WHERE
		WPS_WL_LastLocation IS NOT NULL
)
, ConsignmentsWithSameIDInAllWarehouses AS (
	SELECT
		WRC_PK,
		NoOfVisitsByRCNID = 
			DENSE_RANK() OVER (
					PARTITION BY
						WhsItemReceiveConsignment.WRC_ConsignmentID
					ORDER BY
						WRC_SystemCreateTimeUtc
				)
	FROM
		RCNIDsInDateRange
		JOIN WhsItemReceiveConsignment ON WhsItemReceiveConsignment.WRC_ConsignmentID = RCNIDsInDateRange.WRC_ConsignmentID
)
, PackageInfoWithoutCountByConsignmentID AS (
	SELECT
		KP_PK,
		NULL AS KPH_PackageID,
		#PackageStatesNeedToBeCollected.KP_F3_NKPackType,
		GC_Code,
		GB_Code,
		WW_WarehouseCode,
		WW_PK,
		ISNULL(WRC_ConsignmentID, CONCAT(WPS_UnitType, ' (IN PROGRESS)')) AS Reference,
		PackageQtySum = KP_PackageQty,
		WPS_SystemCreateUser,
		WRC_ConsignmentID,
		WRC_PK,
		NoOfVisitsByPkgID = 1,
		TransactionTime = TransactionTime,
		#PackageStatesNeedToBeCollected.OuterPackageUnitType
	FROM
		#PackageStatesNeedToBeCollected
		JOIN dbo.PkgPackage ON WPS_KP_Package = KP_PK
		JOIN dbo.WhsWarehouse ON WW_PK = WPS_WW_Warehouse
		JOIN dbo.GlbBranch ON GB_PK = WW_GB_RelatedCompanyBranch
		JOIN dbo.GlbCompany ON GC_PK = GB_GC
		LEFT JOIN dbo.WhsItemReceiveConsignment ON WRC_PK = WPS_WRC_TransitReceiveConsignment
	WHERE
		KP_KPH_PackageHeader IS NULL

	UNION ALL

	SELECT
		KP_PK,
		#PackageStatesNeedToBeCollected.KPH_PackageID,
		KP_F3_NKPackType,
		GC_Code,
		GB_Code,
		WW_WarehouseCode,
		WW_PK,
		ISNULL(WRC_ConsignmentID, WRH_ReferenceNumber) AS Reference,
		PackageQtySum = 1,
		#PackageStatesNeedToBeCollected.WPS_SystemCreateUser,
		WRC_ConsignmentID,
		WRC_PK,
		NoOfVisitsByPkgID,
		#PackageStatesNeedToBeCollected.TransactionTime,
		#PackageStatesNeedToBeCollected.OuterPackageUnitType
	FROM
		#PackageStatesNeedToBeCollected
		JOIN PackagesWithSameIDInAllWarehouses on #PackageStatesNeedToBeCollected.WPS_KP_Package = PackagesWithSameIDInAllWarehouses.KP_PK
		JOIN dbo.WhsWarehouse ON WW_PK = #PackageStatesNeedToBeCollected.WPS_WW_Warehouse
		JOIN dbo.GlbBranch ON GB_PK = WW_GB_RelatedCompanyBranch
		JOIN dbo.GlbCompany ON GC_PK = GB_GC
		LEFT JOIN dbo.WhsItemReceiveConsignment ON WRC_PK = #PackageStatesNeedToBeCollected.WPS_WRC_TransitReceiveConsignment
		LEFT JOIN dbo.WhsItemReceiveTransportationUnit ON WRH_PK = #PackageStatesNeedToBeCollected.WPS_WRH_TransitReceiveHeader
)
, PackageInfo AS (
	SELECT
		PackageInfoWithoutCountByConsignmentID.*,
		NoOfVisitsByConsignmentID =
		CASE
			WHEN NoOfVisitsByRCNID IS NULL THEN 1
			ELSE NoOfVisitsByRCNID
		END
	FROM
		PackageInfoWithoutCountByConsignmentID
		LEFT JOIN ConsignmentsWithSameIDInAllWarehouses ON ConsignmentsWithSameIDInAllWarehouses.WRC_PK = PackageInfoWithoutCountByConsignmentID.WRC_PK
)
";

		public override string FromClause => $@"
		(
			SELECT
				PackageInfo.KP_PK,
				ISNULL (
					PackageInfo.KPH_PackageID,
					CONCAT (PackageInfo.PackageQtySum, ' ', PackageInfo.KP_F3_NKPackType)
				) AS PackageID,
				PackageInfo.GC_Code,
				PackageInfo.GB_Code,
				PackageInfo.WW_WarehouseCode,
				PackageInfo.Reference,
				PackageInfo.WPS_SystemCreateUser,
				PackageInfo.TransactionTime AS BillTime,
				PackageInfo.PackageQtySum,
				PackageInfo.OuterPackageUnitType,
				IIF(PackageInfo.NoOfVisitsByPkgID >= PackageInfo.NoOfVisitsByConsignmentID, PackageInfo.NoOfVisitsByPkgID, PackageInfo.NoOfVisitsByConsignmentID) AS NoOfWarehouses
			FROM
				PackageInfo
		) AS PackageGroups
";
		public override string WhereClause => "";
		public override string TransactionCount => "PackageQtySum";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string DateType => RefStlDateType.DateTimeOffset;
		public override string MinCW1Version => "24.4.19.259";
		// CollectionStartDateUtc is for the start date that the collector will be run at the first time only
		public override DateTime CollectionStartDateUtc => new DateTime(2024, 5, 31);
	}

	#endregion
}
