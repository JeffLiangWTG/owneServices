using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Web.Testing
{
	public abstract class AssignDefaultWarehouseTest : TestCaseWithFactory
	{
		public abstract WhsDocket GetDocketWithAssignedDefaultWarehouse();

		public void TestDefaultWarehouseExists()
		{
			ZGuid warehousePK = CreateWarehouse();
			CreateOrder(warehousePK);

			Factory.Save();

			WhsDocket docket = GetDocketWithAssignedDefaultWarehouse();
			AssertEquals("Warehouse should be set if only one warehouse can be selected", warehousePK, docket.WD_WW_Whs);
		}

		public void TestDefaultWarehouseDoesNotExist()
		{
			ZGuid warehouse1PK = CreateWarehouse();
			CreateOrder(warehouse1PK);

			ZGuid warehouse2PK = CreateWarehouse();
			CreateOrder(warehouse2PK);

			Factory.Save();

			WhsDocket docket = GetDocketWithAssignedDefaultWarehouse();
			AssertEquals("Warehouse should be empty if more then one warehouse can be selected", ZGuid.Empty, docket.WD_WW_Whs);
		}

		#region Implementation

		ZGuid CreateWarehouse()
		{
			WhsWarehouse warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_IsActive = true;

			return warehouse.PK;
		}

		void CreateOrder(ZGuid warehousePK)
		{
			WhsOrder order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehousePK;
			order.WD_OH_Client = GlbCompany.CurrentCompany.OrgProxy.PK;
		}

		#endregion
	}
}
