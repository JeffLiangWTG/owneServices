using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProductionRules.GUI.Testing
{
	class ProductionRulesEngineLinkManagerTest : TestCaseWithFactory
	{
		public void TestHandleProductionRulesEngineLinkClick()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				ProductionRulesEngineLinkManager.HandleProductionRulesEngineLinkClick(Notify, "goto/testAlias");
				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);

				AssertEquals("Launched uri is correct.", "https", uri.Scheme);
				AssertEquals("Launched uri is correct.", "address", uri.Host);
				AssertEquals("Launched uri is correct.", "/goto/testAlias", uri.AbsolutePath);
				AssertEquals("Launched uri is correct.", string.Empty, uri.Fragment);

				AssertEquals("No notifications.", null, Notify.LastEvent);
			}
		}

		public void TestHandleProductionRulesEngineLinkClick_ReturnErrorWhenGlowIsNotConfiguredInRegistry()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			ProductionRulesEngineLinkManager.HandleProductionRulesEngineLinkClick(Notify, "goto/testAlias");
			AssertEquals(@"This Production Rules Engine portal cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", Notify.LastEvent.Message);
		}

		protected TestNotificationBuffer Notify => notify ?? (notify = new TestNotificationBuffer());
		protected TestNotificationBuffer notify;
	}
}
