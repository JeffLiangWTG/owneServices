using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Module.Testing
{
	public class GlbAccreditationAttemptFilterStripTest : TestCaseWithFactory
	{
		public void TestHandlesLatestAttemptForGroupFilter()
		{
			var filter = new GlbAccreditationHighestLevelByProgramFilter("description");
			using (var strip = new TestGlbAccreditationAttemptFilterStrip())
			{
				var controls = strip.GetCurrentFilterControls(filter);
				try
				{
					AssertEquals("Should be 1 control", 1, controls.Length);
					AssertType("Control's Type", typeof(GlbAccreditationHighestLevelByProgramFilterControl), controls[0]);
				}
				finally
				{
					DisposeControls(controls);
				}
			}
		}

		#region Test Classes
		class TestGlbAccreditationAttemptFilterStrip : GlbAccreditationAttemptFilterStrip
		{
			public new Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}

		#endregion
		#region Implementation
		void DisposeControls(Control[] controls)
		{
			foreach (var control in controls)
			{
				control.Dispose();
			}
		}
		#endregion
	}
}
