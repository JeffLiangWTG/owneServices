using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class ImportClassificationModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ImportClassification;
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.ImportClassification, testImportClassificationModule.ID);
		}

		public virtual void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ImportClassification, testImportClassificationModule.SecurityCheckpoint);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(ExpectedLicenceCheckpoint, testImportClassificationModule.LicenceCheckPoint);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testImportClassificationModule = GetNewImportClassificationModule();
		}

		protected abstract ImportClassificationModule GetNewImportClassificationModule();

		protected override void TearDown()
		{
			if (testImportClassificationModule != null)
			{
				testImportClassificationModule.Dispose();
			}

			base.TearDown();
		}

		protected virtual LicenceCheckpoint ExpectedLicenceCheckpoint
		{
			get
			{
				return Env.Licence.Broker;
			}
		}

		protected ImportClassificationModule testImportClassificationModule;
	}
}
