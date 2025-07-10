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
	[TestedType(typeof(RefUNLOCOModule))]
	sealed class RefUNLOCOModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefUNLOCO;
		}

		public void TestCheckpoints()
		{
			using (RefUNLOCOModule module = new RefUNLOCOModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.UNLOCO, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefUNLOCOModuleForTest module = new RefUNLOCOModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefUNLOCOFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefUNLOCOModuleForTest module = new RefUNLOCOModuleForTest())
			{
				IBusinessObjectCollection uNLOCOsCollection = module.NewGridCollection;
				Assert("Invalid type", uNLOCOsCollection is RefUNLOCOCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefUNLOCOModuleForTest module = new RefUNLOCOModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefUNLOCOFilterBusinessObject);
			}
		}

		#endregion
	}
}
