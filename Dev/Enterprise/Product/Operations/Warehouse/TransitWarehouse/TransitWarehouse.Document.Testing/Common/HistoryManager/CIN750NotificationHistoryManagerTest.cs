using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public abstract class CIN750NotificationHistoryManagerTest<TBusinessObject> : TestCaseWithFactory where TBusinessObject : EnterpriseBusinessObject
	{
		#region Test Weight Property

		public void Test_HistoryPackageGroup_WeightIsRounded()
		{
			var historyPackageGroup = new HistoryPackageGroup() { Quantity = 1, Weight = 1.234567m };
			AssertEquals(1.235m, historyPackageGroup.Weight);
		}

		public void Test_OutNotificationAdditionalData_WeightIsRounded()
		{
			var outNotificationAdditionalData = new OutNotificationAdditionalData() { PackageQuantityReadyToOut = 1, PackageWeightReadyToOut = 7.654321m };
			AssertEquals(7.654m, outNotificationAdditionalData.PackageWeightReadyToOut);
		}

		public void Test_ConsNotificationAdditionalData_WeightIsRounded()
		{
			var consNotificationAdditionalData = new ConsNotificationAdditionalData() { PackageQuantityToCons = 1, PackageWeightToCons = 1.234567m };
			AssertEquals(1.235m, consNotificationAdditionalData.PackageWeightToCons);
		}

		#endregion

		#region Test MasterBill With Hyphen

		public void TestMasterBillWithHyphen()
		{
			var history1 = new NotificationHistoryInfo() { MasterBill = "023-12345" };
			AssertEquals("02312345", history1.MasterBillWithoutHyphen);

			var history2 = new NotificationHistoryInfo() { MasterBill = "02312345" };
			AssertEquals("02312345", history2.MasterBillWithoutHyphen);
		}

		#endregion
	}
}
