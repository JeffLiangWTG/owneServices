using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module.Testing
{
	public class CusPermitModuleStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (var filterStrip = new CusPermitModuleStripForTest())
			{
				filterStrip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				using (var result = filterStrip.GetCurrentFilterControlsForTest(new PermitTypeModuleFilterForTest(CusPermitFilterStripBusinessObject.Schema.PermitTypeSubType, (val0, val1, val2) => new ZQuery())))
				{
					AssertEquals(typeof(PermitTypeFilterStrip), result.GetType());
				}

				using (var result = filterStrip.GetCurrentFilterControlsForTest(new PermitRuleModuleFilterForTest(CusPermitFilterStripBusinessObject.Schema.PermitRule, (val0, val1, val2) => new ZQuery())))
				{
					AssertEquals(typeof(PermitRuleFilterStrip), result.GetType());
				}
			}
		}

		sealed class CusPermitModuleStripForTest : CusPermitModuleStrip
		{
			public Control GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter) => GetCurrentFilterControls(currentModuleFilter)[0];
		}
	}
}
