using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(StmFeatureTestModule))]
	sealed class StmFeatureTestModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.StmFeatureTest;
		}

		public void TestCheckpoints()
		{
			using (StmFeatureTestModule module = new StmFeatureTestModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.SystemFeatureTest, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (StmFeatureTestModuleForTest module = new StmFeatureTestModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is StmFeatureTestFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (StmFeatureTestModuleForTest module = new StmFeatureTestModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is StmFeatureTestCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (StmFeatureTestModuleForTest module = new StmFeatureTestModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is StmFeatureTestFilterBusinessObject);
			}
		}

		sealed class StmFeatureTestModuleForTest : StmFeatureTestModule
		{
			public StmFeatureTestModuleForTest()
			{
			}

			public IFilterControl NewFilterControl
			{
				get { return GetNewFilterControl(); }
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get { return GetNewGridCollection(); }
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get { return GetNewFilterBusinessObject(); }
			}
		}
	}
}
