using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageRunSheetDashboardModule))]
	public class CartageRunSheetDashboardModuleTest : ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.CartageRunSheetDashboard, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.LocalTransport, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.LocalTransportRunSheetDashboard, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(CartageRunSheetDashboardController), Module.GetNewController().GetType());
		}

		class TestCartageRunSheetDashboardModule : CartageRunSheetDashboardModule
		{
			public new CartageRunSheetDashboardController GetNewController()
			{
				return (CartageRunSheetDashboardController)base.GetNewController();
			}
		}

		TestCartageRunSheetDashboardModule Module
		{
			get
			{
				return module ?? (module = new TestCartageRunSheetDashboardModule());
			}
		}

		TestCartageRunSheetDashboardModule module;
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CartageRunSheetDashboard;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (module != null)
			{
				module.Dispose();
			}
		}
	}
}
