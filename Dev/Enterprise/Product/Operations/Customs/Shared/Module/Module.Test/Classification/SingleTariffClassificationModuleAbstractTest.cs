using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class SingleTariffClassificationModuleAbstractTest<T> : ZModuleBasherTest
		where T : SingleTariffClassificationModule, new()
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.SingleTariffClassification, testSingleTariffClassificationModule.ID);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.CusClassification, testSingleTariffClassificationModule.SecurityCheckpoint);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.Core, testSingleTariffClassificationModule.LicenceCheckPoint);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.SingleTariffClassification;

		protected override void SetUp()
		{
			base.SetUp();
			testSingleTariffClassificationModule = new T();
		}

		protected override void TearDown()
		{
			testSingleTariffClassificationModule?.Dispose();
			base.TearDown();
		}

		protected T testSingleTariffClassificationModule;
	}
}
