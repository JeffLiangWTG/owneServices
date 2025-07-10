using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class EuropeanUnionCustomsMembersProviderTest : TestCaseWithFactory
	{
		public void TestGetEuropeanUnionCustomsMembersList()
		{
			var provider = new EuropeanUnionCustomsMembersProvider();
			AssertEquals(28, provider.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().Length);
		}

		public void TestGetEuropeanUnionAndCtCountries()
		{
			var provider = new EuropeanUnionCustomsMembersProvider();
			AssertEquals(29, provider.GetEuropeanUnionAndCtCountries().Length);
		}

		public void TestIsInEuropeanCustomsUnion()
		{
			var provider = new EuropeanUnionCustomsMembersProvider();

			Assert(!provider.IsInEuropeanCustomsUnion(""));
			Assert(!provider.IsInEuropeanCustomsUnion("hello"));
			Assert(!provider.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.Israel));
			Assert(!provider.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.Jamaica));
			Assert(provider.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.UnitedKingdom));
			Assert(provider.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.Hungary));
			Assert(provider.IsInEuropeanCustomsUnion(Core.Constants.CountryCodes.Croatia));

			foreach (string country in provider.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers())
			{
				Assert(provider.IsInEuropeanCustomsUnion(country));
			}
		}

		public void TestGetCountriesInEuropeanCustomsUnionOrInheritsFromEU()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDate(2019, 1, 1), new ZDate(2060, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2060, 12, 31));

			Factory.Save();

			var provider = new EuropeanUnionCustomsMembersProvider();
			var list = provider.GetCountriesInEuropeanCustomsUnionOrInheritsFromEU().ToHashSet();
			Assert("IT should be in the list", list.Contains(Core.Constants.CountryCodes.Italy));
			Assert("GB should be in the list", list.Contains(Core.Constants.CountryCodes.UnitedKingdom));
			Assert("TR should be in the list", list.Contains(Core.Constants.CountryCodes.Turkey));
			Assert("FR should not be in the list", !list.Contains(Core.Constants.CountryCodes.France));
		}

		public void TestIsInEuropeanCustomsUnionOrInheritsFromEU()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDate(2019, 1, 1), new ZDate(2060, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2060, 12, 31));

			Factory.Save();

			var provider = new EuropeanUnionCustomsMembersProvider();
			Assert(provider.IsInEuropeanCustomsUnionOrInheritsFromEU(Core.Constants.CountryCodes.Italy));
			Assert(provider.IsInEuropeanCustomsUnionOrInheritsFromEU(Core.Constants.CountryCodes.UnitedKingdom));
			Assert(provider.IsInEuropeanCustomsUnionOrInheritsFromEU(Core.Constants.CountryCodes.Turkey));
			Assert(!provider.IsInEuropeanCustomsUnionOrInheritsFromEU(Core.Constants.CountryCodes.France));
		}

		public void TestIsCountryEuOrCtCountry()
		{
			var provider = new EuropeanUnionCustomsMembersProvider();

			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			var tradeGroup2 = helper.CreateTradeGroup("EUN", "EUCTP", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.Switzerland, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			factory.Save();

			CombineAssertions(() =>
			{
				Assert("empty", !provider.IsCountryEuOrCtCountry(""));
				Assert("hallo", !provider.IsCountryEuOrCtCountry("hello"));
				Assert("Israel", !provider.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Israel));
				Assert("Jamaica", !provider.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Jamaica));
				Assert("Switzerland", provider.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Switzerland));

				foreach (var country in provider.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers())
				{
					Assert(country, provider.IsCountryEuOrCtCountry(country));
				}

				Assert("UnitedKingdom", provider.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.UnitedKingdom));
			});
		}

		public void TestIsMemberOfEU()
		{
			var provider = new EuropeanUnionCustomsMembersProvider();
			Assert(!provider.IsMemberOfEU(Core.Constants.CountryCodes.Switzerland));

			foreach (var country in Factory.GetEuropeanUnionForCustomsMembers())
			{
				Assert(provider.IsMemberOfEU(country));
			}
		}

		public void TestLoadDataInConstructor()
		{
			var provider = new EuropeanUnionCustomsMembersProvider();

			object GetFieldValue(string fieldName) => typeof(EuropeanUnionCustomsMembersProvider).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(provider);

			AssertNotNull(GetFieldValue("europeanUnionForCustomsMembers"));
			AssertNotNull(GetFieldValue("europeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers"));
			AssertNotNull(GetFieldValue("europeanUnionAndCtCountries"));
		}
	}
}
