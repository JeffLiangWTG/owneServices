using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusPermitModule))]
	public class CusPermitModuleTest : ZModuleBasherTest
	{
		public void TestLicenceAndSecurityCheckPoint()
		{
			using (CusPermitModule module = new CusPermitModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.Permits, module.SecurityCheckpoint);
			}
		}

		public void TestStatementModuleAllows()
		{
			using (CusPermitModule module = new CusPermitModule())
			{
				AssertEquals("module.AllowNew", true, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", true, module.AllowDelete);
				AssertEquals("module.AllowView", true, module.AllowView);
				AssertEquals("module.AllowUniversalCopy", false, module.AllowUniversalCopy);
			}
		}

		[RequiresSTA]
		public override void TestModuleShowsAndCanSearch()
		{
			Factory.NewWithValidTestData<BaseCusPermitHeader>();
			Factory.Save();
			base.TestModuleShowsAndCanSearch();
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.Permits;

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;

		protected override bool HasController() => true;
	}
}
