using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeaderCollectionForGlbGroupLookup))]
	sealed class OrgHeaderCollectionForGlbGroupLookupTest : BusinessObjectCollectionTestCase
	{
		public void TestExtraNotification()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "GG_CODE2";
			org2.MiscServ.OM_GG_OrgSecurityGroup = group2.PK;

			var collection = new OrgHeaderCollectionForGlbGroupLookup(Factory);
			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			AssertNull(notificationProvider.GetExtraNotification(org1));

			var notification = notificationProvider.GetExtraNotification(org2);
			AssertNotNull(notification);
			AssertEquals("Error", notification.Type.EnumValueName);
			AssertEquals("This record has already been added to the Group (GG_CODE2).", notification.Message);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgHeaderCollectionForGlbGroupLookup(Factory);
		}
	}
}
