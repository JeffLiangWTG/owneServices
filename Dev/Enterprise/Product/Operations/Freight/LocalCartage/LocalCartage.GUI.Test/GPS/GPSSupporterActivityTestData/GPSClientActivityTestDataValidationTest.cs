using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.GPS.Business;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	class GPSClientActivityTestDataValidationTest : TestCaseWithFactory
	{
		public void TestValidateEN_ActivityType()
		{
			var activity = Factory.NewWithValidTestData<GPSSupporterActivityTestData>();
			activity.EN_ActivityType = "TST";
			AssertEquals(true, activity.EN_ActivityTypeInfo.HasErrors());
			activity.EN_ActivityType = GPSConstants.GPSInOutActivityType.Codes.GIN;
			AssertEquals(false, activity.EN_ActivityTypeInfo.HasErrors());
		}

		public void TestValidateClient()
		{
			var activity = Factory.NewWithValidTestData<GPSSupporterActivityTestData>();
			activity.ClientPK = ZGuid.Invalid;
			AssertEquals(true, activity.ClientPKInfo.HasErrors());
			var testOrg = Helper.CreateOrgHeader("Org1", "org1Address1");
			activity.ClientPK = testOrg.PK;
			AssertEquals(false, activity.ClientPKInfo.HasErrors());
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
