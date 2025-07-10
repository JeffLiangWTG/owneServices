using System;
using System.Web;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetController))]
	public class DtbConsignmentRunSheetControllerTest : ZPopupControllerBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.DtbConsignmentRunSheet, new DtbConsignmentRunSheetController().ModuleID);
		}

		#endregion TestModuleID

		#region TestSecurityCheckpoints

		public void TestSecurityCheckpoints()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			var controller = new DtbConsignmentRunSheetController();

			AssertEquals(Env.Security.DtbConsignmentRunSheetDelete, controller.GetCheckPointForDelete(runSheet));
			AssertEquals(Env.Security.DtbConsignmentRunSheetEdit, controller.GetCheckPointForEdit(runSheet));
			AssertEquals(Env.Security.None, controller.GetCheckPointForNew(runSheet));
			AssertEquals(Env.Security.DtbConsignmentRunSheetView, controller.GetCheckPointForView(runSheet));
		}

		#endregion TestSecurityCheckpoints

		public override void TestNewForm()
		{
			TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://Service/"))
			{
				var controller = new DtbConsignmentRunSheetController();
				using var form = controller.ShowNewForm();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				var accessToken = queryKeyValuePairs["sso_otp"];

				AssertNotNull("A Glow Access Token should be attached", accessToken);
				AssertEquals("https", uri.Scheme);
				AssertEquals("address", uri.Host);
				AssertEquals("/Goto/NewRunSheetGlow2", uri.AbsolutePath);
			}
		}

		public void TestNewForm_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			var controller = new DtbConsignmentRunSheetController();

			using (var form = controller.ShowNewForm())
			{
				var assertMessage = "Should display error when glow url was not configured in registry.";
				AssertEquals(assertMessage,
					@"Cannot create new Run sheet in a browser.
Glow portal URL has not been configured for this client. Registry: GLOW/Services/GLOW Portals Root URL.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DtbConsignmentRunSheet;
		}

		#endregion Implementation

		public void TestNewForm_WhenEnableLandTransportIsNotSet()
		{
			TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var controller = new DtbConsignmentRunSheetController();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using var form = controller.ShowNewForm();
			AssertNull(form);
			AssertEquals("Land Transport is not enabled in your system, please request access by raising a CR9 incident.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
