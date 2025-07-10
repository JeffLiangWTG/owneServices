using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module
{
	[TestedType(typeof(NZTariffBulkChangeModule))]
	sealed class NZTariffBulkChangeModuleTest : ZArchitecture.Modules.Testing.ZPopupModuleBasherTest
	{
		public void TestGetNewController()
		{
			AssertEquals(typeof(NZTariffBulkChangeController), module.GetNewController().GetType());
		}

		public void TestShow()
		{
			using (IZForm form = new NZTariffBulkChangeController().ShowNewForm())
			{
				AssertNotNull(form);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = new NZTariffBulkChangeModule_ForTest();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.TariffBulkChange;
		}

		protected override void TearDown()
		{
			if (module != null)
			{
				module.Dispose();
			}

			base.TearDown();
		}

		NZTariffBulkChangeModule_ForTest module;
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.TariffBulkChange, module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.TariffBulkChange, module.SecurityCheckpoint);
		}

		class NZTariffBulkChangeModule_ForTest : NZTariffBulkChangeModule
		{
			public new ZPopupController GetNewController()
			{
				return base.GetNewController();
			}
		}
	}
}
