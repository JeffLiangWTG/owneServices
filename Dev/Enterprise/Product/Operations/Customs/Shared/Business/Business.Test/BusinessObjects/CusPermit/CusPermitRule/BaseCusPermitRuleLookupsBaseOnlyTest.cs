using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusPermitRuleLookupsBaseOnlyTest : BaseCusPermitRuleLookupsTest<BaseCusPermitRuleLookups, BaseCusPermitRule>
	{
		public void TestPermitTypes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var permitHeader = Factory.New<BaseCusPermitHeader>();
				AssertEquals("Enterprise.Customs.ZA.Business.PermitRuleCodeList", permitHeader.CusPermitRules.AddNew().Lookups.PermitRuleCodes.GetType().ToString());
			}
		}
	}
}
