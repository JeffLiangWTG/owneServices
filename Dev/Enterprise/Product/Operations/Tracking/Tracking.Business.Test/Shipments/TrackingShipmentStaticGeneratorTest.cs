using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingShipmentStaticGeneratorTest : TestCaseWithFactory
	{
		public TrackingShipmentStaticGeneratorTest()
		{
			Helper = new TestHelper(Factory);
		}

		readonly TestHelper Helper;

		protected override void SetUp()
		{
			base.SetUp();
			CreateTestShipment();
			Factory.Save();
		}

		void CreateTestShipment()
		{
			TestShipment = Factory.NewWithValidTestData<TrackingShipment>();
			TestShipment.JS_UniqueConsignRef = "S123456789";
			TestShipment.ConsigneePK = Helper.TestOrg.PK;
		}
		TrackingShipment TestShipment;

		public void TestRelatedShipments()
		{
			TestShipment.SiteUser = Helper.TestSiteUser;
			TestShipment.CoLoadShipments.Add(Factory.NewWithValidTestData<TrackingShipment>());
			TestShipment.CoLoadShipments.Add(Factory.NewWithValidTestData<TrackingShipment>());
			TestShipment.CoLoadShipments[0].ConsigneePK = Helper.TestOrg.PK;
			Factory.Save();

			var result = Factory.LoadTop1<TrackingShipment>(new ZQuery(JobShipmentSchema.PK, TestShipment.PK));

			AssertEquals(result.CoLoadShipments.Count, 2);
			AssertEquals("related shipments should be filtered by contact", result.RelatedShipments.Count, 1);
		}

		#region FromNumber Test

		public void TestFromNumber()
		{
			TrackingShipment testShipmentFromNumber = TrackingShipment.FromPKFilteredBySiteUser(Factory, TestShipment.PK, Helper.TestSiteUser);
			AssertEquals(TestShipment, testShipmentFromNumber);
			AssertEquals(Helper.TestOrg.PK, TestShipment.LoggedInOrganisation.PK);
			AssertEquals(Helper.TestContact.PK, TestShipment.LoggedInContact.PK);
		}

		public void TestFromNumberIncorrectOrg()
		{
			TrackingShipment testShipmentFromNumber = TrackingShipment.FromPKFilteredBySiteUser(Factory, TestShipment.PK, null);
			AssertNull(testShipmentFromNumber);
		}

		#endregion FromNumber Test
	}
}
