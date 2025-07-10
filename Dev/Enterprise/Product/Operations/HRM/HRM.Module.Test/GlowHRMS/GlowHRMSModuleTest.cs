using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.HRM.Module.Test
{
	public class GlowHRMSModuleTest : TestCaseWithFactory
	{
		public void TestGlowModuleUrl()
		{
			var glowPortalsUrl = "https://myglowenvironment/Glow/Portals";
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUrl);

			using (var module = new GlowHRMSModule())
			{
				var url = module.Url?.ToString();

				CombineAssertions(() =>
				{
					AssertContains(glowPortalsUrl, url);
					AssertContains("/HRM", url);
				});
			}
		}

		public void TestGlowModuleUrl_PortalRegistryEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			using (var module = new GlowHRMSModule())
			{
				AssertNull(module.Url);

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions("Should show an error in the UI if GLOW is not configured", () =>
				{
					Assert(lastMessage.WasError);
					AssertContains("This module cannot be opened in a browser as GLOW has not been configured for this client.", lastMessage.Text);
				});
			}
		}
	}
}
