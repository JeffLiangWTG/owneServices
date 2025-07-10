using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Loads the order's unpacked items for Directed Packing.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PackageAndProductInfosWebServiceResponse LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(Guid orderPK)
		{
			return HandleWebServiceRequest<PackageAndProductInfosWebServiceResponse>(r => LoadOrderUnpackedProductInfosForDirectedPackingCore(r, orderPK));
		}

		void LoadOrderUnpackedProductInfosForDirectedPackingCore(PackageAndProductInfosWebServiceResponse response, Guid orderPK)
		{
			var docket = Factory.Load<WhsDocket>(orderPK);
			if (docket != null && docket is WhsOrder order)
			{
				LoadOrderUnpackedProductInfos(response, order);
				GetOrderPackingInfo(response, order);
			}
			else
			{
				response.LogBusinessValidationError(OrderDoesNotExistError);
			}
		}

		void GetOrderPackingInfo(PackageAndProductInfosWebServiceResponse response, WhsOrder order)
		{
			if (response.NoError())
			{
				var sqlParams = new ZSqlParameterCollection
				{
					{ "@Warehouse", order.WD_WW_Whs, WhsDocketSchema.WD_WW_Whs },
					{ "@OrderPK", order.PK, WhsDocketSchema.PK }
				};

				var packingData = new DynamicBusinessObjectCollection(Factory);
				packingData.Load(GetPackingInfoSqlForDirectedPacking(includeToteInfo: false), sqlParams);

				var packingDataInfo = packingData.Single();
				var packageInfo = new PackageForPackingInfo
				{
					IsDirectedPacking = true,
					ClientEnforceProductScan = true,
					OrderReference = order.WD_ExternalReference,
					DocketID = order.WD_DocketID,
					PackType = Core.Constants.PkgUnit.Carton,
					IsTote = false,
					DocketStatus = order.WD_DocketStatus,
					RequiredDate = order.WD_RequiredDate.ToDateTime(),
					ClientCode = (ZString)packingDataInfo[OrgHeaderSchema.Constants.OH_Code],
					IsUsingCarrierLabelIntegration = (ZBool)packingDataInfo[nameof(PackageForPackingInfo.IsUsingCarrierLabelIntegration)],
					PackageWeightTolerance = (ZDecimal)packingDataInfo[OrgMiscServSchema.Constants.OM_WhsPackageWeightTolerancePercent],
					PackageWeightToleranceEnabled = (ZBool)packingDataInfo[OrgMiscServSchema.Constants.OM_WhsPackageToleranceEnabled],
					IsUsingCartonSizes = (ZBool)packingDataInfo[nameof(PackageForPackingInfo.IsUsingCartonSizes)],
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
				packageInfo.DimensionUQ = defaultUnits.DefaultDimensionUnit;
				packageInfo.WeightUQ = defaultUnits.DefaultWeightUnit;

				response.Package = packageInfo;
			}
		}
	}
}
