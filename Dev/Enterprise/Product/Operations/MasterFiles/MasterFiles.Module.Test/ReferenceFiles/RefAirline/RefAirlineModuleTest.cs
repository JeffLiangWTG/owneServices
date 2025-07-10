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
	[TestedType(typeof(RefAirlineModule))]
	sealed class RefAirlineModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefAirline;
		}

		public void TestCheckpoints()
		{
			using (RefAirlineModule module = new RefAirlineModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.RefAirline, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefAirlineModuleForTest module = new RefAirlineModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefAirlineFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefAirlineModuleForTest module = new RefAirlineModuleForTest())
			{
				IBusinessObjectCollection airlineCollection = module.NewGridCollection;
				Assert("Invalid type", airlineCollection is RefAirlineCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefAirlineModuleForTest module = new RefAirlineModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefAirlineFilterBusinessObject);
			}
		}

		#endregion

	}
}
