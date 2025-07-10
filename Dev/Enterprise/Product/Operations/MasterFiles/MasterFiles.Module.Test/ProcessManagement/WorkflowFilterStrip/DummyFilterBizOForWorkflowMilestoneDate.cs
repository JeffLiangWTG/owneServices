using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyFilterBizOForWorkflowMilestoneDate : DummyFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = new ModuleFilterCollection();

			collection.AddCustomFilter(new WorkflowModuleFilter("date", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneDate));

			return collection;
		}
	}
}
