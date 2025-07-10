using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyFilterBizOForWorkflowLastCompletedMilestone : DummyFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = new ModuleFilterCollection();

			collection.AddCustomFilter(new WorkflowModuleFilter("last c", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneLastCompleted));

			return collection;
		}
	}
}
