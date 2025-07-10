using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingOrderStaticGeneratorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			Helper = new TestHelper(Factory);
			CreateTestOrder();
			Factory.Save();

			Helper.TestSiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
		}
		TestHelper Helper;

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			base.TearDown();
		}

		void CreateTestOrder()
		{
			TestOrder = Factory.New<TrackingOrder>();
			TestOrder.JD_OrderNumber = "S123456789";
			TestOrder.SupplierPK = Helper.TestOrg.PK;
			TestOrder.BuyerPK = Helper.TestOrg.PK;
		}
		TrackingOrder TestOrder;

		public void TestOrderLinesAreNotCancelled()
		{
			OrderLine ol1 = Factory.NewWithValidTestData<OrderLine>();
			OrderLine ol2 = Factory.NewWithValidTestData<OrderLine>();
			TestOrder.OrderLines.Add(ol1);
			TestOrder.OrderLines.Add(ol2);
			ol1.JO_LineStatus = Core.Constants.OrderStatus.PartDelivered;
			ol2.JO_LineStatus = Core.Constants.OrderStatus.Cancelled;
			AssertCollectionContains("Delivered OrderLine is in collection", ol1, TestOrder.OrderLines);
			AssertCollectionNotContains("Cancelled OrderLine is not in collection", ol2, TestOrder.OrderLines);
		}

		#region TestFromNumber

		public void TestFromPK()
		{
			TrackingOrder testOrderFromPK = TrackingOrder.FromPKFilteredByContact(Factory, TestOrder.PK, Helper.TestSiteUser);
			AssertEquals(TestOrder, testOrderFromPK);
			AssertEquals("LoggedInOrg", Helper.TestOrg.PK, TestOrder.LoggedInOrganisation.PK);
			AssertEquals("LoggedInContact", Helper.TestContact.PK, TestOrder.LoggedInContact.PK);
			AssertEquals("Order AutoCreatedLogDefaultSL_Reference", Helper.TestSiteUser.ContactAndCompanyReference, TestOrder.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		public void TestFromPKQuickShipment()
		{
			TrackingSiteUser user = new TrackingSiteUser();
			user.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			TrackingOrder testOrderFromPK = TrackingOrder.FromPKFilteredByContact(Factory, TestOrder.PK, user);
			AssertEquals(TestOrder, testOrderFromPK);
		}

		public void TestFromNumber()
		{
			TrackingOrder testOrderFromNumber = TrackingOrder.FromNumberFilteredByContact(Factory, TestOrder.Number, Helper.TestSiteUser);
			AssertEquals(TestOrder, testOrderFromNumber);
			AssertEquals("LoggedInOrg", Helper.TestOrg.PK, TestOrder.LoggedInOrganisation.PK);
			AssertEquals("LoggedInContact", Helper.TestContact.PK, TestOrder.LoggedInContact.PK);
			AssertEquals("Order AutoCreatedLogDefaultSL_Reference", Helper.TestSiteUser.ContactAndCompanyReference, TestOrder.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		public void TestFromNumberIncorrectOrg()
		{
			if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
			{
				WebEnv.AppInstance.SiteUser.Logout();
			}
			Assert("pre-condition", WebEnv.AppInstance.SiteUser == null || !WebEnv.AppInstance.SiteUser.IsLoggedIn);
			TrackingOrder testOrderFromNumber = TrackingOrder.FromPKFilteredByContact(Factory, TestOrder.PK, null);
			AssertNull(testOrderFromNumber);
		}

		#endregion TestFromNumber
	}
}
