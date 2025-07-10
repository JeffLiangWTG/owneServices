using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTradeGroupView.Loader))]
	class CusRefTradeGroupViewLoaderTest : LoaderTestCase
	{
		public void TestIsCountryPartOfTradeGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var eu = helper.CreateTradeGroup("EUN", "1010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eu, "DE", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
			var loader = new CusRefTradeGroupView.Loader(Factory);
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
			var tradeGroup5Country = helper.AddCountry(tradeGroup5, Core.Constants.CountryCodes.Bahamas, new ZDate(2016, 1, 1), new ZDate(2016, 5, 31));
			var tradeGroup6 = helper.CreateTradeGroup(Core.Constants.CountryCodes.EastTimor, "TR1", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 31), isSystem: false);
			var tradeGroup6Country = helper.AddCountry(tradeGroup6, Core.Constants.CountryCodes.Bahamas, new ZDate(2016, 1, 1), new ZDate(2016, 12, 31));
			var loader = new CusRefTradeGroupView.Loader(Factory);
			var tradeGroups = loader.Load(Core.Constants.CountryCodes.Ecuador, new ZDateTime(2016, 6, 1), ZString.Empty);
			AssertEquals("tradeGroups.Length", 0, tradeGroups.Length);
			tradeGroups = loader.Load(Core.Constants.CountryCodes.EastTimor, new ZDateTime(2016, 6, 1), ZString.Empty);
			AssertEquals("tradeGroups.Length", 4, tradeGroups.Length);
			AssertCollectionContains(tradeGroup1, tradeGroups);
			AssertCollectionContains(tradeGroup4, tradeGroups);
			AssertCollectionContains(tradeGroup5, tradeGroups);
			AssertCollectionContains(tradeGroup6, tradeGroups);
			tradeGroups = loader.Load(Core.Constants.CountryCodes.EastTimor, new ZDateTime(2016, 6, 1), Core.Constants.CountryCodes.Cambodia);
			AssertEquals("tradeGroups.Length", 0, tradeGroups.Length);
			tradeGroups = loader.Load(Core.Constants.CountryCodes.EastTimor, new ZDateTime(2016, 6, 1), Core.Constants.CountryCodes.Bahamas);
			AssertEquals("tradeGroups.Length", 2, tradeGroups.Length);
			AssertEquals(tradeGroup1, tradeGroups[0]);
			AssertEquals(tradeGroup6, tradeGroups[1]);
			AssertEquals(tradeGroup1.PK, loader.Load(Core.Constants.CountryCodes.EastTimor, "TR1", new ZDateTime(2016, 6, 2)).PK);
			AssertEquals(tradeGroup6.PK, loader.Load(Core.Constants.CountryCodes.EastTimor, "TR1", new ZDateTime(2016, 6, 2), Core.Constants.Customs.Universal.DataSetTypes.OWNData).PK);
			AssertEquals(tradeGroup1.PK, loader.Load(Core.Constants.CountryCodes.EastTimor, "TR1", new ZDateTime(2016, 6, 2), Core.Constants.Customs.Universal.DataSetTypes.WTGData).PK);

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { tradeGroup1.PK, tradeGroup4.PK, tradeGroup6.PK }, loader.Load(Core.Constants.CountryCodes.EastTimor, new ZString[] { "TR1", "TR4" }, new ZDateTime(2016, 6, 2)).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { tradeGroup6.PK }, loader.Load(Core.Constants.CountryCodes.EastTimor, new ZString[] { "TR1", "TR4" }, new ZDateTime(2016, 6, 2), Core.Constants.Customs.Universal.DataSetTypes.OWNData).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { tradeGroup1.PK, tradeGroup4.PK }, loader.Load(Core.Constants.CountryCodes.EastTimor, new ZString[] { "TR1", "TR4" }, new ZDateTime(2016, 6, 2), Core.Constants.Customs.Universal.DataSetTypes.WTGData).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { tradeGroup1.PK, tradeGroup4.PK, tradeGroup5.PK, tradeGroup6.PK }, loader.Load(Core.Constants.CountryCodes.EastTimor, Array.Empty<ZString>(), new ZDateTime(2016, 6, 2)).Select(x => x.PK));
		}

		public void TestIsCountryPartOfTradeGroups()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var gspTradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGsp, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "GSP");
			helper.AddCountry(gspTradeGroup, Core.Constants.CountryCodes.VietNam, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var gspPlusTradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGspPlus, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "GSP+");
			helper.AddCountry(gspPlusTradeGroup, Core.Constants.CountryCodes.SriLanka, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "XXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Not Defined");
			Factory.Save();
			var tradeGroupViewLoader = new CusRefTradeGroupView.Loader(Factory);
			Assert($"{Core.Constants.CountryCodes.VietNam} belongs to GSP", tradeGroupViewLoader.IsCountryPartOfTradeGroupAny(Core.Constants.CountryCodes.VietNam, new ZString[] { Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGsp, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGspPlus }, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDateTime.Now));
			Assert($"{Core.Constants.CountryCodes.VietNam} doesn't belong to GSP+", !tradeGroupViewLoader.IsCountryPartOfTradeGroupAny(Core.Constants.CountryCodes.VietNam, new ZString[] { Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGspPlus }, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDateTime.Now));
			Assert($"{Core.Constants.CountryCodes.SriLanka} belongs to GSP+", tradeGroupViewLoader.IsCountryPartOfTradeGroupAny(Core.Constants.CountryCodes.SriLanka, new ZString[] { Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGspPlus, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGspPlus }, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDateTime.Now));
			Assert($"{Core.Constants.CountryCodes.SriLanka} doesn't belong to GSP", !tradeGroupViewLoader.IsCountryPartOfTradeGroupAny(Core.Constants.CountryCodes.SriLanka, new ZString[] { Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGsp }, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDateTime.Now));
			Assert($"{Core.Constants.CountryCodes.China} doesn't belong to any trade group", !tradeGroupViewLoader.IsCountryPartOfTradeGroupAny(Core.Constants.CountryCodes.China, new ZString[] { Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGsp, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionGspPlus }, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDateTime.Now));
			Assert($"{Core.Constants.CountryCodes.SriLanka} doesn't belong to XXX", !tradeGroupViewLoader.IsCountryPartOfTradeGroupAny(Core.Constants.CountryCodes.SriLanka, new ZString[] { "XXX" }, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDateTime.Now));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusRefTradeGroupView.Loader(Factory);
		}
	}
}
