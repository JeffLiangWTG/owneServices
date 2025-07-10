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
	[TestedType(typeof(RefTimeZoneSetModule))]
	sealed class RefTimeZoneSetModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefTimeZoneSet;
		}

		public void TestCheckpoints()
		{
			using (RefTimeZoneSetModule module = new RefTimeZoneSetModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.TimeZoneSet, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefTimeZoneSetModuleForTest module = new RefTimeZoneSetModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefTimeZoneSetFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefTimeZoneSetModuleForTest module = new RefTimeZoneSetModuleForTest())
			{
				IBusinessObjectCollection timeZonesCollection = module.NewGridCollection;
				Assert("Invalid type", timeZonesCollection is RefTimeZoneSetCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefTimeZoneSetModuleForTest module = new RefTimeZoneSetModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefTimeZoneSetFilterBusinessObject);
			}
		}

		#endregion
	}
}
