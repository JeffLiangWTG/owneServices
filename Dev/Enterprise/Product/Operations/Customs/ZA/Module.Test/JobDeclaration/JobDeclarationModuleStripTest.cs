using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Module.Testing
{
	sealed class JobDeclarationModuleStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (var filterStrip = new JobDeclarationModuleStripForTest())
			{
				filterStrip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				using (var result = filterStrip.GetCurrentFilterControlsForTest(new ProcedureCodesModuleFilter(DeclarationFilterConstants.CPCAndPPC, (val1, val2) => new ZQuery())))
				{
					AssertEquals(typeof(ProcedureCodesFilterStrip), result.GetType());
				}

				using (var result = filterStrip.GetCurrentFilterControlsForTest(new MasterFiles.Module.OrgClientAssignedStaffModuleFilter("Test", (a) => new ZQuery())))
				{
					AssertEquals(typeof(MasterFiles.Module.OrgClientAssignedStaffFilterStrip), result.GetType());
				}
			}
		}

		sealed class JobDeclarationModuleStripForTest : JobDeclarationModuleStrip
		{
			public Control GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter) => GetCurrentFilterControls(currentModuleFilter)[0];
		}
	}
}
