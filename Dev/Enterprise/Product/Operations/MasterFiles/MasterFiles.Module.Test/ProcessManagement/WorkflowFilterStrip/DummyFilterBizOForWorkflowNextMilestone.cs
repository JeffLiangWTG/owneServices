using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyFilterBizOForWorkflowNextMilestone : DummyFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = new ModuleFilterCollection();

			collection.AddCustomFilter(new WorkflowModuleFilter("next", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneNext));

			return collection;
		}
	}
}
