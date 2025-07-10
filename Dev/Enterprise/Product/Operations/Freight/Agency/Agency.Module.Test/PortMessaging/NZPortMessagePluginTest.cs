using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Agency.Module.Testing
{
	class NZPortMessagePluginTest : BaseAgencyTest
	{
		#region MenuItem

		public void TestGetNewTopLevelMenu_PortMessagingIsNotAllowed_DoNotAddPluginMenu()
		{
			Env.Security.SailingSchedulePortMessaging.IsAllowed = false;

			using (var plugin = new NZPortMessagePlugin(Factory.New<JobVoyage>()))
			{
				AssertNotNull("Should only have one menu item", plugin.TopLevelMenu);
				AssertEquals("Item text", "Access denied, click this menu for details.", plugin.TopLevelMenu.Text);
				plugin.TopLevelMenu.PerformClick();
				AssertEquals("Message when the menu item is clicked", @"Error You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Schedules -> Sailing Schedule -> Port Messaging", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestGetNewTopLevelMenu_VoyageNotContainsEnabledPorts_MenuIsNotVisible()
		{
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;

			var voyage = Factory.New<JobVoyage>();
			using (var plugin = new NZPortMessagePlugin(voyage))
			{
				AssertNotNull("Should only have one menu item", plugin.TopLevelMenu);
				AssertEquals("Item text", "Send Load and Discharge Manifest Message", plugin.TopLevelMenu.Text);
				AssertEquals("Should not display the menu item", false, plugin.TopLevelMenu.Visible);
			}
		}

		public void TestGetNewTopLevelMenu_VoyageContainsEnabledPorts_MenuIsVisible()
		{
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;

			var portCollection = new PortManifestPortCollection();
			CreatePortManifestPort(portCollection, "NZAKL");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var voyage = Factory.NewWithValidTestData<JobVoyage>();

				var origin = voyage.Origins.AddNew();
				origin.FillWithValidTestData();
				origin.JA_RL_NKPortOfLoading = "NZAKL";

				using (var plugin = new NZPortMessagePlugin(voyage))
				{
					AssertNotNull("Should only have one menu item", plugin.TopLevelMenu);
					AssertEquals("Item text", "Send Load and Discharge Manifest Message", plugin.TopLevelMenu.Text);
					AssertEquals("Should display the menu item", true, plugin.TopLevelMenu.Visible);
				}
			}
		}

		#endregion

		#region MenuItem Visibility

		public void TestChangeTheVisibility()
		{
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;

			var portCollection = new PortManifestPortCollection();
			CreatePortManifestPort(portCollection, "NZAKL", false);
			CreatePortManifestPort(portCollection, "NZAKL");
			CreatePortManifestPort(portCollection, "NZLYT", false);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var voyage = Factory.NewWithValidTestData<JobVoyage>();

				var origin = voyage.Origins.AddNew();
				origin.FillWithValidTestData();
				origin.JA_RL_NKPortOfLoading = "NZAKL";

				Factory.Save();

				using (var testForm = new ZJobVoyageForm(voyage))
				{
					testForm.Show();

					Application.DoEvents();

					var shippingManagerMenu = testForm.Menu.MenuItems.FindByText("Shipping Manager");
					shippingManagerMenu.PerformClick();

					AssertNotEquals(1, shippingManagerMenu.MenuItems.Count);

					var messageMenu = shippingManagerMenu.MenuItems.FindByText("Send Load and Discharge Manifest Message");

					AssertEquals(true, shippingManagerMenu.Visible);
					AssertEquals(true, messageMenu.Visible);

					origin.JA_RL_NKPortOfLoading = "NZLYT";

					AssertEquals(false, shippingManagerMenu.Visible);
					AssertEquals(false, messageMenu.Visible);
				}
			}

			portCollection = new PortManifestPortCollection();
			CreatePortManifestPort(portCollection, "NZAKL", false);
			CreatePortManifestPort(portCollection, "NZLYT", false);

			var portCollection2 = new PortManifestPortCollection();
			CreatePortManifestPort(portCollection, "NZAKL", true);
			CreatePortManifestPort(portCollection, "NZLYT", false);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, portCollection2))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var voyage = Factory.NewWithValidTestData<JobVoyage>();

				var origin = voyage.Origins.AddNew();
				origin.FillWithValidTestData();
				origin.JA_RL_NKPortOfLoading = "NZAKL";

				Factory.Save();

				using (var testForm = new ZJobVoyageForm(voyage))
				{
					testForm.Show();

					Application.DoEvents();

					var shippingManagerMenu = testForm.Menu.MenuItems.FindByText("Shipping Manager");
					shippingManagerMenu.PerformClick();

					AssertNotEquals(1, shippingManagerMenu.MenuItems.Count);

					var messageMenu = shippingManagerMenu.MenuItems.FindByText("Send Load and Discharge Manifest Message");

					AssertEquals(true, shippingManagerMenu.Visible);
					AssertEquals(true, messageMenu.Visible);

					origin.JA_RL_NKPortOfLoading = "NZLYT";

					AssertEquals(false, shippingManagerMenu.Visible);
					AssertEquals(false, messageMenu.Visible);
				}
			}
		}

		public void TestChangeTheVisibility_WhenParentOnlyHasOneSubMenuItem()
		{
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;

			var portCollection = new PortManifestPortCollection();
			CreatePortManifestPort(portCollection, "NZAKL");
			CreatePortManifestPort(portCollection, "NZLYT", false);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var voyage = Factory.NewWithValidTestData<JobVoyage>();

				var origin = voyage.Origins.AddNew();
				origin.FillWithValidTestData();
				origin.JA_RL_NKPortOfLoading = "NZAKL";

				Factory.Save();

				using (var testForm = new ZJobVoyageForm(voyage))
				{
					testForm.Show();

					Application.DoEvents();

					var shippingManagerMenu = testForm.Menu.MenuItems.FindByText("Shipping Manager");
					shippingManagerMenu.PerformClick();

					var messageMenu = shippingManagerMenu.MenuItems.FindByText("Send Load and Discharge Manifest Message");

					shippingManagerMenu.MenuItems.Clear();
					shippingManagerMenu.MenuItems.Add(messageMenu);

					AssertEquals(1, shippingManagerMenu.MenuItems.Count);

					AssertEquals(true, shippingManagerMenu.Visible);
					AssertEquals(true, messageMenu.Visible);

					origin.JA_RL_NKPortOfLoading = "NZLYT";

					AssertEquals(false, shippingManagerMenu.Visible);
					AssertEquals(false, messageMenu.Visible);
				}
			}
		}

		#endregion

		#region Send Menu Item Click

		public void TestSendMenuItemClick_VoyageIsUnsabved_ShowMessageBox()
		{
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;

			var portCollection = new PortManifestPortCollection();
			CreatePortManifestPort(portCollection, "NZAKL");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var voyage = Factory.New<JobVoyage>().With(jV_VoyageFlight: "EK415");

				var origin = voyage.Origins.AddNew();
				origin.FillWithValidTestData();
				origin.JA_RL_NKPortOfLoading = "NZAKL";

				using (var plugin = new NZPortMessagePlugin(voyage))
				{
					plugin.TopLevelMenu.PerformClick();

					AssertEquals("Error This schedule has changes, please save and try again.", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
		}

		public void TestSendMenuItemClick_VoyageIsSlotWhenMainAvailable_ShowMessageBox()
		{
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;

			var portCollection = new PortManifestPortCollection();
			CreatePortManifestPort(portCollection, "NZAKL");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var mainVoyage = Factory.New<JobVoyage>().With(jV_RV_NKVessel: "ABC", jV_VoyageFlight: "EK415", jV_VoyageType: Constants.VoyageType.MainVoyage);
				var slotVoyage = Factory.New<JobVoyage>().With(jV_RV_NKVessel: "ABC", jV_VoyageFlight: "EK415", jV_VoyageType: Constants.VoyageType.SlotVoyage);

				var mainVoyageOrigin = mainVoyage.Origins.AddNew();
				mainVoyageOrigin.FillWithValidTestData();
				mainVoyageOrigin.JA_RL_NKPortOfLoading = "NZAKL";

				var mainDestinations = mainVoyage.Destinations.AddNew();
				mainDestinations.FillWithValidTestData();
				mainDestinations.JB_RL_NKPortOfDischarge = "DEECK";
				mainDestinations.JB_E_ARV = new DateTime(2022, 11, 30);

				mainVoyage.GenerateSailings();

				var slotVoyageOrigin = slotVoyage.Origins.AddNew();
				slotVoyageOrigin.FillWithValidTestData();
				slotVoyageOrigin.JA_RL_NKPortOfLoading = "NZAKL";

				Factory.Save();

				using (var plugin = new NZPortMessagePlugin(slotVoyage))
				{
					plugin.TopLevelMenu.PerformClick();

					AssertEquals("Information You have Main and Slot schedules for this vessel. Please send Load and Discharge Manifest Message from the Main schedule.", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
		}

		public void TestSendMenuItemClick_VoyageHasErrors_ShowMessageBox()
		{
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;

			var portCollection = new PortManifestPortCollection();
			CreatePortManifestPort(portCollection, "NZAKL");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var voyage = Factory.New<JobVoyage>().With(jV_VoyageFlight: "EK415", jV_VoyageType: Constants.VoyageType.MainVoyage);

				var origin = voyage.Origins.AddNew();
				origin.FillWithValidTestData();
				origin.JA_RL_NKPortOfLoading = "NZAKL";

				Factory.Save();

				using (var plugin = new NZPortMessagePlugin(voyage))
				{
					plugin.TopLevelMenu.PerformClick();

					AssertEquals("Error This schedule has errors, please fix and try again.", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
		}

		#endregion

		#region Implementation

		[ThreadSafe]
		static int senderID = 0;

		internal PortManifestPort CreatePortManifestPort(PortManifestPortCollection collection, ZString portCode, bool isEnabled = true)
		{
			var port = collection.AddNew();

			port.Port = portCode;
			port.PrincipalPK = ZGuid.NewZGuid();
			port.SenderID = "SenderID_" + Interlocked.Increment(ref senderID).ToString();
			port.Enabled = isEnabled;

			return port;
		}

		#endregion
	}
}
