using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(AttachedHVLVOriginLoadListCollection))]
	class AttachedHVLVOriginLoadListCollectionTest : ActiveBusinessObjectCollectionTestCase<AttachedHVLVOriginLoadListCollection>
	{
		public void TestCollection()
		{
			var loadList1 = CreateLoadList(Core.Constants.ELoadListStatuses.Lodged);
			var loadList2 = CreateLoadList(Core.Constants.ELoadListStatuses.Open);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			Factory.Save();
			ReleaseFactory();

			var loadLists = new AttachedHVLVOriginLoadListCollection(Factory, consol.JK_MasterBillNum);
			var loadList3 = CreateLoadList(Core.Constants.ELoadListStatuses.Lodged);

			AssertContainsExactElementsInAnyOrder(new[] { loadList1.PK, loadList3.PK }, loadLists.Select(x => x.PK));
		}

		protected override AttachedHVLVOriginLoadListCollection GetCollectionToTest()
		{
			return new AttachedHVLVOriginLoadListCollection(Factory, "MBN20181211");
		}

		public void TestGetExtraNotification()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "MBN20181211";
			var extraNotificationProvider = new AttachedHVLVOriginLoadListCollection(Factory, consol.JK_MasterBillNum) as IFilterModuleExtraNotificationProvider;

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_MasterBillNumber = "";

			var notification = extraNotificationProvider.GetExtraNotification(loadList);
			AssertNull("Collection should have no errors when loadlist master bill number is empty", notification);

			loadList.HVL_MasterBillNumber = "MBN20181211";

			notification = extraNotificationProvider.GetExtraNotification(loadList);
			AssertNull("Collection should have no errors when loadlist master bill  number matches the current consol's", notification);

			CombineAssertions("Collection should have a warning when the loadlist master bill number matches another consol's master bill number", () =>
			{
				var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
				consol2.JK_MasterBillNum = "MBN20181212";
				loadList.HVL_MasterBillNumber = "MBN20181212";
				notification = extraNotificationProvider.GetExtraNotification(loadList);
				AssertNotNull("Collection should have a warning", notification);
				AssertEquals(CargoWise.ComponentModel.NotificationType.Warning, notification.Type);
				AssertEquals($"Origin Load List {loadList.HumanReadableName}: This Master Bill Number {loadList.HVL_MasterBillNumber} already exists on another Consol", notification.Message);
			});

			CombineAssertions("Collection should have a warning when loadlist does not match current consol master bill Number or any other consol's master bill number", () =>
			{
				loadList.HVL_MasterBillNumber = "MBN20181213";
				notification = extraNotificationProvider.GetExtraNotification(loadList);
				AssertNotNull("Collection should have a warning", notification);
				AssertEquals(CargoWise.ComponentModel.NotificationType.Warning, notification.Type);
				AssertEquals($"Origin Load List {loadList.HumanReadableName}: This Master Bill Number {loadList.HVL_MasterBillNumber} does not match the current Consol Master Bill Number {consol.JK_MasterBillNum}", notification.Message);
			});
		}

		public void TestDefaultOriginLoadListStatus()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var loadLists = new AttachedHVLVOriginLoadListCollection(Factory, consol.JK_MasterBillNum);
			var loadList = loadLists.AddNew();

			AssertEquals("The default status should be lodged", Core.Constants.ELoadListStatuses.Lodged, loadList.HVL_Status);
		}

		HVLVOriginLoadList CreateLoadList(string status)
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = status;

			return loadList;
		}
	}
}
