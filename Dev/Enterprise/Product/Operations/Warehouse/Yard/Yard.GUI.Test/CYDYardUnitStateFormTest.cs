using System;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.GUI.Test
{
	[TestedType(typeof(CYDYardUnitStateForm))]
	public class CYDYardUnitStateFormTest : ZFormBasherTest
	{
		#region ZFormBasherTest

		public void TestFormCaption()
		{
			using (var form = (CYDYardUnitStateForm)GetFormToBash())
			{
				AssertEquals("Yard Unit", form.FormCaption);

				((CYDYardUnitState)form.BusinessEntity).YUS_UnitID = "RE001";
				AssertEquals("Yard Unit RE001", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var yardUnitState = Factory.New<CYDYardUnitState>();
			var form = new CYDYardUnitStateForm(yardUnitState);
			form.ControllerID = ControllerIDs.CYDYardUnitState;
			return form;
		}

		#endregion

		#region Open in Browser

		public void TestGivenGlowRegistryIsEmpty_WhenOpeningInBrowser_ThenShouldReturnError()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = (CYDYardUnitStateForm)GetFormToBashCore())
			{
				form.Show();

				InvokeClick(form.GlowLinkLabel);
				AssertEquals("Expected an error returned when GLOW portals URI is empty.", "This Yard Unit cannot be opened in a browser as GLOW has not been configured for this client.\r\nRegistry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var yardUnitState = Factory.New<CYDYardUnitState>();

			using (var form = new CYDYardUnitStateForm(yardUnitState))
			{
				form.Show();

				InvokeClick(form.GlowLinkLabel);

				AssertEquals("Expected no error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertOpenedOnTheWeb(yardUnitState, glowPortalsUri: "address");
		}

		public void TestGlowLinkCaption()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var yardUnitState = Factory.New<CYDYardUnitState>();

			using (var form = new CYDYardUnitStateForm(yardUnitState))
			{
				form.Show();

				AssertEquals("Yard units can only be accessed via the Container Yard Desktop Portal.", form.GlowLinkLabel.CaptionResourceString.Caption);
			}
		}

		static void AssertOpenedOnTheWeb(CYDYardUnitState yardUnitState, string glowPortalsUri)
		{
			var launchedURL = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedURL, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("Expected URI to go to this GLOW alias URL", "/Goto/CYDYardUnitState", uri.AbsolutePath);
			AssertEquals("Expected URL to include the PK of yardUnitState so the GLOW edit form for yard unit state is opened", yardUnitState.PK.ToString(), entityPK);
		}

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		#endregion

		public void TestDocDataPlugIns()
		{
			using (var form = (CYDYardUnitStateForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}
	}
}
