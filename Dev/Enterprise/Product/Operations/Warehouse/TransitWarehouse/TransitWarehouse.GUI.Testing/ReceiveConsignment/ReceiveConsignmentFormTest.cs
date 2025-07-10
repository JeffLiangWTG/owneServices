using System;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.GUI.Testing
{
	[TestedType(typeof(ReceiveConsignmentForm))]
	public class ReceiveConsignmentFormTest : ZFormBasherTest
	{
		#region TestFormCaption

		public void TestFormCaption()
		{
			using (var form = (ReceiveConsignmentForm)GetFormToBash())
			{
				AssertEquals("Receive Consignment", form.FormCaption);

				((WhsItemReceiveConsignment)form.BusinessEntity).WRC_JobID = "CN001";
				AssertEquals("Receive Consignment CN001", form.FormCaption);
			}
		}

		#endregion

		#region TestFormHasDocumentPlugIn

		public void TestFormHasDocumentPlugIn()
		{
			using (var form = (ReceiveConsignmentForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		#endregion

		#region TestMessageMenu

		public void TestMessageMenu()
		{
			using (var form = (ReceiveConsignmentForm)GetFormToBash())
			{
				var messageMenu = form.Menu.MenuItems.FindByText("Message");
				AssertNull("Form should not have message menu", messageMenu);
			}
		}

		public void TestMessageMenu_RegistryIsEnabled()
		{
			using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = (ReceiveConsignmentForm)GetFormToBash())
			{
				var messageMenu = form.Menu.MenuItems.FindByText("Message");
				messageMenu.OnPopup(EventArgs.Empty);
				AssertNotNull("Form should have message menu", messageMenu);
				AssertEquals("Message menu should have 3 menu items", messageMenu.MenuItems.Count, 3);

				var cinMenuInItem = messageMenu.MenuItems.FindByText("CIN 750 In Notification");
				AssertNotNull("Message menu should contain CIN 750 In Notification menu item", cinMenuInItem);
				Assert("CIN 750 In Notification menu item should be visible", cinMenuInItem.Visible);

				var cinMenuCorItem = messageMenu.MenuItems.FindByText("CIN 750 Cor Notification");
				AssertNotNull("Message menu should contain CIN 750 Cor Notification menu item", cinMenuCorItem);
				Assert("CIN 750 Cor Notification menu item should be visible", cinMenuCorItem.Visible);

				var noMessagesAvailableMenuItem = messageMenu.MenuItems.FindByText("No Messages Available");
				AssertNotNull("Message menu should contain No Messages Available menu item", noMessagesAvailableMenuItem);
				Assert("No Messages Available menu item should be invisible", !noMessagesAvailableMenuItem.Visible);
			}
		}

		#endregion

		#region Open in Browser

		public void TestOpenInBrowser_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = (ReceiveConsignmentForm)GetFormToBashCore())
			{
				form.Show();

				InvokeClick(form.glowLinkLabel);
				AssertEquals(@"This Receive Consignment cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var consignment = Factory.New<WhsItemReceiveConsignment>();

			using (var form = new ReceiveConsignmentForm(consignment))
			{
				form.Show();

				InvokeClick(form.glowLinkLabel);

				AssertEquals("Should not show error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertOpenedOnTheWeb(consignment, glowPortalsUri: "address");
		}

		public static void AssertOpenedOnTheWeb(WhsItemReceiveConsignment consignment, string glowPortalsUri)
		{
			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedUrl, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("/Goto/ReceiveConsignment", uri.AbsolutePath);
			AssertEquals(consignment.PK.ToString(), entityPK);
		}
		#endregion

		#region Implementation

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		protected override Form GetFormToBashCore()
		{
			var form = new ReceiveConsignmentForm(Factory.New<WhsItemReceiveConsignment>());
			form.ControllerID = ControllerIDs.WhsTransitReceiveConsignment;
			return form;
		}

		#endregion
	}
}
