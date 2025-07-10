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
	[TestedType(typeof(CYDTransportationUnitForm))]
	public class CYDTransportationUnitFormTest : ZFormBasherTest
	{
		#region ZFormBasherTest

		[RequiresSTA]
		public void TestFormCaption()
		{
			using (var form = (CYDTransportationUnitForm)GetFormToBash())
			{
				AssertEquals("Transportation Unit", form.FormCaption);

				((CYDTransportationUnit)form.BusinessEntity).YTU_TransportationUnitID = "TPU00000001";
				AssertEquals("Transportation Unit TPU00000001", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var transportationUnit = Factory.New<CYDTransportationUnit>();

			var form = new CYDTransportationUnitForm(transportationUnit);
			form.ControllerID = ControllerIDs.CYDTransportationUnit;

			return form;
		}

		#endregion

		#region Open in Browser

		public void TestGivenGlowRegistryIsEmpty_WhenOpeningInBrowser_ThenShouldReturnError()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = (CYDTransportationUnitForm)GetFormToBashCore())
			{
				form.Show();

				InvokeClick(form.GlowLinkLabel);
				AssertEquals("Expected an error returned when GLOW portals URI is empty.", "This Transportation Unit cannot be opened in a browser as GLOW has not been configured for this client.\r\nRegistry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var transportationUnit = Factory.New<CYDTransportationUnit>();

			using (var form = new CYDTransportationUnitForm(transportationUnit))
			{
				form.Show();

				InvokeClick(form.GlowLinkLabel);

				AssertEquals("Expected no error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertOpenedOnTheWeb(transportationUnit, glowPortalsUri: "address");
		}

		[RequiresSTA]
		public void TestBillingPlugIns()
		{
			using (var form = (CYDTransportationUnitForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
			}
		}

		public void TestGlowLinkCaption()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var transportationUnit = Factory.New<CYDTransportationUnit>();

			using (var form = new CYDTransportationUnitForm(transportationUnit))
			{
				form.Show();

				AssertEquals("Transportation units can only be accessed via the Container Yard Desktop Portal.", form.GlowLinkLabel.CaptionResourceString.Caption);
			}
		}

		static void AssertOpenedOnTheWeb(CYDTransportationUnit transportationUnit, string glowPortalsUri)
		{
			var launchedURL = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedURL, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("Expected URI to go to this GLOW alias URL", "/Goto/CYDTransportationUnit", uri.AbsolutePath);
			AssertEquals("Expected URL to include the PK of transportationUnit so the GLOW edit form for transportation unit is opened", transportationUnit.PK.ToString(), entityPK);
		}

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		#endregion
	}
}
