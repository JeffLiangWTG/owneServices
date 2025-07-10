using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	public class OrdersModuleStripTest : TestCase
	{
		public void TestClientAssignedStaffModuleFilter()
		{
			var filter = new OrgClientAssignedStaffModuleFilter("Test", q => new ZQuery());
			using (var strip = new TestOrdersModuleStrip())
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

		class TestOrdersModuleStrip : OrdersBaseModuleStrip
		{
			public Control[] GetCurrentFilterControlsForTest(ModuleFilter filter)
			{
				return GetCurrentFilterControls(filter);
			}
		}
	}
}
