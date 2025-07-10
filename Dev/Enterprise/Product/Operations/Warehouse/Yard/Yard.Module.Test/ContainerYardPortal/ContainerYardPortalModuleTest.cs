using System;
using System.Web;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	public class ContainerYardPortalModuleTest : TransactionedTestCase
	{
		public void TestModule()
		{
			using (var module = new ContainerYardPortalModule())
			{
				AssertEquals(ModuleIDs.ContainerYardPortal, module.ID);
				AssertEquals(Env.Security.None, module.SecurityCheckpoint);
				AssertEquals(Env.Licence.ContainerYard, module.LicenceCheckPoint);
			}
		}

		public void TestModule_Show_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var module = new ContainerYardPortalModule())
			{
				module.Show();
				AssertEquals(@"This module cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestModule_Show()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var module = new ContainerYardPortalModule())
			{
				module.Show();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				var accessToken = queryKeyValuePairs["sso_otp"];

				AssertNotNull("A Glow Access Token should be attached", accessToken);
				AssertEquals("https", uri.Scheme);
				AssertEquals("address", uri.Host);
				AssertEquals("/Goto/YardManagementPortal", uri.AbsolutePath);
			}
		}
	}
}
