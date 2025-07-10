using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	public class GlowRateSelectorModuleTest : TransactionedTestCase
	{
		public void TestErrorShownIfGlowNotConfigured()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			using (var module = new GlowRateSelectorModule())
			{
				module.Show();
				AssertEquals(@"CargoWise CarrierConnect cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
