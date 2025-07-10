using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingCartageModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			TrackingCartage testCartage = Factory.NewWithValidTestData<TrackingCartage>();
			Factory.Save();

			var cartage = Factory.LoadTop1<TrackingCartage>(new ZQuery(JobCartageSchema.PK, testCartage.PK));

			var milestone1 = ((IWorkflowProvider)cartage).WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			cartage.ReloadMilestones();
			AssertNotEquals(0, cartage.Milestones.Count);

			return cartage;
		}
	}
}
