using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Module.Testing
{
	sealed class WorkflowFilterStripsHelperUSAMSTest : WorkflowFilterStripsHelperTest
	{
		public void TestSetAlternativeParentColumn()
		{
			var filterBizo = new USAMSFilterStrip();
			var filter = (TasksModuleFilter)filterBizo.ModuleFilters["Tasks"];
			AssertEquals("AlternativeParentColumn", CusInBondHeaderSchema.BH_ParentID, filter.AlternativeParentColumn);
		}
	}
}
