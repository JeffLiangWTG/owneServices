using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingWhsReceiveValidationTest : WhsReceiveValidationTest
	{
		#region TestCheckWD_WW_Whs

		[SetGlobalsIsWeb]
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

			var receive = WebHelper.CreateWhsReceive();
			receive.WhsReceive.WD_WW_Whs = whsGranted.PK;
			AssertNoErrors(receive.WhsReceive.WD_WW_WhsInfo);

			receive.WhsReceive.WD_WW_Whs = whsDenied.PK;
			AssertHasError("This warehouse is prohibited, so there should be an error.", receive.WhsReceive.WD_WW_WhsInfo, "You are not authorized for warehouse WH2 (WH2). Please contact your system administrator to request access rights.");
		}

		#endregion

		#region Helper

		TestHelper WebHelper
		{
			get { return fWebHelper ?? (fWebHelper = new TestHelper(Factory)); }
		}
		TestHelper fWebHelper;

		#endregion

		#region Overrides

		new TrackingWhsReceive GetNewBusinessObject()
		{
			return TrackingHelper.Get(Docket);
		}

		#endregion
	}
}
