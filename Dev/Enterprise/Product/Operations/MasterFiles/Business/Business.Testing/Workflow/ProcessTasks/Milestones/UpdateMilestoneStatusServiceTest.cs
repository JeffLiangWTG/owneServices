using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UpdateMilestoneStatusServiceTest : TestCaseWithFactory
	{
		public void TestServiceWithEmptyBusinessObjects()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Hi Dave";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			Factory.Save();
			var service = WorkflowAfterOnSavingBOService.TryHookupMilestoneStatusService(Factory, new MilestoneCollectionView(job.WorkflowItems));
			AssertNoExceptionThrown(() => Factory.Save());
		}
	}
}
