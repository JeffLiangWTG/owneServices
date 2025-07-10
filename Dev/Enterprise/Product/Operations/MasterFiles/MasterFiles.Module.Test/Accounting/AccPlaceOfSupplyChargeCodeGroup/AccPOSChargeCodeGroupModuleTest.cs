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
	[TestedType(typeof(AccPOSChargeCodeGroupModule))]
	sealed class AccPOSChargeCodeGroupModuleTest : ZModuleBasherTest
	{
		public AccPOSChargeCodeGroupModuleTest() : base()
		{
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.AccPlaceOfSupplyChargeCodeGroup;

		public void TestCheckpoints()
		{
			using (AccPOSChargeCodeGroupModule module = new AccPOSChargeCodeGroupModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.POSChargeCodeGroups, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (AccPOSChargeCodeGroupModuleForTest module = new AccPOSChargeCodeGroupModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AccPOSChargeCodeGroupFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (AccPOSChargeCodeGroupModuleForTest module = new AccPOSChargeCodeGroupModuleForTest())
			{
				IBusinessObjectCollection chargeCodeCollection = module.NewGridCollection;
				Assert("Invalid type", chargeCodeCollection is AccPOSChargeCodeGroupCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (AccPOSChargeCodeGroupModuleForTest module = new AccPOSChargeCodeGroupModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AccPOSChargeCodeGroupFilterBusinessObject);
			}
		}

		#endregion
	}
}
