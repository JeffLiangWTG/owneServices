using System;
using System.Web;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbConsignmentController))]
	public class DtbConsignmentControllerTest : ZControllerBasherTest
	{
		public override void TestViewForm()
		{
			Assert("Actions not supported", true);
		}

		public override void TestNewForm()
		{
			TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://Service/"))
			{
				var controller = new DtbConsignmentController();
				using var form = controller.ShowNewForm();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				var accessToken = queryKeyValuePairs["sso_otp"];

				AssertNotNull("A Glow Access Token should be attached", accessToken);
				AssertEquals("https", uri.Scheme);
				AssertEquals("address", uri.Host);
				AssertEquals("/Goto/NewConsignmentGlow2", uri.AbsolutePath);
			}
		}

		public void TestNewForm_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			var controller = new DtbConsignmentController();

			using (var form = controller.ShowNewForm())
			{
				var assertMessage = "Should display error when glow url was not configured in registry.";
				AssertEquals(assertMessage,
					@"Cannot create new Consignment in a browser.
Glow portal URL has not been configured for this client. Registry: GLOW/Services/GLOW Portals Root URL.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public override void TestEditForm()
		{
			Assert("Actions not supported", true);
		}

		public override void TestDeleteForm()
		{
			Assert("Actions not supported", true);
		}

		public void TestGetForm()
		{
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			consignment.LTC_JobID = "CN00001";
			Factory.Save();

			var controller = new DtbConsignmentController();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = controller.ShowEditForm(consignment))
			{
				AssertNotNull(form);
				AssertEquals(typeof(DtbConsignmentForm), form.GetType());
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DtbConsignment;
		}

		public void TestModuleId()
		{
			AssertEquals(ModuleIDs.DtbConsignment, new DtbConsignmentController().ModuleID);
		}

		public void TestControllerId()
		{
			AssertEquals(ControllerIDs.DtbConsignment, new DtbConsignmentController().ID);
		}

		public void TestSecurityCheckpoints()
		{
			var dtbConsignment = Factory.New<DtbConsignment>();
			var controller = new DtbConsignmentController();

			AssertEquals(Env.Security.DtbConsignmentDelete, controller.GetCheckPointForDelete(dtbConsignment));
			AssertEquals(Env.Security.DtbConsignmentEdit, controller.GetCheckPointForEdit(dtbConsignment));
			AssertEquals(Env.Security.None, controller.GetCheckPointForNew(dtbConsignment));
			AssertEquals(Env.Security.DtbConsignmentView, controller.GetCheckPointForView(dtbConsignment));
		}

		public void TestNewForm_WhenEnableLandTransportIsNotSet()
		{
			TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var controller = new DtbConsignmentController();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using var form = controller.ShowNewForm();
			AssertNull(form);
			AssertEquals("Land Transport is not enabled in your system, please request access by raising a CR9 incident.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
