using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyFilterStripBizOWithWorkflowFiltersAndRelatedJobFilters : DummyFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var helper = new WorkflowFilterStripsHelperForTest(typeof(OrgHeader), ZString.Empty, Factory);
			helper.AddMilestonesFilters_Exposed(result);
			helper.AddRelatedMilestoneFilters_Exposed(result, new ZDBOnlySubQuery(typeof(DummyWithWorkflow), DummyBizoSchema.Z0_Guid));

			return result;
		}
	}
}
