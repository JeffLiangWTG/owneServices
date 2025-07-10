using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
{
	[TestedType(typeof(NZCConcessionModule))]
	sealed class NZCConcessionModuleTest : ZModuleBasherTest
	{
		public void TestConcessionModuleAllowsNoModifications()
		{
			using (NZCConcessionModule module = new NZCConcessionModule())
			{
				AssertEquals("module.AllowNew", false, module.AllowNew);
				AssertEquals("module.AllowEdit", false, module.AllowEdit);
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.NewZealand;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.NZ.Concession;
		}
	}
}
