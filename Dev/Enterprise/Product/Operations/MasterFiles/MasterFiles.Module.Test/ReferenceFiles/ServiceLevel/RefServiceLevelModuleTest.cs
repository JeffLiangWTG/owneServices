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
	[TestedType(typeof(ServiceLevelModule))]
	sealed class RefServiceLevelModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ServiceLevel;
		}

		public void TestCheckpoints()
		{
			using (ServiceLevelModule module = new ServiceLevelModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.ServiceLevels, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (ServiceLevelModuleForTest module = new ServiceLevelModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is ServiceLevelFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (ServiceLevelModuleForTest module = new ServiceLevelModuleForTest())
			{
				IBusinessObjectCollection serviceLevelsCollection = module.NewGridCollection;
				Assert("Invalid type", serviceLevelsCollection is RefServiceLevelCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (ServiceLevelModuleForTest module = new ServiceLevelModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is ServiceLevelFilterBusinessObject);
			}
		}

		#endregion
	}
}
