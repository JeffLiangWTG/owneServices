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
	[TestedType(typeof(RefCountryStatesModule))]
	sealed class RefCountryStatesModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefCountryStates;
		}

		public void TestCheckpoints()
		{
			using (RefCountryStatesModule module = new RefCountryStatesModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.States, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefCountryStatesModuleForTest module = new RefCountryStatesModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefCountryStatesFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefCountryStatesModuleForTest module = new RefCountryStatesModuleForTest())
			{
				IBusinessObjectCollection statesCollection = module.NewGridCollection;
				Assert("Invalid type", statesCollection is RefCountryStatesCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefCountryStatesModuleForTest module = new RefCountryStatesModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefCountryStatesFilterBusinessObject);
			}
		}

		#endregion
	}
}
