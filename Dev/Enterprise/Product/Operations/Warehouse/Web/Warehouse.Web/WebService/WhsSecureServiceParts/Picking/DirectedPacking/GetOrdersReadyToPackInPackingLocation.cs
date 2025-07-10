using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Get orders that are ready to pack in packing location.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsOrdersWebServiceResponse GetOrdersReadyToPackInPackingLocation(string packingStationLocation)
		{
			return HandleWebServiceRequest<WhsOrdersWebServiceResponse>(r => GetOrdersReadyToPackInPackingLocationCore(r, packingStationLocation));
		}

		void GetOrdersReadyToPackInPackingLocationCore(WhsOrdersWebServiceResponse response, string packingStationLocation)
		{
			var packingStation = WebServiceHelper.GetLocationByLocationString(Factory, SecurityHeader.WarehouseCode, response, packingStationLocation);
			if (response.NoError())
			{
				if (packingStation == null)
				{
					response.LogBusinessValidationError(Res.GetString("e9a81242-2fde-411c-b021-732c5173520c", "Packing Station '{0}' does not exist in warehouse {1}.", packingStationLocation, WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode).WW_WarehouseNameMultilingual));
				}
				else if (!packingStation.IsPackingStationLocation)
				{
					response.LogBusinessValidationError(Res.GetString("c932a063-aaa2-41c5-9c6e-1b2889f4b092", "Location '{0}' is not a Packing Station.", packingStationLocation));
				}
				else
				{
					GetReadyToPackOrdersInPackingStation(response, packingStation);
				}
			}
		}

		void GetReadyToPackOrdersInPackingStation(WhsOrdersWebServiceResponse response, WhsLocation packingStation)
		{
			var orderPks = GetOrderPksInPackingStation(packingStation);
			var orderInfos = new List<WhsDocketInfo>();
			if (orderPks.Length > 0)
			{
				var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, orderPks));
				GetReadyToPackOrders(orderInfos, orders);
			}

			response.Orders = orderInfos.ToArray();
		}

		ZGuid[] GetOrderPksInPackingStation(WhsLocation location)
		{
			var getOrderPksQuery = @$"
SELECT
	DISTINCT WD_PK
FROM
	dbo.WhsDocket
	JOIN dbo.WhsOrderStatusView ON WOS_PK = WD_PK
	JOIN dbo.WhsDocketLine OrderLine ON OrderLine.WE_WD = WD_PK
	JOIN dbo.WhsPickLine ON OrderLine.WE_PK = WZ_WE_TransactionLine
	JOIN dbo.WhsDocketLine InventoryLine ON InventoryLine.WE_PK = WZ_WE_InventoryLine
WHERE
	WD_DocketType = '{DocketType.Codes.Order}'
	AND WOS_OrderStatus = '{WhsOrderStatus.Codes.ReadyToPack}'
	AND WZ_WE_OriginalPickedInventoryLine IS NOT NULL
	AND InventoryLine.WE_CurrentInventoryStatus = '{InventoryStatus.Codes.ReadyToPack}'
	AND InventoryLine.WE_WL = @PackingStationPK
	AND (WD_GS_NKAssignedPacker = '' OR WD_GS_NKAssignedPacker = @AssignedUser)
";

			var user = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@PackingStationPK", location.PK, WhsDocketLineSchema.WE_WL),
				ZSqlParameter.New("@AssignedUser", user.GS_Code, WhsDocketSchema.WD_GS_NKAssignedPacker)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(getOrderPksQuery, sqlParams);

			return collection.Select(dynamicObject => (ZGuid)dynamicObject[WhsDocketSchema.Constants.PK]).ToArray();
		}

		void GetReadyToPackOrders(List<WhsDocketInfo> orderInfos, WhsOrder[] orders)
		{
			AddOrderAndPackageJobLoadingFetchHints(orders, Factory);
			foreach (var order in orders)
			{
				var packageJob = order.PackageJob;
				if (packageJob != null)
				{
					var pickLines = order.Lines
						.Where(line => line.WE_WE_ParentDocketLine.IsEmpty)
						.SelectMany(line => line.PickLines);
					if (pickLines.Any(pickLine => !packageJob.IsPacked(pickLine)))
					{
						orderInfos.Add(new WhsDocketInfo(order, shouldCreateDocketLines: false));
					}
				}
				else
				{
					orderInfos.Add(new WhsDocketInfo(order, shouldCreateDocketLines: false));
				}
			}
		}

		static void AddOrderAndPackageJobLoadingFetchHints(WhsOrder[] orders, BusinessObjectFactory factory)
		{
			var orderPKs = orders.Select(o => o.PK).ToArray();
			JobDocAddressFetchHintsHelper.AddFetchHints(orderPKs, factory);
			factory.AddFetchHint(PkgPackageJobSchema.Instance, new ZQuery(PkgPackageJobSchema.KJ_ParentID, orderPKs));
			factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WD, orderPKs));

			factory.AddFetchHint(WhsPickSchema.Instance, new ZQuery(WhsPickSchema.PK, orders.Select(o => o.WD_WP)));

			var lines = orders.SelectMany(o => o.Lines).ToArray();
			factory.AddFetchHint(OrgSupplierPartSchema.Instance, new ZQuery(OrgSupplierPartSchema.PK, lines.Select(line => line.WE_OP).ToArray()));
			factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, lines.Select(line => line.PK).ToArray()));

			var packageJobs = orders.Select(o => o.PackageJob).ToArray();
			factory.AddFetchHint(PkgPackageSchema.Instance, new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJobs.Select(job => job.PK).ToArray()));

			var packagePKs = packageJobs.SelectMany(o => o.Packages).Select(pkg => pkg.PK).ToArray();
			factory.AddFetchHint(PkgPackageItemDivotSchema.Instance, new ZQuery(PkgPackageItemDivotSchema.KI_KP_Package, packagePKs));
		}
	}
}
