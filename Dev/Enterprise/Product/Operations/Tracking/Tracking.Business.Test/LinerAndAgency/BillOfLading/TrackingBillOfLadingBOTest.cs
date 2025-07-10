using System;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingBillOfLading))]
	sealed class TrackingBillOfLadingBOTest : BillOfLadingBOTest
	{
		#region Milestones

		public void TestMilestones()
		{
			var testBillOfLading = Factory.NewWithValidTestData<TrackingBillOfLading>();
			AssertNotNull(testBillOfLading.Milestones);
			AssertEquals(0, testBillOfLading.Milestones.Count);

			var milestone1 = testBillOfLading.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = testBillOfLading.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			testBillOfLading.Factory.Save();
			AssertEquals(0, testBillOfLading.Milestones.Count);

			testBillOfLading.ReloadMilestones();
			AssertEquals(2, testBillOfLading.Milestones.Count);
		}

		#endregion

		public void TestShowAgentNotes()
		{
			var testBillOfLading = Factory.NewWithValidTestData<TrackingBillOfLading>();
			AssertEquals("Should not show Agent Notes", false, testBillOfLading.ShowAgentNotes);
		}
	}
}
