using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class OrderModuleToModuleSenderTest : TestCaseWithFactory
	{
		public void TestCreateNewEntityFromParent()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.Buyer.OH_IsWarehouseClient = true;
			var orderModuleToModuleSender = new OrderModuleToModuleSender();
			var result1 = orderModuleToModuleSender.CreateNewEntityFromParent(order);
			AssertEquals(UniversalResult.HadErrors, result1.ResultType);
			AssertEquals("Failed to create Warehouse Receive:\r\nNo Warehouse is specified.", result1.ErrorMessage);
			AssertNull(result1.FindJobIfExists());

			order.WarehouseDocAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			var result2 = orderModuleToModuleSender.CreateNewEntityFromParent(order);
			AssertEquals(UniversalResult.HadErrors, result2.ResultType);
			AssertStartsWith("Receive Creation failed, no warehouse with this address", "Failed to create Warehouse Receive:\r\n", result2.ErrorMessage);
			AssertNull(result2.FindJobIfExists());

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");
			warehouse[WhsWarehouseSchema.WW_OA_WarehouseAddress] = order.WarehouseAddress.PK;
			Factory.Save();

			var result3 = orderModuleToModuleSender.CreateNewEntityFromParent(order);
			AssertEquals(UniversalResult.Internal, result3.ResultType);
			AssertEquals("", result3.ErrorMessage);
			AssertNotNull(result3.FindJobIfExists());
		}
	}
}
