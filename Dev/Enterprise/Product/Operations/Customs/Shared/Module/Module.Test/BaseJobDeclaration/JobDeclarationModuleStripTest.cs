using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	sealed class JobDeclarationModuleStripTest : TestCase
	{
		public void TestClientAssignedStaffModuleFilter()
		{
			var filter = new OrgClientAssignedStaffModuleFilter("Test", q => new ZQuery());
			using (var strip = new TestJobDeclarationModuleStrip())
			{
				var controls = strip.GetCurrentFilterControlsForTest(filter);
				try
				{
					AssertEquals(1, controls.Length);
					AssertEquals(typeof(OrgClientAssignedStaffFilterStrip), controls[0].GetType());
				}
				finally
				{
					controls[0].Dispose();
				}
			}
		}

		sealed class TestJobDeclarationModuleStrip : JobDeclarationModuleStrip
		{
			public Control[] GetCurrentFilterControlsForTest(ModuleFilter filter)
			{
				return GetCurrentFilterControls(filter);
			}
		}
	}
}
