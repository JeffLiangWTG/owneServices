using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ContractManagement.Module.Testing
{
	public class CarrierContractGlowModuleTest : TestCaseWithFactory
	{
		public void TestCarrierContractGlowModuleUrl()
		{
			var glowPortalsUrl = "https://myglowenvironment/Glow/Portals";
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUrl);

			using (var carrierContractModule = new CarrierContractGlowModule())
			{
				var url = carrierContractModule.Url?.ToString();

				AssertContains(glowPortalsUrl, url);
				AssertContains("/goto/CarrierContracts", url);
			}
		}

		public void TestCarrierContractGlowModuleUrl_PortalRegistryEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			using (var carrierContractModule = new CarrierContractGlowModule())
			{
				AssertNull(carrierContractModule.Url);

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
