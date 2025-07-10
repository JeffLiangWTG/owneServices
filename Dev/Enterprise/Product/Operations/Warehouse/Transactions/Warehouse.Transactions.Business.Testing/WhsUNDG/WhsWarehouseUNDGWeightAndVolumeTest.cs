namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWarehouseUNDGWeightAndVolumeTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			var totalWeight = 10m;
			var totalWeightUQ = "KG";
			var totalVolume = 20m;
			var totalVolumeUQ = "M3";

			var whsWarehouseUNDGWeightAndVolume = new WhsWarehouseUNDGWeightAndVolume(totalWeight, totalWeightUQ, totalVolume, totalVolumeUQ);
			CombineAssertions(() =>
			{
				AssertEquals(totalWeight, whsWarehouseUNDGWeightAndVolume.TotalWeight);
				AssertEquals(totalWeightUQ, whsWarehouseUNDGWeightAndVolume.TotalWeightUQ);
				AssertEquals(totalVolume, whsWarehouseUNDGWeightAndVolume.TotalVolume);
				AssertEquals(totalVolumeUQ, whsWarehouseUNDGWeightAndVolume.TotalVolumeUQ);
			});
		}
	}
}
