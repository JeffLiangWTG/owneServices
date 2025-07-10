using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ImporterSupplierFilterStripTest : TestCaseWithFactory
	{
		public void TestABCCategoryWarehouseFilterControlGetsCreated()
		{
			using (var filterStrip = new TestImporterSupplierFilterStrip())
			{
				var abcCategory = new ABCCategoryWarehouseFilter();
				using (var control = filterStrip.GetCurrentFilterControlsForTesting(abcCategory)[0])
				{
					AssertEquals(typeof(ABCCategoryWarehouseFilterControl), control.GetType());
				}
			}
		}

		class TestImporterSupplierFilterStrip : ImporterSupplierFilterStrip
		{
			public Control[] GetCurrentFilterControlsForTesting(ModuleFilter currentModuleFilter)
			{
				return GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}
