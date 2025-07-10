using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingDeclarationModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			var testDeclaration = new TrackingDeclaration(Factory.NewWithValidTestData<BaseJobDeclaration>());
			AssertNotNull(testDeclaration.Milestones);

			var milestone1 = testDeclaration.Declaration.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			testDeclaration.ReloadMilestones();
			AssertNotEquals(0, testDeclaration.Milestones.Count);

			return testDeclaration;
		}
	}
}
