using System;
using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class ProductWarehousePortalModuleTest : TestCaseWithFactory
	{
		public void TestModule()
		{
			using (var module = new ProductWarehousePortalModule())
			{
				AssertEquals(ModuleIDs.WhsProductWarehousePortal, module.ID);
				AssertEquals(Env.Security.None, module.SecurityCheckpoint);
				AssertEquals(Env.Licence.AlwaysAllow, module.LicenceCheckPoint);
			}
		}

		public void TestModule_Show_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			using (var module = new ProductWarehousePortalModule())
			{
				module.Show();
				AssertEquals(@"This module cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestModule_Show()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var module = new ProductWarehousePortalModule())
			{
				module.Show();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				var accessToken = queryKeyValuePairs["sso_otp"];

				AssertNotNull("A Glow Access Token should be attached", accessToken);
				AssertEquals("https", uri.Scheme);
				AssertEquals("address", uri.Host);
				AssertEquals("/WCC", uri.AbsolutePath);
			}
		}
	}
}
