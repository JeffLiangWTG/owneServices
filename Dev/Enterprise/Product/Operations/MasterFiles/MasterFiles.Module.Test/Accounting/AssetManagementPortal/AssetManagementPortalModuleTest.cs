using System;
using System.Web;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AssetManagementPortalModuleTest : TransactionedTestCase
	{
		public void TestModule()
		{
			using (var module = new AssetManagementPortalModule())
			{
				AssertEquals(ModuleIDs.AssetManagementPortal, module.ID);
				AssertEquals(Env.Security.AssetManagementPortal, module.SecurityCheckpoint);
				AssertEquals(Env.Licence.Accountant, module.LicenceCheckPoint);
			}
		}

		public void TestModule_Show_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var module = new AssetManagementPortalModule())
			{
				AssertNull(module.Url);
				module.Show();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				Assert(lastMessage.WasError);
				AssertEquals(@"This module cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", lastMessage.Text);
			}
		}

		public void TestModule_Show()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var module = new AssetManagementPortalModule())
			{
				module.Show();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				var accessToken = queryKeyValuePairs["sso_otp"];

				AssertNotNull("A Glow Access Token should be attached", accessToken);
				AssertEquals("https", uri.Scheme);
				AssertEquals("address", uri.Host);
				AssertEquals("/AST", uri.AbsolutePath);
			}
		}
	}
}
