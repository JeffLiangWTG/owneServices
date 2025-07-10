using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingLinerAndAgencyBookingModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			var testLinerAndAgencyBooking = Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();
			Factory.Save();

			var linerAndAgencyBooking = Factory.LoadTop1<TrackingLinerAndAgencyBooking>(new ZQuery(JobShipmentSchema.PK, testLinerAndAgencyBooking.PK));

			var milestone1 = linerAndAgencyBooking.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			linerAndAgencyBooking.ReloadMilestones();
			AssertNotEquals(0, linerAndAgencyBooking.Milestones.Count);

			return linerAndAgencyBooking;
		}
	}
}
