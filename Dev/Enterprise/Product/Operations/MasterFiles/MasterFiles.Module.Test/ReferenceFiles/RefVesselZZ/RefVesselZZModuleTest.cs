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
	[TestedType(typeof(RefVesselZZModule))]
	class RefVesselZZModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefVesselZZ;
		}

		public void TestModuleID()
		{
			using (RefVesselZZModule module = new RefVesselZZModule())
			{
				AssertEquals("ModuleID", ModuleIDs.RefVesselZZ, module.ID);
			}
		}

		public void TestCheckpoints()
		{
			using (RefVesselZZModule module = new RefVesselZZModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.Vessels, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		protected virtual RefVesselZZModule GetNewVesselModule()
		{
			return new RefVesselZZModule();
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefVesselZZModuleForTest module = new RefVesselZZModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefVesselZZFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefVesselZZModuleForTest module = new RefVesselZZModuleForTest())
			{
				IBusinessObjectCollection vesselsCollection = module.NewGridCollection;
				Assert("Invalid type", vesselsCollection is RefVesselZZCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefVesselZZModuleForTest module = new RefVesselZZModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefVesselZZFilterBusinessObject);
			}
		}

		#endregion
	}
}
