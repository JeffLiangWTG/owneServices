using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class StowPlanNotificationTest : TestCaseWithFactory
	{
		public void TestStowPlanNotification()
		{
			var bill = Factory.New<BillOfLading>();
			var billData = new StowPlanShipmentData(bill);
			var stwNotification = new StowPlanNotification(billData, NotificationType.Error, "Some error", "Some error");
			AssertEquals(bill.PK, stwNotification.TargetPK);
			AssertEquals(bill.TablePrefix, stwNotification.TargetCode);
			AssertEquals(bill.HumanReadableName, stwNotification.Subject);
		}
	}
}
