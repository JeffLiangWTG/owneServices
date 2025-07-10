using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class OrdersWebPortalModuleTest : TestCaseWithFactory
	{
		public void TestOrdersWebPortalModuleUrl()
		{
			var glowPortalsUrl = "https://myglowenvironment/Glow/Portals";
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUrl);

			using (var module = new OrdersWebPortalModule())
			{
				var url = module.Url?.ToString();

				AssertContains(glowPortalsUrl, url);
				AssertContains("/goto/OrderList", url);
			}
		}

		public void TestLicenseCheckpoint()
		{
			using (var module = new OrdersWebPortalModule())
			{
				Assertion.AssertEquals(Env.Licence.OrderManager, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new OrdersWebPortalModule())
			{
				Assertion.AssertEquals(Env.Security.OrdersWebPortal, module.SecurityCheckpoint);
			}
		}
	}
}
