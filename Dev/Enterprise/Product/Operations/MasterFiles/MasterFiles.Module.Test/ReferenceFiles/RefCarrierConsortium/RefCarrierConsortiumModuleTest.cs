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
	[TestedType(typeof(RefCarrierConsortiumModule))]
	sealed class RefCarrierConsortiumModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefCarrierConsortium;
		}

		public void TestCheckpoints()
		{
			using (RefCarrierConsortiumModule module = new RefCarrierConsortiumModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.VesselConsortium, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefCarrierConsortiumModuleForTest module = new RefCarrierConsortiumModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefCarrierConsortiumFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefCarrierConsortiumModuleForTest module = new RefCarrierConsortiumModuleForTest())
			{
				IBusinessObjectCollection carrierConsortiumsCollection = module.NewGridCollection;
				Assert("Invalid type", carrierConsortiumsCollection is RefCarrierConsortiumCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefCarrierConsortiumModuleForTest module = new RefCarrierConsortiumModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefCarrierConsortiumFilterBusinessObject);
			}
		}

		#endregion
	}
}
