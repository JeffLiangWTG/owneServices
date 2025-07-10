using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class ReconFilterStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (var filterStrip = new ReconFilterStripForTest())
			{
				filterStrip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				using (var result = filterStrip.GetCurrentFilterControlsForTest(new EntryNoOnReconciliationFilter(ReconFilterStripBusinessObject.Schema.EntryNumberOnReconciliation)))
				{
					AssertEquals(typeof(EntryNoOnReconciliationControl), result.GetType());
				}
			}
		}

		sealed class ReconFilterStripForTest : ReconFilterStrip
		{
			public Control GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter) => GetCurrentFilterControls(currentModuleFilter)[0];
		}
	}
}
