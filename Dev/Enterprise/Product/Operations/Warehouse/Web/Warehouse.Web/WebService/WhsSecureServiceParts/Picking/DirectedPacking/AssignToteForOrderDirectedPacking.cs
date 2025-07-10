using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Assigns tote for Order and loads the order's unpacked items for Directed Packing.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PackageAndProductInfosWebServiceResponse AssignToteForOrderDirectedPacking(string toteID, Guid orderPK)
		{
			return HandleWebServiceRequest<PackageAndProductInfosWebServiceResponse>(r => AssignToteForOrderDirectedPackingCore(r, toteID, orderPK));
		}

		void AssignToteForOrderDirectedPackingCore(PackageAndProductInfosWebServiceResponse response, string toteID, Guid orderPK)
		{
			if (toteID.IsNullOrEmpty())
			{
				response.LogBusinessValidationError(Res.GetString("dc17ecc3-7a07-41f1-b32a-105a5b7156f4", "Empty Tote ID."));
				response.IsInvalidToteId = true;
			}
			else
			{
				var docket = Factory.Load<WhsDocket>(orderPK);
				if (docket != null && docket is WhsOrder order)
				{
					var packageJob = Factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, order.PK));
					ValidateToteIdForDirectedPacking(response, toteID, packageJob);
					LoadOrderUnpackedProductInfos(response, order);
					GetOrderTotePackingInfo(response, toteID, order);
				}
				else
				{
					response.LogBusinessValidationError(OrderDoesNotExistError);
				}
			}
		}

		void ValidateToteIdForDirectedPacking(PackageAndProductInfosWebServiceResponse response, string toteID, PkgPackageJob packageJob)
		{
			if (!ValidateToteForOrderDirectedPacking(response, toteID, packageJob))
			{
				response.IsInvalidToteId = true;
			}
		}

		bool ValidateToteForOrderDirectedPacking(WebServiceResponse response, string toteID, PkgPackageJob packageJob)
		{
			var package = packageJob.GetAllPackagesOnJob().FirstOrDefault(p => p.KP_PackageID == toteID);
			if (package != null)
			{
				if (package.GetIsTote())
				{
					response.LogBusinessValidationError(Res.GetString("cf1dffb4-6cfe-4dac-8577-26c0946f5427", "Tote '{0}' is already used on this order, select another Tote.", toteID));
				}
				else
				{
					response.LogBusinessValidationError(Res.GetString("0ca36f84-ae80-4f9d-adfb-f4a79a84fd69", "A non Tote Package on the order was found using Package ID '{0}' already.", toteID));
				}
			}

			return response.NoError();
		}

		void LoadOrderUnpackedProductInfos(PackageAndProductInfosWebServiceResponse response, WhsOrder order)
		{
			if (response.NoError())
			{
				if (order.WarehouseOrderStatus.Equals(WhsOrderStatus.Codes.ReadyToPack))
				{
					var productInfos = PackageHelper.GetOrderUnpackedProductInfos(Factory, order.PK);
					if (productInfos.Any())
					{
						response.ProductInfos = productInfos;
					}
					else
					{
						response.LogBusinessValidationError(GetOrderNothingToPackError(order.WD_ExternalReference));
					}
				}
				else
				{
					response.LogBusinessValidationError(GetOrderNotReadyToPackError(order.WD_ExternalReference));
				}
			}
		}

		void GetOrderTotePackingInfo(PackageAndProductInfosWebServiceResponse response, string toteID, WhsOrder order)
		{
			if (response.NoError())
			{
				var sqlParams = new ZSqlParameterCollection
				{
					{ "@Warehouse", order.WD_WW_Whs, WhsDocketSchema.WD_WW_Whs },
					{ "@OrderPK", order.PK, WhsDocketSchema.PK }
				};

				var packingData = new DynamicBusinessObjectCollection(Factory);
				packingData.Load(GetPackingInfoSqlForDirectedPacking(includeToteInfo: true), sqlParams);

				var packingDataInfo = packingData.Single();
				var packageInfo = new PackageForPackingInfo
				{
					IsDirectedPacking = true,
					OrderReference = order.WD_ExternalReference,
					DocketID = order.WD_DocketID,
					PackageID = toteID,
					ToteID = toteID,
					PackType = Core.Constants.PkgUnit.Tote,
					IsTote = true,
					DocketStatus = order.WD_DocketStatus,
					RequiredDate = order.WD_RequiredDate.ToDateTime(),
					ClientCode = (ZString)packingDataInfo[OrgHeaderSchema.Constants.OH_Code],
					IsUsingCarrierLabelIntegration = (ZBool)packingDataInfo[nameof(PackageForPackingInfo.IsUsingCarrierLabelIntegration)],
					PackageWeightTolerance = (ZDecimal)packingDataInfo[OrgMiscServSchema.Constants.OM_WhsPackageWeightTolerancePercent],
					PackageWeightToleranceEnabled = (ZBool)packingDataInfo[OrgMiscServSchema.Constants.OM_WhsPackageToleranceEnabled],
					IsUsingCartonSizes = (ZBool)packingDataInfo[nameof(PackageForPackingInfo.IsUsingCartonSizes)],
					EmptyWeight = (ZDecimal)packingDataInfo[nameof(PackageForPackingInfo.Weight)],
					WeightUQ = (ZString)packingDataInfo[nameof(PackageForPackingInfo.WeightUQ)],
					Length = (ZDecimal)packingDataInfo[nameof(PackageForPackingInfo.Length)],
					Width = (ZDecimal)packingDataInfo[nameof(PackageForPackingInfo.Width)],
					Height = (ZDecimal)packingDataInfo[nameof(PackageForPackingInfo.Height)],
					DimensionUQ = (ZString)packingDataInfo[nameof(PackageForPackingInfo.DimensionUQ)],
					OrderIsUsingDirectedPackingConsolidation = (ZBool)packingDataInfo[WhsDocketSchema.Constants.WD_UseDirectedPackingConsolidation],
					PackageRequiresPutaway = true,
					AssignedDockDoorLocationString = (ZString)packingDataInfo[WhsLocationViewSchema.Constants.WLV_LocationString],
					AssignedDockDoorLocationStringUserFriendly = (ZString)packingDataInfo[WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly],
					AssignedPutawayLocationString = (ZString)packingDataInfo[nameof(WhsLocationInfo.LocationString)],
					AssignedPutawayLocationStringUserFriendly = (ZString)packingDataInfo[nameof(WhsLocationInfo.LocationString_UserFriendly)],
					AssignedPutawayLocationClass = (ZString)packingDataInfo["LocationClass"],
					AllowedToOverrideDockDoorLocation = (ZBool)packingDataInfo["AllowDDLOverride"]
				};

				var defaultUnits = ObjectFactory.Get<IPackageDefaultUQs>();
				if (packageInfo.DimensionUQ.IsNullOrEmpty())
				{
					packageInfo.DimensionUQ = defaultUnits.DefaultDimensionUnit;
				}

				if (packageInfo.WeightUQ.IsNullOrEmpty())
				{
					packageInfo.WeightUQ = defaultUnits.DefaultWeightUnit;
				}

				response.Package = packageInfo;
			}
		}

		string GetPackingInfoSqlForDirectedPacking(bool includeToteInfo)
		{
			var toteInfoSelectQueryString = includeToteInfo
				? @"
	ISNULL(F3_UnitOfDimension, '') AS DimensionUQ,
	ISNULL(F3_Height, 0) AS Height,
	ISNULL(F3_Length, 0) AS Length,
	ISNULL(F3_Width, 0) AS Width,
	ISNULL(F3_Weight, 0) AS Weight,
	ISNULL(F3_UnitOfWeight, '') AS WeightUQ,
"
				: string.Empty;

			var toteInfoOuterApplyQueryString = includeToteInfo
				? $@"
	OUTER APPLY
	(
		SELECT
			F3_UnitOfDimension,
			F3_Height,
			F3_Length,
			F3_Width,
			F3_Weight,
			F3_UnitOfWeight
		FROM
			dbo.RefPackType
		WHERE
			F3_Code = '{Core.Constants.PkgUnit.Tote}'
	) ToteDimensions
"
				: string.Empty;

			return $@"
SELECT
	OH_Code,
	OM_WhsPackageWeightTolerancePercent,
	OM_WhsPackageToleranceEnabled,
	OM_WhsEnforceScanOfProductsWhenPackingTote,
	CASE WHEN WPP_IsUsingCartonSizes is null THEN CAST(1 AS BIT) ELSE WPP_IsUsingCartonSizes END AS IsUsingCartonSizes,
	CAST(CASE WHEN E2_OA_Address IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsUsingCarrierLabelIntegration,
	{toteInfoSelectQueryString}
	WD_UseDirectedPackingConsolidation,
	WLV_LocationString,
	WLV_LocationString_UserFriendly,
	AssignedPutawayLocationFromPackingStation.LocationString,
	AssignedPutawayLocationFromPackingStation.LocationString_UserFriendly,
	AssignedPutawayLocationFromPackingStation.LocationClass,
	CASE
		WHEN WDA_FirstPutawayToDockDoorUtc IS NULL AND ClientAllowsDDLOverride = 1 THEN CAST(1 AS BIT)
		ELSE CAST(0 AS BIT)
	END AS AllowDDLOverride
FROM
	dbo.WhsDocket
	LEFT JOIN dbo.JobDocAddress ON E2_ParentID = WD_PK AND E2_AddressType = '{DocAddressTypes.Codes.CarrierBookingAgent}' AND E2_AddressSequence = 0
	JOIN dbo.WhsPick ON WD_WP = WP_PK
	LEFT JOIN dbo.WhsDockDoorAssignment ON WDA_PK = WP_WDA_DockDoorAssignment
	JOIN dbo.WhsLocationView ON WLV_PK = ISNULL(WDA_WL_AssignedDockDoor, WP_WL_DockDoor)
	JOIN dbo.OrgHeader ON OH_PK = WD_OH_Client
	JOIN dbo.OrgMiscServ ON OM_OH = OH_PK
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
	{toteInfoOuterApplyQueryString}
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
			AND OrderLine.WE_WD = @OrderPK
	) AssignedPutawayLocationFromPackingStation
WHERE
	WD_WW_Whs = @Warehouse
	AND WD_PK = @OrderPK
	AND	WP_PickStatus <> '{PickStatus.Codes.Finalised}'
";
		}

		public static string GetOrderNotReadyToPackError(string orderReference) => Res.GetString("acc83933-ac86-4d8b-acb0-1c380f7c6afe", "Order '{0}' is not ready to pack.", orderReference);
		public static string GetOrderNothingToPackError(string orderReference) => Res.GetString("85f09053-a473-421f-96eb-a1b2ee0a5c6c", "Order '{0}' has nothing to pack.", orderReference);
		public static string OrderDoesNotExistError => Res.GetString("c736b958-438d-4aee-a973-875128b8b680", "Order does not exist.");
	}
}
