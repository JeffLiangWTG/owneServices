using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingWhsOrderValidationTest : WhsOrderValidationTest
	{
		#region TestCheckWD_WW_Whs

		public void TestCheckWD_WW_Whs()
		{
			var contact = WebHelper.TestSiteUser.LoggedInUser;
			AssertNotNull("Precondition", contact);

			var whsGranted = Helper.CreateWarehouse("WH1");
			var whsDenied = Helper.CreateWarehouse("WH2");

			Helper.ProhibitWarehouseAccessForOrgContact(whsDenied, contact);
			// there are different factories for helper and contact in test environment, so have to save changes
			contact.Factory.Save();
			Factory.Save();

			var order = WebHelper.CreateWhsOrder();
			order.WhsOrder.WD_WW_Whs = whsGranted.PK;
			AssertNoErrors(order.WhsOrder.WD_WW_WhsInfo);

			order.WhsOrder.WD_WW_Whs = whsDenied.PK;
			AssertHasError("This warehouse is prohibited, so there should be an error.", order.WhsOrder.WD_WW_WhsInfo, "You are not authorized for warehouse WH2 (WH2). Please contact your system administrator to request access rights.");
		}

		#endregion

		#region Overrides

		new TrackingWhsOrder GetNewBusinessObject()
		{
			return TrackingHelper.Get(Docket);
		}

		#endregion

		#region Helper

		TestHelper WebHelper
		{
			get
			{
				if (fWebHelper == null)
				{
					fWebHelper = new TestHelper(Factory);
					Factory.Save();
				}
				return fWebHelper;
			}
		}
		TestHelper fWebHelper;
		#endregion

		#region Test Setup

		bool oldIsWeb;

		protected override void SetUp()
		{
			base.SetUp();
			oldIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;
		}

		protected override void TearDown()
		{
			Globals.IsWeb = oldIsWeb;
			base.TearDown();
		}

		#endregion
	}
}
