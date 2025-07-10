using System;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.GUI.Testing
{
	[TestedType(typeof(DispatchTransportationUnitForm))]
	public class DispatchTransportationUnitTest : ZFormBasherTest
	{
		#region TestFormCaption

		public void TestFormCaption()
		{
			using (var form = (DispatchTransportationUnitForm)GetFormToBash())
			{
				AssertEquals("Dispatch Transportation Unit", form.FormCaption);

				((WhsItemDispatchTransportationUnit)form.BusinessEntity).WDH_VehicleReference = "CN001";
				AssertEquals("Dispatch Transportation Unit", form.FormCaption);
			}
		}

		#endregion

		#region Open in Browser

		public void TestOpenInBrowser_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = (DispatchTransportationUnitForm)GetFormToBashCore())
			{
				form.Show();

				InvokeClick(form.glowLinkLabel);
				AssertEquals(@"This Dispatch Transportation Unit cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();

			using (var form = new DispatchTransportationUnitForm(dtu))
			{
				form.Show();

				InvokeClick(form.glowLinkLabel);

				AssertEquals("Should not show error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertOpenedOnTheWeb(dtu, glowPortalsUri: "address");
		}

		public static void AssertOpenedOnTheWeb(WhsItemDispatchTransportationUnit dtu, string glowPortalsUri)
		{
			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedUrl, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("/Goto/DispatchTransportationUnit", uri.AbsolutePath);
			AssertEquals(dtu.PK.ToString(), entityPK);
		}

		#endregion

		#region PlugIns

		public void TestPlugIns()
		{
			using (var form = (DispatchTransportationUnitForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.ApportionmentForTransitTransportationUnit));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
			}
		}

		#endregion

		#region TestFormHasDocumentPlugIn

		public void TestFormHasDocumentPlugIn()
		{
			using (var form = (DispatchTransportationUnitForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		#endregion

		#region Implementation

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		protected override Form GetFormToBashCore()
		{
			var form = new DispatchTransportationUnitForm(Factory.New<WhsItemDispatchTransportationUnit>());
			form.ControllerID = ControllerIDs.WhsItemDispatchTransportationUnit;
			return form;
		}

		#endregion
	}
}
