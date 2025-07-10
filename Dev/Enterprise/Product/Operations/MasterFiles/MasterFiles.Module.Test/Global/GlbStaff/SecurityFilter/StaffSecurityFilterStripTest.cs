using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class StaffSecurityFilterStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (StaffSecurityFilterStrip strip = new StaffSecurityFilterStrip())
			{
				strip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				Control[] result = strip.TestGetCurrentFilterControlsInternal(new StaffSecurityModuleFilter("hello", new GlbBranchCollection(Factory), new GlbDepartmentCollection(Factory)));
				AssertEquals(1, result.Length);
				AssertEquals(typeof(StaffSecurityFilterControl), result[0].GetType());
				result[0].Dispose();

				result = strip.TestGetCurrentFilterControlsInternal(new ModuleTextFilter("hello", GlbStaffSchema.GS_Code));
				AssertNull(result);
			}
		}

		public void TestGetCurrentFilterControlsForLeaveDateRangeWithTypeFilter()
		{
			using (StaffSecurityFilterStrip strip = new StaffSecurityFilterStrip())
			{
				strip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				var result = strip.TestGetCurrentFilterControlsInternal(new LeaveDateRangeWithTypeFilter("hello"));
				AssertEquals(1, result.Length);
				AssertEquals(typeof(LeaveDateRangeWithTypeFilterControl), result[0].GetType());
				result[0].Dispose();
			}
		}
	}
}
