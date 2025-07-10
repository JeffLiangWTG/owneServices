using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class GateControlModuleTest : TestCaseWithFactory
	{
		public void TestGateControlModuleUrl()
		{
			var glowPortalsUrl = "https://myglowenvironment/Glow/Portals";
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUrl);

			using (var gateControlModule = new GateControlModule())
			{
				var url = gateControlModule.Url?.ToString();

				AssertContains(glowPortalsUrl, url);
				AssertContains("/Goto/GateControl", url);
			}
		}

		public void TestGateControlModuleUrl_PortalRegistryEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			using (var gateControlModule = new GateBookingModule())
			{
				AssertNull(gateControlModule.Url);

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
