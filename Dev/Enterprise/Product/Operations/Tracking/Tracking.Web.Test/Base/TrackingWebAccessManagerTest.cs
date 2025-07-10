using System.Collections.Generic;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TrackingWebAccessManagerTest : ZWebAccessManagerTest
	{
		#region Test Cases

		protected override void SetupTestItemsForLicenceCheckpoints(Dictionary<string, ILicenceCheckpoint[]> itemsToTest)
		{
			itemsToTest.Add(TrackingConstants.RelativePath.BookingsPage, new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerBooking });
			itemsToTest.Add(TrackingConstants.RelativePath.BookingDetailsPage, new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerBooking });
			itemsToTest.Add(TrackingConstants.RelativePath.EditBookingPage, new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerBooking });
		}

		#endregion

		#region Overrides

		protected override IWebAccessManager GetNewWebAccessManager()
		{
			return new TrackingWebAccessManager(new ZTestGlobal());
		}

		#endregion
	}
}
