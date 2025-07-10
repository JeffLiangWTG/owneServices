using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderLineShortfallQuantityHelper
	{
		#region Constructor

		public WhsOrderLineShortfallQuantityHelper(BusinessObjectFactory factory)
		{
			Factory = Argument.NotNull(factory, "Factory");
		}

		#endregion

		readonly BusinessObjectFactory Factory;

		#region ReCalculateShortFallConsideringBOMComponents

		public ZDecimal ReCalculateShortFallConsideringBOMComponents(ZDecimal shortFall, OrgSupplierPart kitProduct, ZGuid clientPK, ZGuid warehousePK)
		{
			var result = shortFall;
			var minimumPossibleProduct = PossibleProductQtyCanBeAssembledFromItsComponents(kitProduct, clientPK, warehousePK);

			if (minimumPossibleProduct > 0)
			{
				result = shortFall >= minimumPossibleProduct ? shortFall - minimumPossibleProduct : 0;
			}

			return result;
		}

		#endregion

		#region PossibleProductCanBeAssembledFromItsComponent

		// #warning Needs to be addressed
		/// <summary>
		/// This code will not work when Duplicate Component Products are specified.
		/// This needs to be fixed later.
		/// </summary>
		public ZDecimal PossibleProductQtyCanBeAssembledFromItsComponents(OrgSupplierPart kitProduct, ZGuid clientPK, ZGuid warehousePK)
		{
			int minimumPossibleProduct = 0;

			if (WhsProduct.GetWhsProduct(kitProduct).IsBOMProductPickedOnSalesOrder)
			{
				minimumPossibleProduct = kitProduct.BillOfMaterials.Count > 0 ? kitProduct.BillOfMaterials.Min(partBom => GetNumberOfPossibleKitsFromComponent(partBom, clientPK, warehousePK)) : 0;
			}

			return minimumPossibleProduct;
		}

		int GetNumberOfPossibleKitsFromComponent(OrgPartBOM partBOM, ZGuid clientPK, ZGuid warehousePK)
		{
			int result = 0;

			if (partBOM != null)
			{
				var availablePickQuantity = GetAvailableComponentsToPickQuantity(partBOM.OE_OP_Component, clientPK, warehousePK);
				result = BOMComponentQuantityHelper.GetNumberOfPossibleKitsFromComponent(partBOM, availablePickQuantity);
			}
			return result;
		}

		#endregion

		#region GetAvailableComponentsToPickQuantity

#if DEBUG
		public
#endif
		ZDecimal GetAvailableComponentsToPickQuantity(ZGuid partPK, ZGuid clientPK, ZGuid warehousePK)
		{
			var query = @"
WI_PK in (
select
	WI_PK
from
	dbo.WhsInventoryView
	join dbo.WhsLocation on WL_PK = WI_WL and WL_LocationStatus = @LocationStatus
	join dbo.WhsRow on WR_PK = WL_WR
where
	WI_OH_Client = @ClientPK
	and WR_WW_Whs = @WarehousePK
	and WI_OP = @PartPK
	and WI_TotalUnits > 0
	and WI_InventoryStatus = @InventoryStatus)
";
			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@ClientPK", clientPK, WhsDocketSchema.WD_OH_Client);
			queryParams.Add("@WarehousePK", warehousePK, WhsDocketSchema.WD_WW_Whs);
			queryParams.Add("@PartPK", partPK, WhsDocketLineSchema.WE_OP);
			queryParams.Add("@InventoryStatus", InventoryStatus.Codes.Available, WhsInventoryViewSchema.WI_InventoryStatus);
			queryParams.Add("@LocationStatus", LocationStatus.Codes.Normal, WhsLocationViewSchema.WLV_LocationStatus);

			var filter = new ZDBOnlyQuery(typeof(WhsInventoryView));
			filter.AddFilterAndZSQLParameterCollection(query, queryParams);

			return Factory.Load<WhsInventoryView>(filter).Sum(inv => inv.WI_AvailableToPickQuantity);
		}

		#endregion
	}
}
