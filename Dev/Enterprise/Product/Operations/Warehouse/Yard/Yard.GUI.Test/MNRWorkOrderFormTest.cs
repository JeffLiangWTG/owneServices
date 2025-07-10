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
	[TestedType(typeof(MNRWorkOrderForm))]
	public class MNRWorkOrderFormTest : ZFormBasherTest
	{
		#region ZFormBasherTest

		protected override Form GetFormToBashCore()
		{
			var workOrderHeader = Factory.New<MNRWorkOrderHeader>();

			var form = new MNRWorkOrderForm(workOrderHeader);
			form.ControllerID = ControllerIDs.MNRWorkOrder;

			return form;
		}

		#endregion

		#region Open in Browser

		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var workOrderHeader = Factory.New<MNRWorkOrderHeader>();

			using (var form = new MNRWorkOrderForm(workOrderHeader))
			{
				form.Show();

				InvokeClick(form.GlowLinkLabel);

				AssertEquals("Expected no error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertOpenedOnTheWeb(workOrderHeader, glowPortalsUri: "address");
		}

		public void TestGlowLinkCaption()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var workOrderHeader = Factory.New<MNRWorkOrderHeader>();

			using (var form = new MNRWorkOrderForm(workOrderHeader))
			{
				form.Show();

				AssertEquals("Work order can only be accessed via the Container Yard GLOW Portal.", form.GlowLinkLabel.CaptionResourceString.Caption);
			}
		}

		static void AssertOpenedOnTheWeb(MNRWorkOrderHeader workOrderHeader, string glowPortalsUri)
		{
			var launchedURL = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedURL, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("Expected URI to go to this GLOW alias URL", "/Goto/MNRWorkOrderHeader", uri.AbsolutePath);
			AssertEquals("Expected URL to include the PK of workOrder so the GLOW edit form for receive advice is opened", workOrderHeader.PK.ToString(), entityPK);
		}

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		#endregion

		public void TestDocDataPlugIns()
		{
			using (var form = (MNRWorkOrderForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
			}
		}
	}
}
