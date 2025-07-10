using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingBillOfLadingModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			TrackingBillOfLading testBill = Factory.NewWithValidTestData<TrackingBillOfLading>();
			Factory.Save();

			var billOfLading = Factory.LoadTop1<TrackingBillOfLading>(new ZQuery(JobShipmentSchema.PK, testBill.PK));

			var milestone1 = billOfLading.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			billOfLading.ReloadMilestones();
			AssertNotEquals(0, billOfLading.Milestones.Count);

			return billOfLading;
		}
	}
}
