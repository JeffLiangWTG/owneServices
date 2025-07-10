using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCountryRulesCollection))]
	sealed class RefCountryRulesCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RefCountryRulesCollection(Factory);
		}

		public void TestLoadCountry()
		{
			RefCountry country1;
			RefCountry country2;
			RefCountryRules rule1;
			RefCountryRules rule2;
			RefCountryRules rule3;

			country1 = Factory.New<RefCountry>();
			country1.Code = "UA";

			rule1 = Factory.New<RefCountryRules>();
			rule1.R7_RN_NKDestination = country1.Code;
			rule1.R7_RN_NKOrigin = "AA";

			rule2 = Factory.New<RefCountryRules>();
			rule2.R7_RN_NKDestination = "BB";
			rule2.R7_RN_NKOrigin = country1.Code;

			rule3 = Factory.New<RefCountryRules>();
			rule3.R7_RN_NKDestination = "AA";
			rule3.R7_RN_NKOrigin = "BB";

			country2 = Factory.New<RefCountry>();
			country2.Code = country1.Code;

			AssertCollectionContains(rule1, country2.Rules);
			AssertCollectionContains(rule2, country2.Rules);
			AssertCollectionNotContains(rule3, country2.Rules);
		}

		public void TestCountryCode()
		{
			var collection = new RefCountryRulesCollection(Factory);

			AssertEquals("", collection.CountryCode);

			var country = Factory.New<RefCountry>();
			country.Code = "UA";
			collection.LoadCountry(country);

			AssertEquals("UA", collection.CountryCode);

			var newRule = collection.AddNew();
			AssertEquals("UA", newRule.CurrentCountryCode);
		}
	}
}
