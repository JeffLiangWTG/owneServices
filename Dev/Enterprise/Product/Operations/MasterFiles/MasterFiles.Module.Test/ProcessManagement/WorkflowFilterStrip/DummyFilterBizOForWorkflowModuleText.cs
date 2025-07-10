using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyFilterBizOForWorkflowModuleText : DummyFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = new ModuleFilterCollection();

			collection.AddCustomFilter(new WorkflowModuleTextFilter("text", WorkflowModuleTextFilterTest.GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(DummyBusinessObject)));

			return collection;
		}
	}
}
