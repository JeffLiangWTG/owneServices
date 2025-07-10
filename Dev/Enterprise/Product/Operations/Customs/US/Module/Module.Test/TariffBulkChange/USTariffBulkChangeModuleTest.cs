using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USTariffBulkChangeModule))]
	sealed class USTariffBulkChangeModuleTest : ZArchitecture.Modules.Testing.ZPopupModuleBasherTest
	{
		public void TestGetNewController()
		{
			AssertEquals(typeof(USTariffBulkChangeController), module.GetNewControllerInternal().GetType());
		}

		public void TestShow()
		{
			using (var form = new USTariffBulkChangeController().ShowNewForm())
			{
				AssertNotNull(form);
			}
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.Customs.US.USTariffBulkChange, module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.TariffBulkChange, module.SecurityCheckpoint);
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = new USTariffBulkChangeModule();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.US.USTariffBulkChange;
		}

		protected override void TearDown()
		{
			if (module != null)
			{
				module.Dispose();
			}

			base.TearDown();
		}

		USTariffBulkChangeModule module;
	}
}
