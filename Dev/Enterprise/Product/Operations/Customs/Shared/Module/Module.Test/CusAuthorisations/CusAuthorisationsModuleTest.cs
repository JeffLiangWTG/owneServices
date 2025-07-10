using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusAuthorisationsModule))]
	sealed class CusAuthorisationsModuleTest : ZModuleBasherTest
	{
		public void TestLicenceAndSecurityCheckPoint()
		{
			using (var module = new CusAuthorisationsModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.Authorisations, module.SecurityCheckpoint);
			}
		}

		public void TestStatementModuleAllows()
		{
			using (var module = new CusAuthorisationsModule())
			{
				AssertEquals("module.AllowNew", true, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", true, module.AllowDelete);
				AssertEquals("module.AllowView", true, module.AllowView);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CusAuthorisations;
		protected override string CountryCode => Core.Constants.CountryCodes.Latvia;
		public override void TestModuleShowsAndCanSearch()
		{
			Factory.NewWithValidTestData<CusAuthorisationHeader>();
			Factory.Save();
			base.TestModuleShowsAndCanSearch();
		}
	}
}
