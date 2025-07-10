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
	[TestedType(typeof(CYDReceiveAdviceForm))]
	public class CYDReceiveAdviceFormTest : ZFormBasherTest
	{
		#region ZFormBasherTest

		public void TestFormCaption()
		{
			using (var form = (CYDReceiveAdviceForm)GetFormToBash())
			{
				AssertEquals("Pre-Arrival Instruction", form.FormCaption);

				((CYDReceiveAdvice)form.BusinessEntity).YRA_JobNumber = "RA001";
				AssertEquals("Pre-Arrival Instruction RA001", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var receiveAdvice = Factory.New<CYDReceiveAdvice>();

			var form = new CYDReceiveAdviceForm(receiveAdvice);
			form.ControllerID = ControllerIDs.CYDReceiveAdvice;

			return form;
		}

		#endregion

		#region Open in Browser

		public void TestGivenGlowRegistryIsEmpty_WhenOpeningInBrowser_ThenShouldReturnError()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = (CYDReceiveAdviceForm)GetFormToBashCore())
			{
				form.Show();

				InvokeClick(form.GlowLinkLabel);
				AssertEquals("Expected an error returned when GLOW portals URI is empty.", "This Pre-Arrival Instruction cannot be opened in a browser as GLOW has not been configured for this client.\r\nRegistry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var receiveAdvice = Factory.New<CYDReceiveAdvice>();

			using (var form = new CYDReceiveAdviceForm(receiveAdvice))
			{
				form.Show();

				InvokeClick(form.GlowLinkLabel);

				AssertEquals("Expected no error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertOpenedOnTheWeb(receiveAdvice, glowPortalsUri: "address");
		}

		public void TestBillingPlugIns()
		{
			using (var form = (CYDReceiveAdviceForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
			}
		}

		public void TestGlowLinkCaption()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var receiveAdvice = Factory.New<CYDReceiveAdvice>();

			using (var form = new CYDReceiveAdviceForm(receiveAdvice))
			{
				form.Show();

				AssertEquals("Pre-arrival instructions can only be accessed via the Container Yard Desktop Portal.", form.GlowLinkLabel.CaptionResourceString.Caption);
			}
		}

		static void AssertOpenedOnTheWeb(CYDReceiveAdvice receiveAdvice, string glowPortalsUri)
		{
			var launchedURL = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedURL, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("Expected URI to go to this GLOW alias URL", "/Goto/CYDReceiveAdvice", uri.AbsolutePath);
			AssertEquals("Expected URL to include the PK of receiveAdvice so the GLOW edit form for receive advice is opened", receiveAdvice.PK.ToString(), entityPK);
		}

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		#endregion
	}
}
