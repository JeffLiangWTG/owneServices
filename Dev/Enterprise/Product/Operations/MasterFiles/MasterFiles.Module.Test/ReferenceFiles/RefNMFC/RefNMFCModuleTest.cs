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
	[TestedType(typeof(RefNMFCModule))]
	sealed class RefNMFCModuleTest : ZModuleBasherTest
	{
		#region Standard Module Overrides

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefNMFC;
		}

		#endregion

		public void TestCheckpoints()
		{
			using (RefNMFCModule module = new RefNMFCModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.RefNMFC, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefNMFCModuleForTest module = new RefNMFCModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefNMFCFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefNMFCModuleForTest module = new RefNMFCModuleForTest())
			{
				IBusinessObjectCollection nmfcCollection = module.NewGridCollection;
				Assert("Invalid type", nmfcCollection is RefNMFCCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefNMFCModuleForTest module = new RefNMFCModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefNMFCFilterBusinessObject);
			}
		}

		#endregion
	}
}
