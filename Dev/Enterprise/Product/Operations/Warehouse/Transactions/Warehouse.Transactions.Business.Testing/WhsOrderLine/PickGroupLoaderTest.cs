using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PickGroupLoaderTest : WhsTestCaseWithFactory
	{
		#region TestGetDefaultPickGroup

		public void TestGetDefaultPickGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var productParams = new WhsProductParamsByWhsAndClientCollection(data.Part1, Factory).AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_PickGroup = 13;

			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			AssertEquals("Pick group should be defaulted from product params.", (ZShort)13, PickGroupLoader.GetDefaultPickGroup(rowFactory, data.Part1.PK, order.WD_WW_Whs, order.WD_OH_Client));

			AssertEquals("Pick group should return 0 when no product pk provided.", (ZShort)0, PickGroupLoader.GetDefaultPickGroup(rowFactory, ZGuid.Empty, order.WD_WW_Whs, order.WD_OH_Client));

			AssertEquals("Pick group should return 0 when no warehouse set on order.", (ZShort)0, PickGroupLoader.GetDefaultPickGroup(rowFactory, data.Part1.PK, ZGuid.Empty, order.WD_OH_Client));

			AssertEquals("Pick group should return 0 when no client set on order.", (ZShort)0, PickGroupLoader.GetDefaultPickGroup(rowFactory, data.Part1.PK, order.WD_WW_Whs, ZGuid.Empty));

			productParams.W3_OP = data.Part2.PK;
			AssertEquals("Pick group should return 0 when there are no matching product params.", (ZShort)0, PickGroupLoader.GetDefaultPickGroup(rowFactory, data.Part1.PK, order.WD_WW_Whs, order.WD_OH_Client));
		}

		#endregion
	}
}
