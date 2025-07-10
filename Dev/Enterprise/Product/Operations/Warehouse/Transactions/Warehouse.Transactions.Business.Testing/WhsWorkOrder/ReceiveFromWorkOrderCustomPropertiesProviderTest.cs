using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ReceiveFromWorkOrderCustomPropertiesProviderTest : TestCaseWithFactory
	{
		public void TestGetCustomPropertiesForAssembledReceive()
		{
			var container = ReceiveFromWorkOrderCustomPropertiesProvider.GetCustomPropertiesForAssembledReceive();
			AssertType<CustomPropertyContainer<WhsReceiveLine>>(container);

			var property = container.CustomProperties.Single();
			AssertEquals("IsLeftoverComponent", property.Identifier);
			AssertEquals("Is Leftover Component", property.Info.GetCaption());
		}

		public void TestGetCustomPropertiesForAssembledReceive_Function()
		{
			var container = ReceiveFromWorkOrderCustomPropertiesProvider.GetCustomPropertiesForAssembledReceive();
			var property = container.CustomProperties.Single();
			var receiveLine = Factory.New<WhsReceiveLine>();
			AssertEquals(ZBool.True, property.GetValue(receiveLine));

			var link = Factory.New<WhsBOMInventoryPivot>();
			link.WIP_WE_InventoryLine = receiveLine.PK;
			AssertEquals(ZBool.False, property.GetValue(receiveLine));
		}
	}
}
