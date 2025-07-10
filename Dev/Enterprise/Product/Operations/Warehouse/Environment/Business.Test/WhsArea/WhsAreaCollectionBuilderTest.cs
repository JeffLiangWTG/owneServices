using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.CodeLists;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsAreaCollectionBuilderTest : TestCaseWithFactory
	{
		#region TestGetPickingAreas

		public void TestGetPickingAreas()
		{
			var whs = Factory.New<WhsWarehouse>();
			var bothArea = Helper.CreateArea(whs, "A1", AreaTypes.Codes.FreeStore, true, true);
			var putawayArea = Helper.CreateArea(whs, "A2", AreaTypes.Codes.FreeStore, false, true);
			var pickingArea = Helper.CreateArea(whs, "A3", AreaTypes.Codes.FreeStore, true, false);
			var builder = new WhsAreaCollectionBuilder();
			var pickingAreas = builder.GetPickingAreas(Factory, whs);
			AssertCollectionContains(bothArea, pickingAreas);
			AssertCollectionNotContains(putawayArea, pickingAreas);
			AssertCollectionContains(pickingArea, pickingAreas);
		}

		#endregion

		#region TestGetPutawayAreas

		public void TestGetPutawayAreas()
		{
			var whs = Factory.New<WhsWarehouse>();
			var bothArea = Helper.CreateArea(whs, "A1", AreaTypes.Codes.FreeStore, true, true);
			var putawayArea = Helper.CreateArea(whs, "A2", AreaTypes.Codes.FreeStore, false, true);
			var pickingArea = Helper.CreateArea(whs, "A3", AreaTypes.Codes.FreeStore, true, false);
			var builder = new WhsAreaCollectionBuilder();
			var putawayAreas = builder.GetPutawayAreas(Factory, whs);
			AssertCollectionContains(bothArea, putawayAreas);
			AssertCollectionContains(putawayArea, putawayAreas);
			AssertCollectionNotContains(pickingArea, putawayAreas);
		}

		#endregion

		#region Implmentation

		WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}
