using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Get Packages for Packing.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PackageForPackingWebServiceResponse GetPackagesForPacking(string packageID)
		{
			return HandleWebServiceRequest<PackageForPackingWebServiceResponse>(r => GetPackagesForPackingCore(r, packageID));
		}

		void GetPackagesForPackingCore(PackageForPackingWebServiceResponse response, string packageID)
		{
			if (string.IsNullOrEmpty(packageID))
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("12C1FA40-D9F1-437F-89BD-0817E04E322A", "Please provide a Tote Number or Package ID.");
			}
			else
			{
				var packageInfo = GetPackagesForPackingInfo(packageID, null);
				if (!packageInfo.Any())
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("07C63426-3351-48CA-8E48-D6D855587313", "Tote '{0}' cannot be found or is not valid for Packing.", packageID);
				}
				else
				{
					response.PackagesForPackingInfo = new PackageForPackingInfoCollection(packageInfo);
				}
			}
		}

		PackageForPackingInfo[] GetPackagesForPackingInfo(string toteNumber, Guid? packagePK)
		{
			var packageInfos = new List<PackageForPackingInfo>();

			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode).PK;
			var sqlParams = new ZSqlParameterCollection
			{
				{ "@Warehouse", warehouse, WhsDocketSchema.WD_WW_Whs },
				{ "@PackageID", toteNumber, PkgPackageHeaderSchema.KPH_PackageID }
			};

			if (SecurityHeader.IsAndroidDevice)
			{
				var sql = BuildConsolidationHUQuery();

				var packageData = new DynamicBusinessObjectCollection(Factory);
				packageData.Load(sql, sqlParams);

				ProcessConsolidationHUQueryResults(packageData, packageInfos);
			}

			if (packageInfos.Count == 0)
			{
				var sql = BuildGetPackagesForPackingSqlQuery(packagePK);

				if (packagePK.HasValue)
				{
					sqlParams.Add("@PackagePK", packagePK, PkgPackageSchema.PK);
				}

				var packageData = new DynamicBusinessObjectCollection(Factory);
				packageData.Load(sql, sqlParams);

				foreach (var package in packageData)
				{
					var weightUQ = (ZString)package[PkgPackageSchema.Constants.KP_WeightUQ];
					if (weightUQ.IsEmpty)
					{
						weightUQ = PackingRegistry.Instance.WeightUnit.Value;
					}

					var dimensionUQ = (ZString)package[PkgPackageSchema.Constants.KP_DimensionUQ];
					if (dimensionUQ.IsEmpty)
					{
						dimensionUQ = PackingRegistry.Instance.DimensionUnit.Value;
					}

					packageInfos.Add(new PackageForPackingInfo(
						((ZGuid)package[PkgPackageSchema.Constants.PK]).ToGuid(),
						(ZString)package[WhsDocketSchema.Constants.WD_ExternalReference],
						(ZString)package[WhsDocketSchema.Constants.WD_DocketID],
						(ZBool)package[nameof(PackageForPackingInfo.IsUsingCarrierLabelIntegration)],
						(ZString)package[PkgPackageHeaderSchema.Constants.KPH_PackageID],
						(ZDecimal)package[PkgPackageSchema.Constants.KP_Weight],
						(ZDecimal)package[PkgPackageSchema.Constants.KP_TareWeight],
						weightUQ,
						(ZDecimal)package[OrgMiscServSchema.Constants.OM_WhsPackageWeightTolerancePercent],
						(ZBool)package[OrgMiscServSchema.Constants.OM_WhsPackageToleranceEnabled],
						(ZDecimal)package[PkgPackageSchema.Constants.KP_Length],
						(ZDecimal)package[PkgPackageSchema.Constants.KP_Width],
						(ZDecimal)package[PkgPackageSchema.Constants.KP_Height],
						dimensionUQ,
						(ZString)package[WhsDocketSchema.Constants.WD_DocketStatus],
						((ZDateTimeOffset)package[WhsDocketSchema.Constants.WD_RequiredDate]).ToDateTime(),
						(ZString)package[PkgPackageJobSchema.Constants.KJ_JobID],
						(ZString)package[OrgHeaderSchema.Constants.OH_Code],
						(ZBool)package[nameof(PackageForPackingInfo.IsUsingCartonSizes)],
						(ZBool)package[nameof(PackageForPackingInfo.IsTote)],
						(ZBool)package[OrgMiscServSchema.Constants.OM_WhsEnforceScanOfProductsWhenPackingTote],
						(ZBool)package[nameof(PackageForPackingInfo.IsConsolidationHandlingUnit)],
						(ZBool)package[WhsDocketSchema.Constants.WD_UseDirectedPackingConsolidation],
						RequiresPackagePutaway((ZString)package[WhsPackageLocationViewSchema.Constants.WPK_LocationClass], (ZString)package[WhsPackageLocationViewSchema.Constants.WPK_PackedInventoryStatus]),
						(ZString)package[WhsLocationViewSchema.Constants.WLV_LocationString],
						(ZString)package[WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly],
						(ZString)package[nameof(WhsLocationInfo.LocationString)],
						(ZString)package[nameof(WhsLocationInfo.LocationString_UserFriendly)],
						(ZString)package["LocationClass"],
						allowedToOverrideDockDoorLocation: (ZBool)package["AllowDDLOverride"]));
				}
			}

			return packageInfos.ToArray();

			bool RequiresPackagePutaway(ZString packageCurrentLocationClass, ZString packedInventoryStatus)
			{
				return packageCurrentLocationClass.IsEmpty
					|| !(packageCurrentLocationClass.EqualsIgnoringCase(LocationClasses.Codes.CON) || packageCurrentLocationClass.EqualsIgnoringCase(LocationClasses.Codes.DDL))
					|| packedInventoryStatus.EqualsIgnoringCase(InventoryStatus.Codes.InTransit);
			}
		}

		void ProcessConsolidationHUQueryResults(DynamicBusinessObjectCollection packageData, List<PackageForPackingInfo> packageInfos)
		{
			if (packageData.Count > 0)
			{
				var package = packageData[0];

				var weightUQ = (ZString)package[PkgPackageSchema.Constants.KP_WeightUQ];
				if (weightUQ.IsEmpty)
				{
					weightUQ = PackingRegistry.Instance.WeightUnit.Value;
				}

				var dimensionUQ = (ZString)package[PkgPackageSchema.Constants.KP_DimensionUQ];
				if (dimensionUQ.IsEmpty)
				{
					dimensionUQ = PackingRegistry.Instance.DimensionUnit.Value;
				}

				packageInfos.Add(new PackageForPackingInfo(
						((ZGuid)package[PkgPackageSchema.Constants.PK]).ToGuid(),
						ZString.Empty,
						ZString.Empty,
						(ZBool)package[nameof(PackageForPackingInfo.IsUsingCarrierLabelIntegration)],
						(ZString)package[PkgPackageHeaderSchema.Constants.KPH_PackageID],
						(ZDecimal)package[PkgPackageSchema.Constants.KP_Weight],
						(ZDecimal)package[PkgPackageSchema.Constants.KP_TareWeight],
						weightUQ,
						0m,
						false,
						(ZDecimal)package[PkgPackageSchema.Constants.KP_Length],
						(ZDecimal)package[PkgPackageSchema.Constants.KP_Width],
						(ZDecimal)package[PkgPackageSchema.Constants.KP_Height],
						dimensionUQ,
						ZString.Empty,
						DateTime.MinValue,
						ZString.Empty,
						ZString.Empty,
						false,
						false,
						false,
						isConsolidationHandlingUnit: true));
			}
		}

		#region Queries

		string BuildGetPackagesForPackingSqlQuery(Guid? packagePK)
		{
			return Invariant($@"
SELECT DISTINCT
	WD_ExternalReference,
	WD_DocketID,
	WD_DocketStatus,
	WD_RequiredDate,
	KJ_JobID,
	KP_PK,
	KPH_PackageID,
	KP_Weight,
	KP_TareWeight,
	KP_WeightUQ,
	KP_Length,
	KP_Width,
	KP_Height,
	KP_DimensionUQ,
	OH_Code,
	OM_WhsPackageWeightTolerancePercent,
	OM_WhsPackageToleranceEnabled,
	OM_WhsEnforceScanOfProductsWhenPackingTote,
	CASE WHEN WPP_IsUsingCartonSizes is null THEN CAST(1 AS BIT) ELSE WPP_IsUsingCartonSizes END AS IsUsingCartonSizes,
	CAST(CASE WHEN E2_OA_Address IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsUsingCarrierLabelIntegration,
	CAST(CASE WHEN XA_Data = 'Y' THEN 1 ELSE 0 END AS BIT) AS IsTote,
	CAST(0 AS BIT) AS IsConsolidationHandlingUnit,
	WD_UseDirectedPackingConsolidation,
	WLV_LocationString,
	WLV_LocationString_UserFriendly,
	WPK_LocationClass,
	WPK_PackedInventoryStatus,
	AssignedPutawayLocationFromPackingStation.LocationString,
	AssignedPutawayLocationFromPackingStation.LocationString_UserFriendly,
	AssignedPutawayLocationFromPackingStation.LocationClass,
	CASE
		WHEN WDA_FirstPutawayToDockDoorUtc IS NULL AND ClientAllowsDDLOverride = 1 THEN CAST(1 AS BIT)
		ELSE CAST(0 AS BIT)
	END AS AllowDDLOverride
FROM
	dbo.PkgPackage
	JOIN dbo.PkgPackageHeader ON KP_KPH_PackageHeader = KPH_PK
	JOIN dbo.PkgPackageJob ON KP_KJ_ParentPackageJob = KJ_PK
	LEFT JOIN dbo.GenAddOnColumn ON XA_ParentID = KP_PK AND XA_Name = '{PkgPackageExtensions.TotePackage}'
	JOIN dbo.WhsDocket ON KJ_ParentID = WD_PK
	LEFT JOIN dbo.JobDocAddress ON E2_ParentID = WD_PK AND E2_AddressType = '{DocAddressTypes.Codes.CarrierBookingAgent}' AND E2_AddressSequence = 0
	JOIN dbo.WhsPick ON WD_WP = WP_PK
	LEFT JOIN dbo.WhsDockDoorAssignment ON WDA_PK = WP_WDA_DockDoorAssignment
	JOIN dbo.WhsLocationView ON WLV_PK = ISNULL(WDA_WL_AssignedDockDoor, WP_WL_DockDoor)
	JOIN dbo.OrgHeader ON OH_PK = WD_OH_Client
	JOIN dbo.OrgMiscServ ON OM_OH = OH_PK
	LEFT JOIN dbo.WhsPackageLocationView ON KP_PK = WPK_KP_Package
	OUTER APPLY
	(
		SELECT TOP 1 WPP_IsUsingCartonSizes
		FROM
			dbo.WhsClientPickPackParamsByWhs
		WHERE
			WPP_OH_Client = WD_OH_Client
			AND WPP_WW_Warehouse = @Warehouse
			AND (WPP_WSH_SalesChannel IS NULL OR WPP_WSH_SalesChannel = WD_WSH_SalesChannel)
		ORDER BY
			CASE WHEN WPP_WSH_SalesChannel IS NULL THEN 1 ELSE 0 END ASC
	) as PickPackParams
	OUTER APPLY
	(
		SELECT WPP_AllowPickDockDoorLocationOverride AS ClientAllowsDDLOverride
		FROM
			dbo.WhsClientPickPackParamsByWhs
		WHERE
			WPP_OH_Client = WD_OH_Client
			AND WPP_WW_Warehouse = @Warehouse
			AND WPP_WSH_SalesChannel IS NULL
	) as DockDoorOverride
	OUTER APPLY
	(
		SELECT TOP 1
			PutawayLocation.WLV_LocationString AS LocationString,
			PutawayLocation.WLV_LocationString_UserFriendly AS LocationString_UserFriendly,
			PutawayLocation.WLV_LocationClass AS LocationClass
		FROM
			dbo.WhsDocketLine OrderLine
			JOIN dbo.WhsPickLine on WZ_WE_TransactionLine = OrderLine.WE_PK
			JOIN dbo.WhsDocketLine InventoryLine on InventoryLine.WE_PK = WZ_WE_InventoryLine
			JOIN dbo.WhsLocationView TransferFromLocation on InventoryLine.WE_WL_TransferFrom = TransferFromLocation.WLV_PK
			JOIN dbo.WhsLocationView PutawayLocation on InventoryLine.WE_WL = PutawayLocation.WLV_PK
		WHERE
			WZ_WE_OriginalPickedInventoryLine IS NOT NULL
			AND InventoryLine.WE_DocketLineType = '{DocketType.Codes.Transfer}'
			AND InventoryLine.WE_StockOnHand > 0
			AND InventoryLine.WE_DocketLineStatus = '{DocketLineStatus.Codes.Finalised}'
			AND TransferFromLocation.WLV_LocationClass = '{LocationClasses.Codes.PST}'
			AND OrderLine.WE_WD = KJ_ParentID
	) AS AssignedPutawayLocationFromPackingStation
WHERE
	WD_WW_Whs = @Warehouse AND
	{BuildPackagePKClause()}
	KPH_PackageID = @PackageID AND
	WP_PickStatus != '{PickStatus.Codes.Finalised}' AND
	(KP_ClosedTimeUtc IS NULL OR KP_IsClosed = 0) AND
	KP_PK NOT IN
	(
		SELECT
			KI_KP_Package
		FROM
			dbo.WhsPickLine
			JOIN dbo.PkgPackageItemDivot ON KI_ParentID = WZ_PK
		WHERE
			WZ_PickedDateTime IS NULL
			AND WZ_WE_OriginalPickedInventoryLine IS NULL
			AND KP_PK = KI_KP_Package
	)

ORDER BY
	WD_ExternalReference,
	WD_DocketID");

			string BuildPackagePKClause()
				=> packagePK.HasValue
					? Invariant($"{PkgPackageSchema.Constants.PK} = @PackagePK AND") // SQL Statement
					: "";
		}

		string BuildConsolidationHUQuery() => Invariant($@"
SELECT 
	KP_PK,
	WPK_PackageID AS KPH_PackageID,
	KP_Weight,
	KP_TareWeight,
	KP_WeightUQ,
	KP_Length,
	KP_Width,
	KP_Height,
	KP_DimensionUQ,
	CAST(CASE WHEN CBACount = 1 AND HasNullCBA = 0 THEN 1 ELSE 0 END AS BIT) AS IsUsingCarrierLabelIntegration
FROM 
	WhsPackageLocationView
	JOIN PkgPackage HandlingUnit on WPK_KP_Package = KP_PK
	CROSS APPLY
	(
		SELECT
			COUNT (DISTINCT E2_OA_Address) as CBACount,
			MAX (CASE WHEN E2_PK IS NULL THEN 1 ELSE 0 END) as HasNullCBA
		FROM
			dbo.PkgPackage InnerPackage
			JOIN dbo.PkgPackageJob ON InnerPackage.KP_KJ_ParentPackageJob = KJ_PK
			JOIN dbo.WhsDocket WhsOrder ON KJ_ParentID = WD_PK
			LEFT JOIN dbo.JobDocAddress ON E2_ParentID = WhsOrder.WD_PK AND E2_AddressType = '{DocAddressTypes.Codes.CarrierBookingAgent}' AND E2_AddressSequence = 0
		WHERE
			KP_KP_TopHandlingUnitPackage = HandlingUnit.KP_PK
	) AS InnerPackageCBAs
WHERE
	WPK_WW_WHS = @Warehouse AND
	WPK_PackageID = @PackageID AND
	WPK_IsHandlingUnit = 1
	AND (KP_IsClosed = 1 OR KP_ClosedTimeUtc IS NOT NULL)");
	}

	#endregion
}
