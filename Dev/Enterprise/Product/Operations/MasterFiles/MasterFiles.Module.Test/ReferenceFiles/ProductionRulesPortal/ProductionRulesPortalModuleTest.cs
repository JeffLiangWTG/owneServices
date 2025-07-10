using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ProductionRulesPortalModuleTest : TestCaseWithFactory
	{
		public void TestProductionRulesModuleUrl()
		{
			var endpoint = "https://myglowenvironment/Glow/Portals";
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, endpoint);

			using (var productionRulesModule = new ProductionRulesPortalModule())
			{
				var url = productionRulesModule.Url?.ToString();

				AssertContains(endpoint, url);
				AssertContains("/PRE", url);
			}
		}

		public void TestProductionModuleUrl_PortalRegistryEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			using (var productionRulesModule = new ProductionRulesPortalModule())
			{
				AssertNull(productionRulesModule.Url);

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
