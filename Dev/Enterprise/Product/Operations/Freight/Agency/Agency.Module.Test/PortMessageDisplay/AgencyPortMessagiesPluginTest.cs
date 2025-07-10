using System.Reflection;
using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class AgencyPortMessagiesPluginTest : BaseAgencyTest
	{
		#region TestBusinessEntityForPlugin

		public void TestBusinessEntityForPlugin()
		{
			AssertEquals(typeof(PortMessageHostCollection), Plugin.BusinessEntity.GetType());
		}

		#endregion

		#region TestUserControl

		public void TestUserControl()
		{
			AssertEquals(typeof(PortMessageDisplayControl), Plugin.UserControl.GetType());
		}

		#endregion

		#region TestTopLevelMenu

		public void TestGetNewTopLevelMenu()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				using (AgencyPortMessagesPlugin plugin = new AgencyPortMessagesPlugin(voyage))
				{
					MenuItem topLevelMenuItem = GetNewTopLevelMenu(plugin);
					AssertNotNull("Should only have one menu item", topLevelMenuItem);
					AssertEquals("TopLevelMenuItem should have the correct text", "Send Port Authority Message", topLevelMenuItem.Text);
				}
			}
		}

		public void TestGetNewTopLevelMenuSecurityDenied()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			Env.Security.SailingSchedulePortMessaging.IsAllowed = false;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				using (AgencyPortMessagesPlugin plugin = new AgencyPortMessagesPlugin(voyage))
				{
					MenuItem topLevelMenuItem = GetNewTopLevelMenu(plugin);
					AssertNotNull("Should only have one menu item", topLevelMenuItem);
					AssertEquals("Menu item of topLevelMenuItem should have the correct error text", "Access denied, click this menu for details.", topLevelMenuItem.Text);
				}
			}
		}

		public void TestGetNewTopLevelMenu_CountryIsNotAustralia_DoNotAddPluginMenu()
		{
			var voyage = Factory.New<JobVoyage>();
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Ukraine))
			{
				using (var plugin = new AgencyPortMessagesPlugin(voyage))
				{
					AssertNotNull("Should only have one menu item", plugin.TopLevelMenu);
					AssertEquals("Item text", "Not available, click this menu for details.", plugin.TopLevelMenu.Text);
					plugin.TopLevelMenu.PerformClick();
					AssertEquals("Message when the menu item is clicked", "Error Ports in your country/region are not configured for Port Messages. Contact WiseTech Global for a possibility of having this connection established.", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
		}

		#endregion

		#region Implementation

		#region Voyage

		JobVoyage Voyage
		{
			get
			{
				return voyage ?? (voyage = Factory.New<JobVoyage>());
			}
		}

		JobVoyage voyage;

		#endregion

		#region Plugin

		AgencyPortMessagesPlugin Plugin
		{
			get
			{
				return plugin ?? (plugin = new AgencyPortMessagesPlugin(Voyage));
			}
		}

		AgencyPortMessagesPlugin plugin;

		#endregion

		#region Menu

		MenuItem GetNewTopLevelMenu(AgencyPortMessagesPlugin plugin)
		{
			return (MenuItem)typeof(ZPlugIn).GetMethod("GetNewTopLevelMenu", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(plugin, System.Array.Empty<object>());
		}

		#endregion

		#region TestCleanUp

		protected override void TearDown()
		{
			if (plugin != null)
			{
				plugin.Dispose();
			}

			base.TearDown();
		}

		#endregion

		#endregion
	}
}
