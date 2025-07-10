using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.US.AMS.Module.Testing
{
	sealed class StowPlanPluginTest : TestCaseWithFactory
	{
		public void TestTopLevelMenu()
		{
			using (var plugin = new StowPlanPlugin(Factory.New<JobVoyage>()))
			{
				var menuItem = (MenuItem)typeof(ZPlugIn).GetMethod("GetNewTopLevelMenu", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(plugin, System.Array.Empty<object>());
				AssertEquals("Stow Plan", menuItem.Text);
				AssertNotNull(menuItem.MenuItems.FindByText("Send Stow Plan Message"));
			}
		}
	}
}
