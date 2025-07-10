using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ConsolDashboardModule))]
	public class ConsolDashboardModuleTest : ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.ConsolPlanningBoard, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ConsolPlanningBoard, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(ConsolDashboardController), Module.GetNewController().GetType());
		}

		#region Test Classes

		class TestConsolDashboardModule : ConsolDashboardModule
		{
			public new ZPopupController GetNewController()
			{
				return base.GetNewController();
			}
		}

		#endregion

		#region Implementation

		TestConsolDashboardModule Module
		{
			get { return module ?? (module = new TestConsolDashboardModule()); }
		}
		TestConsolDashboardModule module;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ConsolPlanningBoard;
		}

		protected override void TearDown()
		{
			base.TearDown();

			module?.Dispose();
		}

		#endregion
	}
}
