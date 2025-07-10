using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.TransportConsignment.Module.Testing
{
	public class DtbConsignmentWebPortalModuleTest : TestCaseWithFactory
	{
		public void TestWebPortalModuleUrl()
		{
			var glowPortalsUrl = "https://myglowenvironment/Glow/Portals";
			GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUrl);

			using (var module = new DtbConsignmentWebPortalModule())
			{
				var url = module.Url?.ToString();

				AssertContains(glowPortalsUrl, url);
				AssertContains("/goto/ConsignmentList", url);
			}
		}

		public void TestLicenseCheckpoint()
		{
			using (var module = new DtbConsignmentWebPortalModule())
			{
				AssertEquals(Env.Licence.LandTransport, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new DtbConsignmentWebPortalModule())
			{
				AssertEquals(Env.Security.DtbConsignmentWebPortal, module.SecurityCheckpoint);
			}
		}
	}
}
