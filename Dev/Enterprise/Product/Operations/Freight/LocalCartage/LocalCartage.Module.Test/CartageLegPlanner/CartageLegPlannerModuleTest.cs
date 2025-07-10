using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageLegPlannerModuleForTest))]
	public class CartageLegPlannerModuleTest : ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.CartageLegPlanner, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.LocalTransport, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.LocalTransportLegPlanner, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(CartageLegPlannerController), Module.GetNewController().GetType());
		}

		class CartageLegPlannerModuleForTest : CartageLegPlannerModule
		{
			public new ZPopupController GetNewController()
			{
				return base.GetNewController();
			}
		}

		CartageLegPlannerModuleForTest Module
		{
			get
			{
				return module ?? (module = new CartageLegPlannerModuleForTest());
			}
		}

		CartageLegPlannerModuleForTest module;
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CartageLegPlanner;
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
