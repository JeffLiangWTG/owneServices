using System;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.GUI.Test
{
	[TestedType(typeof(GteGateMovementForm))]
	public class GteGateMovementFormTest : ZFormBasherTest
	{
		public void TestOpenInBrowser()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			{
				var booking = Factory.NewWithValidTestData<GteGateMovement>();

				using (var form = new GteGateMovementForm(booking))
				{
					form.Show();

					InvokeClick(form.GlowLinkLabel);

					AssertEquals("Expected no error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}

				AssertOpenedOnTheWeb(booking, glowPortalsUri: "address");
			}
		}

		static void AssertOpenedOnTheWeb(GteGateMovement gateMovement, string glowPortalsUri)
		{
			var launchedURL = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedURL, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("Expected URI to go to this GLOW alias URL", "/Goto/GteVehicleMovement", uri.AbsolutePath);
			AssertEquals("Expected URL to include the PK of vehicle movement so the GLOW edit form for vehicle movement is opened", gateMovement.VehicleMovement.PK.ToString(), entityPK);
		}

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var booking = Factory.New<GteGateMovement>();
			return new GteGateMovementForm(booking);
		}

		#endregion
	}
}
