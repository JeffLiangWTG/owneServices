using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsWarehouseUNDGTotalsInfoTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			var undgSubstance = new ZGuid();
			var undgCountryReference = new ZGuid();
			var undgClass = "1";
			var totalWeight = 10m;
			var totalWeightLimit = 20m;
			var totalWeightLimitUQ = "KG";
			var totalVolume = 11m;
			var totalVolumeLimit = 21m;
			var totalVolumeLimitUQ = "M3";

			var warehouseUNDGTotalsInfo = new WhsWarehouseUNDGTotalsInfo(
				undgSubstance,
				undgCountryReference,
				undgClass,
				totalWeight,
				totalWeightLimit,
				totalWeightLimitUQ,
				totalVolume,
				totalVolumeLimit,
				totalVolumeLimitUQ);

			CombineAssertions(() =>
			{
				AssertEquals(undgSubstance, warehouseUNDGTotalsInfo.UNDGSubStance);
				AssertEquals(undgCountryReference, warehouseUNDGTotalsInfo.UNDGCountryReference);
				AssertEquals(undgClass, warehouseUNDGTotalsInfo.UNDGClass);
				AssertEquals(totalWeight, warehouseUNDGTotalsInfo.TotalWeight);
				AssertEquals(totalWeightLimit, warehouseUNDGTotalsInfo.TotalWeightLimit);
				AssertEquals(totalWeightLimitUQ, warehouseUNDGTotalsInfo.TotalWeightLimitUQ);
				AssertEquals(totalVolume, warehouseUNDGTotalsInfo.TotalVolume);
				AssertEquals(totalVolumeLimit, warehouseUNDGTotalsInfo.TotalVolumeLimit);
				AssertEquals(totalVolumeLimitUQ, warehouseUNDGTotalsInfo.TotalVolumeLimitUQ);
			});
		}
	}
}
