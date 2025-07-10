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
	[TestedType(typeof(GteVehicleMovementForm))]
	public class GteVehicleMovementFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestFormCaption()
		{
			using (var form = (GteVehicleMovementForm)GetFormToBashCore())
			{
				AssertEquals("Vehicle Movement", form.FormCaption);
			}
		}
		[RequiresSTA]
		public void TestOpenInBrowser()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			{
				var vehicleMovement = Factory.New<GteVehicleMovement>();

				using (var form = new GteVehicleMovementForm(vehicleMovement))
				{
					form.Show();

					InvokeClick(form.GlowLinkLabel);

					AssertEquals("Expected no error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}

				AssertOpenedOnTheWeb(vehicleMovement, glowPortalsUri: "address");
			}
		}

		static void AssertOpenedOnTheWeb(GteVehicleMovement vehicleMovement, string glowPortalsUri)
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
			AssertEquals("Expected URL to include the PK of vehicleMovement so the GLOW edit form for vehicleMovmeent is opened", vehicleMovement.PK.ToString(), entityPK);
		}

		void InvokeClick(ZLinkLabel zlinkLabel) => zlinkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, zlinkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var booking = Factory.New<GteVehicleMovement>();
			return new GteVehicleMovementForm(booking);
		}

		#endregion
	}
}
