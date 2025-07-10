using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class GateBookingModuleTest : TestCaseWithFactory
	{
		public void TestGateBookingModuleUrl()
		{
			var glowPortalsUrl = "https://myglowenvironment/Glow/Portals";
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUrl);

			using (var gateBookingModule = new GateBookingModule())
			{
				var url = gateBookingModule.Url?.ToString();

				AssertContains(glowPortalsUrl, url);
				AssertContains("/Goto/GateBooking", url);
			}
		}

		public void TestGateBookingModuleUrl_PortalRegistryEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			using (var gateBookingModule = new GateBookingModule())
			{
				AssertNull(gateBookingModule.Url);

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions("Should show an error in the UI if GLOW is not configured", () =>
				{
					Assert(lastMessage.WasError);
					AssertEquals(@"This module cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", lastMessage.Text);
				});
			}
		}
	}
}
