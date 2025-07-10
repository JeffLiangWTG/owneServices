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
	[TestedType(typeof(CYDReleaseAdviceForm))]
	public class CYDReleaseAdviceFormTest : ZFormBasherTest
	{
		#region ZFormBasherTest

		[RequiresSTA]
		public void TestFormCaption()
		{
			using (var form = (CYDReleaseAdviceForm)GetFormToBash())
			{
				AssertEquals("Release Order", form.FormCaption);

				((CYDReleaseAdvice)form.BusinessEntity).YRE_JobNumber = "RE001";
				AssertEquals("Release Order RE001", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var releaseAdvice = Factory.New<CYDReleaseAdvice>();

			var form = new CYDReleaseAdviceForm(releaseAdvice);
			form.ControllerID = ControllerIDs.CYDReleaseAdvice;

			return form;
		}

		#endregion

		#region Open in Browser

		public void TestGivenGlowRegistryIsEmpty_WhenOpeningInBrowser_ThenShouldReturnError()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = (CYDReleaseAdviceForm)GetFormToBashCore())
			{
				form.Show();

				InvokeClick(form.GlowLinkLabel);
				AssertEquals("Expected an error returned when GLOW portals URI is empty.", "This Release Order cannot be opened in a browser as GLOW has not been configured for this client.\r\nRegistry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var releaseAdvice = Factory.New<CYDReleaseAdvice>();

			using (var form = new CYDReleaseAdviceForm(releaseAdvice))
			{
				form.Show();

				InvokeClick(form.GlowLinkLabel);

				AssertEquals("Expected no error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertOpenedOnTheWeb(releaseAdvice, glowPortalsUri: "address");
		}

		public void TestGlowLinkCaption()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var releaseAdvice = Factory.New<CYDReleaseAdvice>();

			using (var form = new CYDReleaseAdviceForm(releaseAdvice))
			{
				form.Show();

				AssertEquals("Release orders can only be accessed via the Container Yard Desktop Portal.", form.GlowLinkLabel.CaptionResourceString.Caption);
			}
		}

		static void AssertOpenedOnTheWeb(CYDReleaseAdvice releaseAdvice, string glowPortalsUri)
		{
			var launchedURL = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedURL, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("Expected URI to go to this GLOW alias URL", "/Goto/CYDReleaseAdvice", uri.AbsolutePath);
			AssertEquals("Expected URL to include the PK of releaseAdvice so the GLOW edit form for release advice is opened", releaseAdvice.PK.ToString(), entityPK);
		}

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		#endregion

		#region TestAddPlugIns

		public void TestAddPlugIns()
		{
			using (var form = (CYDReleaseAdviceForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
			}
		}

		#endregion
	}
}
