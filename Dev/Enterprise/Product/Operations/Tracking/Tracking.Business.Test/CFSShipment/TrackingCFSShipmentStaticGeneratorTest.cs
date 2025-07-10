using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingCFSShipmentStaticGeneratorTest : TestCaseWithFactory
	{
		public TrackingCFSShipmentStaticGeneratorTest()
		{
			helper = new TestHelper(Factory);
		}

		readonly TestHelper helper;

		protected override void SetUp()
		{
			base.SetUp();
			CreateTestShipment();
			Factory.Save();
		}

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			base.TearDown();
		}

		void CreateTestShipment()
		{
			testShipment = Factory.NewWithValidTestData<TrackingCFSShipment>();
			testShipment.JS_UniqueConsignRef = "S123456789";
			testShipment.ConsigneePK = helper.TestOrg.PK;
		}
		TrackingCFSShipment testShipment;

		#region FromNumber Test

		public void TestFromNumber()
		{
			var testShipmentFromNumber = TrackingCFSShipment.FromPKFilteredBySiteUser(Factory, testShipment.PK, helper.TestSiteUser);
			AssertEquals(testShipment, testShipmentFromNumber);
			AssertEquals(helper.TestOrg.PK, testShipment.LoggedInOrganisation.PK);
			AssertEquals(helper.TestContact.PK, testShipment.LoggedInContact.PK);
		}

		public void TestFromNumberIncorrectOrg()
		{
			var testShipmentFromNumber = TrackingCFSShipment.FromPKFilteredBySiteUser(Factory, testShipment.PK, null);
			AssertNull(testShipmentFromNumber);
		}

		#endregion FromNumber Test
	}
}
