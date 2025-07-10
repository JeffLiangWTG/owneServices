using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(ExportClassificationModule))]
	public abstract class ExportClassificationModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ExportClassification;
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.ExportClassification, testExportClassificationModule.ID);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ExportClassification, testExportClassificationModule.SecurityCheckpoint);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(ExpectedLicenceCheckpoint, testExportClassificationModule.LicenceCheckPoint);
		}

		protected virtual LicenceCheckpoint ExpectedLicenceCheckpoint
		{
			get
			{
				return Env.Licence.Broker;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			testExportClassificationModule = GetNewExportClassificationModule();
		}

		protected abstract ExportClassificationModule GetNewExportClassificationModule();

		protected override void TearDown()
		{
			if (testExportClassificationModule != null)
			{
				testExportClassificationModule.Dispose();
			}

			base.TearDown();
		}

		protected ExportClassificationModule testExportClassificationModule;
	}
}
