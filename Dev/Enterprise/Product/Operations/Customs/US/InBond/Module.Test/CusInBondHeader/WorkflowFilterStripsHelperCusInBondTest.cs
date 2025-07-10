using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	sealed class WorkflowFilterStripsHelperCusInBondTest : WorkflowFilterStripsHelperTest
	{
		public void TestSetAlternativeParentColumn()
		{
			var filterBizo = new CusInBondHeaderFilterStripBusinessObject();
			var filter = (TasksModuleFilter)filterBizo.ModuleFilters["Tasks"];
			AssertEquals("AlternativeParentColumn", CusInBondHeaderSchema.BH_ParentID, filter.AlternativeParentColumn);
		}
	}
}
