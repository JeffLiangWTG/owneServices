using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module.Testing
{
	sealed class CusAuthorisationsFilterStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			CombineAssertions(() =>
			{
				using (var filterStrip = new CusAuthorisationsFilterStripForTesting())
				{
					filterStrip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
					using (var result = filterStrip.GetCurrentFilterControls(new CusAuthorisationsRuleModuleFilter("Test", (val1, op, val2) => new ZQuery(), new CodeDescriptionPairList())))
					{
						AssertType<CusAuthorisationsRuleModuleFilterStrip>("CusAuthorisationsRuleModuleFilter returns CusAuthorisationsRuleModuleFilterStrip", result);
					}

					AssertNull("ModuleTextFilter returns null", filterStrip.GetCurrentFilterControls(new ModuleTextFilter("hello", GlbStaffSchema.GS_Code)));
				}
			});
		}

		sealed class CusAuthorisationsFilterStripForTesting : CusAuthorisationsFilterStrip
		{
			public new Control GetCurrentFilterControls(ModuleFilter currentModuleFilter) => base.GetCurrentFilterControls(currentModuleFilter)?[0];
		}
	}
}
