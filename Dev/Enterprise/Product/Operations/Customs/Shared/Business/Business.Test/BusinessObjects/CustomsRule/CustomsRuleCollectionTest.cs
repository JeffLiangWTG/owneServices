using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CustomsRuleCollection))]
	public class CustomsRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<CustomsRuleCollection>
	{
		public void TestCreateRelationshipFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var customsRule1 = Factory.NewWithValidTestData<CustomsRule>();
				var guarantee = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
				var authorisation = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				var permit = Factory.NewWithValidTestData<BaseCusPermitHeader>();
				Factory.Save();

				var company = Factory.New<GlbCompany>();
				company.GC_Code = "TST";
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				var customsRule2 = Factory.NewWithValidTestData<CustomsRule>();
				customsRule2.CPH_GC_Company = company.PK;
				customsRule2.CPH_RN_NKCountryCode = company.GC_RN_NKCountryCode;
				Factory.Save();

				var filter = new CustomsRuleCollection(Factory).CompleteFilter;
				AssertEquals(true, customsRule1.MatchesFilter(filter));
				AssertEquals(false, customsRule2.MatchesFilter(filter));
				AssertEquals(false, guarantee.MatchesFilter(filter));
				AssertEquals(false, authorisation.MatchesFilter(filter));
				AssertEquals(false, permit.MatchesFilter(filter));
			}
		}
	}
}
