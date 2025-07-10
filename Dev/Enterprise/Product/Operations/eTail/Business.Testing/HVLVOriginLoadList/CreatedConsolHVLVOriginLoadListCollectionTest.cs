using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(CreatedConsolHVLVOriginLoadListCollection))]
	class CreatedConsolHVLVOriginLoadListCollectionTest : ActiveBusinessObjectCollectionTestCase<CreatedConsolHVLVOriginLoadListCollection>
	{
		public void TestCollection()
		{
			var loadList1 = CreateLoadList(Core.Constants.ELoadListStatuses.Lodged);
			var loadList2 = CreateLoadList(Core.Constants.ELoadListStatuses.Open);

			Factory.Save();
			ReleaseFactory();

			var loadLists = new CreatedConsolHVLVOriginLoadListCollection(Factory);
			var loadList3 = CreateLoadList(Core.Constants.ELoadListStatuses.Lodged);

			AssertContainsExactElementsInAnyOrder(new[] { loadList1.PK, loadList3.PK }, loadLists.Select(x => x.PK));
		}

		public void TestGetExtraNotification()
		{
			var extraNotificationProvider = new CreatedConsolHVLVOriginLoadListCollection(Factory) as IFilterModuleExtraNotificationProvider;

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_MasterBillNumber = "MBN20181211";

			var notification = extraNotificationProvider.GetExtraNotification(loadList);
			AssertNull("Collection should have no errors", notification);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "MBN20181211";
			consol.JK_UniqueConsignRef = "C00001000";

			notification = extraNotificationProvider.GetExtraNotification(loadList);

			AssertNotNull("Collection should have an error", notification);
			AssertEquals($"The Master Bill Number MBN20181211 already exists on Consol C00001000, please attach Load List within the Consol via Actions > Attach HVLV Origin Load List", notification.Message);
		}

		public void TestDefaultOriginLoadListStatus()
		{
			var loadLists = new CreatedConsolHVLVOriginLoadListCollection(Factory);
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
