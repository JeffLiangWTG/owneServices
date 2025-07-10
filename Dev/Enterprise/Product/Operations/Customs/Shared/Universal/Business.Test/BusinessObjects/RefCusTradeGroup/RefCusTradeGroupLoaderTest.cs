using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTradeGroup.Loader))]
	sealed class RefCusTradeGroupLoaderTest : LoaderTestCase
	{
		public void TestIsCountryPartOfTradeGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var eu = helper.CreateTradeGroup("EUN", "1010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eu, "DE", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
			var loader = new RefCusTradeGroup.Loader(Factory);
			Assert(loader.IsCountryPartOfTradeGroup("DE", "1010", "EUN", ZDateTime.Today));
			Assert(!loader.IsCountryPartOfTradeGroup("CN", "1010", "EUN", ZDateTime.Today));
		}

		public void TestLoad()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.EastTimor, "TR1", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 31));
			var tradeGroup1Country = helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.Bahamas, new ZDate(2016, 1, 1), new ZDate(2016, 12, 31));
			var tradeGroup2 = helper.CreateTradeGroup(Core.Constants.CountryCodes.EastTimor, "TR2", new ZDateTime(2016, 7, 1), new ZDateTime(2016, 12, 31));
			var tradeGroup3 = helper.CreateTradeGroup(Core.Constants.CountryCodes.EastTimor, "TR3", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 5, 31));
			var tradeGroup4 = helper.CreateTradeGroup(Core.Constants.CountryCodes.EastTimor, "TR4", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 6, 30));
			var tradeGroup4Country = helper.AddCountry(tradeGroup4, Core.Constants.CountryCodes.Bangladesh, new ZDate(2016, 1, 1), new ZDate(2016, 12, 31));
			var tradeGroup5 = helper.CreateTradeGroup(Core.Constants.CountryCodes.EastTimor, "TR5", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 31));
			var tradeGroup5Country = helper.AddCountry(tradeGroup4, Core.Constants.CountryCodes.Bahamas, new ZDate(2016, 1, 1), new ZDate(2016, 5, 31));
			var loader = new RefCusTradeGroup.Loader(Factory);
			var tradeGroups = loader.Load(Core.Constants.CountryCodes.Ecuador, new ZDateTime(2016, 6, 1), ZString.Empty);
			AssertEquals("tradeGroups.Length", 0, tradeGroups.Length);
			tradeGroups = loader.Load(Core.Constants.CountryCodes.EastTimor, new ZDateTime(2016, 6, 1), ZString.Empty);
			AssertEquals("tradeGroups.Length", 3, tradeGroups.Length);
			AssertEquals(tradeGroup1.PK, tradeGroups[0].PK);
			AssertEquals(tradeGroup4.PK, tradeGroups[1].PK);
			AssertEquals(tradeGroup5.PK, tradeGroups[2].PK);
			tradeGroups = loader.Load(Core.Constants.CountryCodes.EastTimor, new ZDateTime(2016, 6, 1), Core.Constants.CountryCodes.Cambodia);
			AssertEquals("tradeGroups.Length", 0, tradeGroups.Length);
			tradeGroups = loader.Load(Core.Constants.CountryCodes.EastTimor, new ZDateTime(2016, 6, 1), Core.Constants.CountryCodes.Bahamas);
			AssertEquals("tradeGroups.Length", 1, tradeGroups.Length);
			AssertEquals(tradeGroup1.PK, tradeGroups[0].PK);
			AssertEquals(tradeGroup1.PK, loader.Load(Core.Constants.CountryCodes.EastTimor, "TR1", new ZDateTime(2016, 6, 2)).PK);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusTradeGroup.Loader(Factory);
		}
	}
}
