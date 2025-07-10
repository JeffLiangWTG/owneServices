using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.Documents.Module.Testing
{
	sealed class ETerminalReleaseManifestPortMessagingPluginTest : TestCaseWithFactory
	{
		public void TestTopLevelMenu()
		{
			using (var plugin = new ETerminalReleaseManifestPortMessagingPlugin(Factory.New<JobVoyage>()))
			{
				var menuItem = (MenuItem)typeof(ZPlugIn).GetMethod("GetNewTopLevelMenu", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(plugin, System.Array.Empty<object>());
				AssertEquals("eTerminal Release Manifest", menuItem.Text);

				menuItem.PerformClick();

				using (var form = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals("Should be showing the right form.", typeof(ETerminalReleaseMessageDialog), form.GetType());
				}
			}
		}
	}
}
