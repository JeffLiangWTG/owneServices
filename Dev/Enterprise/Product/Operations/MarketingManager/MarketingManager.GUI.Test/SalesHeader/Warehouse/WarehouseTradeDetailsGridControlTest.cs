using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class WarehouseTradeDetailsGridControlTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNewType(OrgSalesWarehouseServiceTypesList.Codes.Orders, typeof(WarehouseOrdersTradeDetailsGridControl));
			AssertNewType(OrgSalesWarehouseServiceTypesList.Codes.Receipts, typeof(WarehouseReceiptsTradeDetailsGridControl));
			AssertNewType(OrgSalesWarehouseServiceTypesList.Codes.Storage, typeof(WarehouseStorageTradeDetailsGridControl));
		}

		void AssertNewType(string service, Type expectedType)
		{
			using (var control = WarehouseTradeDetailsGridControl.New(WarehouseProduct, service))
			{
				AssertType(expectedType, control);
			}
		}

		OrgSalesProduct WarehouseProduct
		{
			get
			{
				if (warehouseProduct == null)
				{
					warehouseProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
				}

				return warehouseProduct;
			}
		}
		OrgSalesProduct warehouseProduct;
	}
}
