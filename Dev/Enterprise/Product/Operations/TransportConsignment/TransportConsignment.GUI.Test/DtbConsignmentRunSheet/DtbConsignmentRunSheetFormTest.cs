using System;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetForm))]
	public class DtbConsignmentRunSheetFormTest : ZFormBasherTest
	{
		#region TestControllerID

		public void TestControllerID()
		{
			using (var form = new DtbConsignmentRunSheetForm(Helper.CreateRunSheet()))
			{
				AssertEquals(ControllerIDs.DtbConsignmentRunSheet, form.ControllerID);
			}
		}

		#endregion

		#region TestWorkflowTab

		public void TestWorkflowTab()
		{
			using (var form = new DtbConsignmentRunSheetForm(Helper.CreateRunSheet()))
			{
				AssertNotNull(form.Controls["MainPanel"].Controls["MainTabControl"].Controls["WorkflowTabPage"]);
			}
		}

		#endregion

		#region PlugIns

		public void TestPlugIns()
		{
			using (var form = new DtbConsignmentRunSheetForm(Helper.CreateRunSheet()))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocumentVisualizer));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.Apportionment));
			}
		}

		#endregion

		#region TestFormCaption

		public void TestFormCaption()
		{
			using (var form = (DtbConsignmentRunSheetForm)GetFormToBash())
			{
				AssertEquals("Run Sheet", form.FormCaption.TrimEnd());

				((DtbConsignmentRunSheet)form.BusinessEntity).KG_RunSheetNumber = "RS001";
				AssertEquals("Run Sheet RS001", form.FormCaption);
			}
		}

		#endregion

		#region TestInstructionsGridIsEditable

		public void TestInstructionsGridIsEditable()
		{
			using (var form = (DtbConsignmentRunSheetFormForTest)GetFormToBash())
			{
				AssertEquals(false, form.InstructionsGrid.ReadOnly);
				AssertEquals(false, form.InstructionsGrid.IsWholeRowSelectedOnClick);
			}
		}

		#endregion

		#region TestOpenInBrowser

		public void TestOpenInBrowser_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = (DtbConsignmentRunSheetForm)GetFormToBash())
			{
				form.Show();

				InvokeClick(form.GlowLinkLabel);

				var assertMessage = "Should display error when glow url was not configured in registry.";
				AssertEquals(assertMessage,
					@"This Run Sheet cannot be opened in a browser.
Glow portal URL has not been configured for this client. Registry: GLOW/Services/GLOW Portals Root URL.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenInGlowPortal()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://Service/"))
			{
				var runSheet = Factory.New<DtbConsignmentRunSheet>();

				using (var form = new DtbConsignmentRunSheetForm(runSheet))
				{
					form.Show();

					InvokeClick(form.GlowLinkLabel);

					AssertEquals("Should not show error if registry item exists.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}

				AssertOpenedOnTheWeb(runSheet, glowPortalsUri: "address");
			}
		}

		public static void AssertOpenedOnTheWeb(DtbConsignmentRunSheet runSheet, string glowPortalsUri)
		{
			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedUrl, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("/Goto/RunSheetGlow2", uri.AbsolutePath);
			AssertEquals(runSheet.PK.ToString(), entityPK);
		}

		#endregion

		void InvokeClick(ZLinkLabel label) => label.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, label, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			return new DtbConsignmentRunSheetFormForTest(runSheet);
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
