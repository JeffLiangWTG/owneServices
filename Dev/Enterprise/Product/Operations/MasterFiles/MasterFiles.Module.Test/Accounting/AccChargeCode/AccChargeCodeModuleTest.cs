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
	[TestedType(typeof(AccChargeCodeModule))]
	sealed class AccChargeCodeModuleTest : ZModuleBasherTest
	{
		public AccChargeCodeModuleTest() : base()
		{
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccChargeCode;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		public void TestCheckpoints()
		{
			using (AccChargeCodeModule module = new AccChargeCodeModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.ChargeCodes, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (AccChargeCodeModuleForTest module = new AccChargeCodeModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AccChargeCodeFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (AccChargeCodeModuleForTest module = new AccChargeCodeModuleForTest())
			{
				IBusinessObjectCollection chargeCodeCollection = module.NewGridCollection;
				Assert("Invalid type", chargeCodeCollection is AccChargeCodeCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (AccChargeCodeModuleForTest module = new AccChargeCodeModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AccChargeCodeFilterBusinessObject);
			}
		}

		#endregion
	}
}
