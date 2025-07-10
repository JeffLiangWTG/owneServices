using System;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(CO2eDashboardModule))]
	class CO2eDashboardModuleTest : TransactionedTestCase
	{
		public void TestModule()
		{
			using (var module = new CO2eDashboardModule())
			{
				AssertEquals(ModuleIDs.CO2eDashboard, module.ID);
				AssertEquals(Env.Security.None, module.SecurityCheckpoint);
				AssertEquals(Env.Licence.AlwaysAllow, module.LicenceCheckPoint);
			}
		}

		public void TestModule_Show_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var module = new CO2eDashboardModule())
			{
				module.Show();
				AssertEquals(@"This module cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestModule_Show()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var module = new CO2eDashboardModule())
			{
				module.Show();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				AssertEquals("https", uri.Scheme);
				AssertEquals("address", uri.Host);
				AssertEquals("/GHG", uri.AbsolutePath);
			}
		}
	}
}
