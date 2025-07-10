using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class OrderLinesWebPortalModuleTest : TestCaseWithFactory
	{
		public void TestOrderLinesWebPortalModuleUrl()
		{
			var glowPortalsUrl = "https://myglowenvironment/Glow/Portals";
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUrl);

			using (var module = new OrderLinesWebPortalModule())
			{
				var url = module.Url?.ToString();

				AssertContains(glowPortalsUrl, url);
				AssertContains("/goto/OrderLineList", url);
			}
		}

		public void TestLicenseCheckpoint()
		{
			using (var module = new OrderLinesWebPortalModule())
			{
				Assertion.AssertEquals(Env.Licence.OrderManager, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new OrderLinesWebPortalModule())
			{
				Assertion.AssertEquals(Env.Security.OrderLinesWebPortal, module.SecurityCheckpoint);
			}
		}
	}
}
