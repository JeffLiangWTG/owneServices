using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccWithholdingModule))]
	sealed class AccWithholdingModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccWithholding;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		public void TestCheckpoints()
		{
			using (AccWithholdingModule module = new AccWithholdingModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.WHTTaxRates, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (AccWithholdingModuleForTest module = new AccWithholdingModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AccWithholdingFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (AccWithholdingModuleForTest module = new AccWithholdingModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is AccWithholdingCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (AccWithholdingModuleForTest module = new AccWithholdingModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is AccWithholdingFilterBusinessObject);
			}
		}

		#endregion
	}
}
