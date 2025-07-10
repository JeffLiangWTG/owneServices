using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class AdditionalFilterStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (var filterStrip = new FTZAdmissionNumberFilterStripForTest())
			{
				filterStrip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				using (var result = filterStrip.GetCurrentFilterControlsForTest(new FTZAdmissionNumberFilter(DeclarationFilterConstants.FTZAdmissionNumber)))
				{
					AssertEquals(typeof(FTZAdmissionNumberControl), result.GetType());
				}

				using (var result = filterStrip.GetCurrentFilterControlsForTest(new OrgClientAssignedStaffModuleFilter("Test", q => new ZQuery())))
				{
					AssertEquals(typeof(OrgClientAssignedStaffFilterStrip), result.GetType());
				}
			}
		}

		sealed class FTZAdmissionNumberFilterStripForTest : AdditionalFilterStrip
		{
			public Control GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter) => GetCurrentFilterControls(currentModuleFilter)[0];
		}
	}
}
