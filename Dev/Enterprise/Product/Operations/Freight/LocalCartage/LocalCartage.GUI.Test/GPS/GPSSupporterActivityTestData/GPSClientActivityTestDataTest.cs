using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(GPSSupporterActivityTestData))]
	class GPSClientActivityTestDataTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnly()
		{
			var activity = Factory.NewWithValidTestData<GPSSupporterActivityTestData>();
			AssertEquals("Activity should not be read-only", false, activity.ReadOnly);
		}

		public void TestActivityTypes()
		{
			var activity = Factory.NewWithValidTestData<GPSSupporterActivityTestData>();
			AssertEquals("ActivityTypes should have 2 types.", 2, activity.ActivityTypes.Count);
			AssertEquals(GPSConstants.GPSInOutActivityType.Codes.GIN, activity.ActivityTypes[0].Code);
			AssertEquals(GPSConstants.GPSInOutActivityType.Codes.GOT, activity.ActivityTypes[1].Code);
		}

		public void TestClientPK_SetActivityInformation()
		{
			var testOrg = Helper.CreateOrgHeader("Org1", "org1Address1");
			var activity = Factory.NewWithValidTestData<GPSSupporterActivityTestData>();
			AssertEquals("ActivityInformation should not be set", ZString.Empty, activity.EN_ActivityInformation);
			AssertEquals("Client should not be set", null, activity.Client);
			AssertEquals("ClientPK should not be set", ZGuid.Empty, activity.ClientPK);
			activity.ClientPK = testOrg.PK;
			AssertEquals("ActivityInformation should be set", "Org1 - org1Address1", activity.EN_ActivityInformation);
			AssertEquals("Client should be set", testOrg, activity.Client);
			AssertEquals("ClientPK should be set", testOrg.PK, activity.ClientPK);
		}

		public void TestClients()
		{
			var activity = Factory.NewWithValidTestData<GPSSupporterActivityTestData>();
			AssertEquals("The type of Clients is incorrect.", typeof(OrgHeaderCollection), activity.Clients.GetType());
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<GPSSupporterActivityTestData>();
		}
	}
}
