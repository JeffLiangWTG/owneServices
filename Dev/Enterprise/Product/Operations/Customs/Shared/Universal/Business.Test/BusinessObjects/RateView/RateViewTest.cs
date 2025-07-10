using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RateView))]
	public class RateViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var rateView = Factory.New<RateView>();
			CombineAssertions(() =>
			{
				AssertEquals("ZZ2_DataSet", "O", rateView.ZZ2_DataSet);
				AssertEquals("ZZ2_ParentTableType", "CR1", rateView.ZZ2_ParentTableType);
			}

			);
		}

		public void TestZZ2_DataSet_ReadOnly()
		{
			var rate = Factory.New<RateView>();
			Assert(rate.ZZ2_DataSetInfo.ReadOnly);
		}

		public void TestITariffEffectiveDatesRelatedBusinessObjectMembers()
		{
			var testRate = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0", dataGrouping: "EUN");
			ITariffEffectiveDatesRelatedBusinessObject bizObj = testRate;
			AssertEquals("DataGrouping", "EUN", bizObj.DataGrouping);
			AssertEquals("StartDate", new ZDateTime(2010, 12, 10), bizObj.StartDate);
			AssertEquals("EndDate", new ZDateTime(2079, 06, 06), bizObj.EndDate);
		}

		public void TestFilteredRateApplicabilities()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var er = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping("DG1", parent: er);
			Factory.Save();
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			var rateType = UniversalReferenceTestDataHelper.CreateCusRateType(Factory, Core.Constants.CountryCodes.Eritrea, Constants.RateTypes.Duty, "Duty", ensureDataGroupingExists: false);
			var rateCode = helper.CreateCusRateCode(Factory, "RC1", rateType.PK);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var zaTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "ZATRADE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);
			var erTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Eritrea, "ERTRADE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);
			var tg12 = helper.CreateTradeGroup("DG1", "TG12", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);
			var rate = helper.CreateRate(tariff, rateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0");
			var applic1 = helper.CreateCusApplicabilityInternal(rate, erTradeGroup, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31), "DJC", "O");
			var applic2 = helper.CreateCusApplicabilityInternal(rate, zaTradeGroup, new ZDateTime(2012, 12, 10), new ZDateTime(2079, 06, 06), "LSC", "P");
			var applic3 = helper.CreateCusApplicabilityInternal(rate, tg12, new ZDateTime(2012, 12, 10), new ZDateTime(2079, 06, 06), "CA1", "G");
			Factory.Save();
			var f = new BusinessObjectFactory();
			var rateReloaded = f.Load<RateView>(rate.PK);
			var wrapper = rateReloaded.CusTariff.Wrapper;
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = ZDate.Empty;
			var filteredCollection = rateReloaded.FilteredRateApplicabilities;
			var collection = rateReloaded.RateApplicabilities;
			AssertEquals("filteredCollection", 2, filteredCollection.Count);
			AssertNotNull("filteredCollection.applic1", filteredCollection.FindByPK(applic1.PK));
			AssertNotNull("filteredCollection.applic3", filteredCollection.FindByPK(applic3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.applic1", collection.FindByPK(applic1.PK));
			AssertNotNull("collection.applic2", collection.FindByPK(applic2.PK));
			AssertNotNull("collection.applic3", collection.FindByPK(applic3.PK));
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.applic1", filteredCollection.FindByPK(applic1.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.applic1", collection.FindByPK(applic1.PK));
			AssertNotNull("collection.applic2", collection.FindByPK(applic2.PK));
			AssertNotNull("collection.applic3", collection.FindByPK(applic3.PK));
			wrapper.EffectiveDataGrouping = "DG1";
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.applic3", filteredCollection.FindByPK(applic3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.applic1", collection.FindByPK(applic1.PK));
			AssertNotNull("collection.applic2", collection.FindByPK(applic2.PK));
			AssertNotNull("collection.applic3", collection.FindByPK(applic3.PK));
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = new ZDate(2012, 12, 9);
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.applic1", filteredCollection.FindByPK(applic1.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.applic1", collection.FindByPK(applic1.PK));
			AssertNotNull("collection.applic2", collection.FindByPK(applic2.PK));
			AssertNotNull("collection.applic3", collection.FindByPK(applic3.PK));
			wrapper.EffectiveDate = new ZDate(2013, 1, 1);
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.applic3", filteredCollection.FindByPK(applic3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.applic1", collection.FindByPK(applic1.PK));
			AssertNotNull("collection.applic2", collection.FindByPK(applic2.PK));
			AssertNotNull("collection.applic3", collection.FindByPK(applic3.PK));
		}

		public void TestRateApplicabilities()
		{
			var eutrade = Helper.CreateTradeGroup(Core.Constants.CountryCodes.Eritrea, "EUTRADE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "EU Trade Agreement Jan 2000");
			var sadc = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "SADC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "SADC Trade Agreement 2000");
			var testRate = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0");
			var applic1 = Helper.CreateCusApplicabilityInternal(testRate, eutrade, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31), "DJC", "O");
			var applic2 = Helper.CreateCusApplicabilityInternal(testRate, sadc, new ZDateTime(2012, 12, 10), new ZDateTime(2079, 06, 06), "LSC", "P");
			Factory.Save();
			var f = new BusinessObjectFactory();
			var rateReloaded = f.Load<RateView>(testRate.PK);
			var wrapper = rateReloaded.CusTariff.Wrapper;
			wrapper.EffectiveDataGrouping = "@#";
			wrapper.EffectiveDate = new ZDate(2009, 12, 10);
			var collection = rateReloaded.RateApplicabilities;
			AssertEquals(2, collection.Count);
			AssertNotNull("applic1", collection.FindByPK(applic1.PK));
			AssertNotNull("applic2", collection.FindByPK(applic2.PK));
		}

		public void TestPropertyNamesFromFriends()
		{
			var preference = Helper.CreatePreferenceForCountryAndGroupingInternal("DAN", "DANIEL", Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();
			var testRate1 = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0", preferencePk: preference.PK);
			var refCusRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.UnitedKingdom, "DJC", "Clarke");
			var refCusRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "LJL", refCusRateType.PK);
			refCusRateCode.ZY1_Description = "Little John Locke";
			testRate1.ZZ2_ZY1_RateCode = refCusRateCode.PK;
			AssertEquals("LJL", testRate1.RateCode);
			AssertEquals("Little John Locke", testRate1.RateDescription);
			AssertEquals(Factory.Load<CusRefPreferenceView>(preference.PK), testRate1.Preference);
			AssertEquals("DAN", testRate1.PreferenceCode);
			AssertEquals("DANIEL", testRate1.PreferenceDescription);
		}

		public void TestRateFormulaDescription_ReturnsZZ2_RateFormulaDerivedFromIfNotEmpty()
		{
			var rateView = Factory.New<RateView>();
			rateView.ZZ2_RateFormulaDerivedFrom = "DEF";
			AssertEquals("DEF", rateView.RateFormulaDescription);
		}

		public void TestRateFormulaDescription_ReturnsHumanReadableFormulaWhenZZ2_RateFormulaDerivedFromIsEmpty()
		{
			var rateView = Factory.New<RateView>();
			rateView.ZZ2_ZZZ_NKDataGrouping = "EUN";
			rateView.ZZ2_RateFormula = "VFD * 0.2";
			rateView.ZZ2_RateFormulaDerivedFrom = "";
			AssertEquals("20.0% of the Value for Duty", rateView.RateFormulaDescription);
		}

		public void TestRateFormulaDescription_HumanReadableFormula_Units()
		{
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList("EUN", "CUSUQ", "LPA", "Liters Pure Alcohol", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var rateView = Factory.New<RateView>();
			rateView.ZZ2_ZZZ_NKDataGrouping = "EUN";
			rateView.ZZ2_RateFormula = "24.3 * [LPA]";
			AssertEquals("24.3 EUR per Liters Pure Alcohol", rateView.RateFormulaDescription);
		}

		public void TestRateFormulaDescription_CacheRefreshOnDataGroupingChange()
		{
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList("EUN", "CUSUQ", "LPA", "Liters Pure Alcohol", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList("IT", "CUSUQ", "LPA", "Something Else", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var rateView = Factory.New<RateView>();
			rateView.ZZ2_ZZZ_NKDataGrouping = "EUN";
			rateView.ZZ2_RateFormula = "24.3 * [LPA]";
			AssertEquals("24.3 EUR per Liters Pure Alcohol", rateView.RateFormulaDescription);

			rateView.ZZ2_ZZZ_NKDataGrouping = "IT";
			AssertEquals("24.3 EUR per Something Else", rateView.RateFormulaDescription);
		}

		public void TestRateFormulaDescription_CacheRefreshOnCurrencyOverrideChange()
		{
			var rateView = Factory.New<RateView>();
			rateView.ZZ2_ZZZ_NKDataGrouping = "CA";
			rateView.ZZ2_RateFormula = "119.54 * [PCE]";
			AssertEquals("119.54 CAD per PCE", rateView.RateFormulaDescription);

			rateView.ZZ2_RX_NKCurrencyOverride = "CNY";
			AssertEquals("119.54 CNY per PCE", rateView.RateFormulaDescription);
		}

		public void TestRateFormulaDescription_CacheRefreshOnRateFormulaChange()
		{
			var rateView = Factory.New<RateView>();
			rateView.ZZ2_ZZZ_NKDataGrouping = "EUN";
			rateView.ZZ2_RateFormula = "VFD * 0.2";
			AssertEquals("20.0% of the Value for Duty", rateView.RateFormulaDescription);

			rateView.ZZ2_RateFormula = "VFD * 0.3";
			AssertEquals("30.0% of the Value for Duty", rateView.RateFormulaDescription);
		}

		public void TestRateFormulaDescription_CacheRefreshOnFormulaDerivedFromChange()
		{
			var rateView = Factory.New<RateView>();
			rateView.ZZ2_RateFormulaDerivedFrom = "ABC";
			AssertEquals("ABC", rateView.RateFormulaDescription);

			rateView.ZZ2_RateFormulaDerivedFrom = "DEF";
			AssertEquals("DEF", rateView.RateFormulaDescription);
		}

		public void TestRateFormulaDescription_Caption()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<RateView>().RateFormulaDescriptionInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Rate Formula Description", resourceStringDataAttribute.Caption);
				AssertEquals("ShortCaption", "Rate Formula Desc.", resourceStringDataAttribute.ShortCaption);
			});
		}

		public void TestRateFormulaDescription_UnparsableFormula()
		{
			var consolError = Console.Error;
			try
			{
				using (var writer = new StringWriter())
				{
					Console.SetError(writer);
					var rateView = Factory.New<RateView>();
					rateView.ZZ2_RateFormula = "#";

					AssertEquals("", rateView.RateFormulaDescription);
					AssertContains("Unable to parse formula: #", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
					AssertEquals("Expecting no logging to console", "", writer.ToString());
				}
			}
			finally
			{
				Console.SetError(consolError);
			}
		}

		public void TestRateFormulaDescription_UnconvertableFormula()
		{
			var rateView = Factory.New<RateView>();
			rateView.ZZ2_ZZZ_NKDataGrouping = "EUN";
			rateView.ZZ2_RateFormula = "99999999999999999999999999999999999999999999999 * VFD";

			AssertEquals("", rateView.RateFormulaDescription);
			AssertContains("Unable to convert formula to human readable string: 99999999999999999999999999999999999999999999999 * VFD", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestRateFormulaNumber()
		{
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList("EUN", "CUSUQ", "LPA", "Liters Pure Alcohol", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var rateView = Factory.New<RateView>();
			rateView.ZZ2_ZZZ_NKDataGrouping = "EUN";
			rateView.ZZ2_RateFormula = "24.3 * [LPA]";
			AssertEquals(24.3m, rateView.RateFormulaNumber);
		}

		public void TestCusPreference()
		{
			var preference = Helper.CreatePreferenceForCountryAndGroupingInternal("PRF", "Cus Preference", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			var rate = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2016, 12, 10), new ZDateTime(2079, 06, 06), "0", preference.PK);
			AssertNotNull("CusPreference Is Null", rate.Preference);
			AssertEquals("CusPreference ZZS_Preference not correct", rate.Preference.ZZS_Preference, "PRF");
			AssertEquals("CusPreference ZZS_Description not correct", rate.Preference.ZZS_Description, "Cus Preference");
		}

		public void TestGetApplicableTradeGroups()
		{
			var testRate1 = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 01, 10), new ZDateTime(2079, 06, 06), "0");
			var testRate2 = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 01, 29), new ZDateTime(2012, 06, 06), "0");
			var testRate3 = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 10, 29), new ZDateTime(2079, 06, 06), "0");
			testRate1.Factory.Save();
			testRate2.Factory.Save();
			testRate3.Factory.Save();
			var testTradeGroup1 = Helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "TG1", new ZDateTime(2001, 01, 01), new ZDateTime(2079, 06, 06));
			testTradeGroup1.Factory.Save();
			Helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(2001, 01, 01), new ZDate(2079, 06, 06));
			Helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.Germany, new ZDate(2001, 01, 01), new ZDate(2079, 06, 06));
			Helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2001, 01, 01), new ZDate(2079, 06, 06));
			var testTradeGroup2 = Helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "TG2", new ZDateTime(2015, 01, 01), new ZDateTime(2079, 06, 06));
			testTradeGroup2.Factory.Save();
			Helper.AddCountry(testTradeGroup2, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(2015, 01, 01), new ZDate(2079, 06, 06));
			Helper.AddCountry(testTradeGroup2, Enterprise.Core.Constants.CountryCodes.UnitedStates, new ZDate(2015, 01, 01), new ZDate(2079, 06, 06));
			Helper.AddCountry(testTradeGroup2, Enterprise.Core.Constants.CountryCodes.Uzbekistan, new ZDate(2015, 01, 01), new ZDate(2079, 06, 06));
			var testTradeGroup3 = Helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "TG3", new ZDateTime(2001, 01, 01), new ZDateTime(2008, 12, 31));
			testTradeGroup3.Factory.Save();
			Helper.AddCountry(testTradeGroup3, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(2001, 01, 01), new ZDate(2005, 06, 06));
			Helper.AddCountry(testTradeGroup3, Enterprise.Core.Constants.CountryCodes.Germany, new ZDate(2001, 01, 01), new ZDate(2005, 06, 06));
			Helper.AddCountry(testTradeGroup3, Enterprise.Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2001, 01, 01), new ZDate(2005, 06, 06));
			var testTradeGroup4 = Helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "TG4", new ZDateTime(2001, 01, 01), new ZDateTime(2008, 12, 31));
			testTradeGroup4.Factory.Save();
			Helper.AddCountry(testTradeGroup4, Enterprise.Core.Constants.CountryCodes.Germany, new ZDate(2002, 01, 01), new ZDate(2005, 06, 06));
			Helper.AddCountry(testTradeGroup4, Enterprise.Core.Constants.CountryCodes.Antarctica, new ZDate(2002, 01, 01), new ZDate(2005, 06, 06));
			var testApplicability11 = Helper.CreateCusApplicability(testRate1, testTradeGroup1, new ZDateTime(2004, 01, 01), new ZDateTime(2017, 06, 06));
			var testApplicability12 = Helper.CreateCusApplicability(testRate1, testTradeGroup2, new ZDateTime(2004, 01, 01), new ZDateTime(2018, 06, 06));
			var testApplicability2 = Helper.CreateCusApplicability(testRate2, testTradeGroup2, new ZDateTime(2016, 01, 01), new ZDateTime(2017, 06, 06));
			var testApplicability3 = Helper.CreateCusApplicability(testRate3, testTradeGroup3, new ZDateTime(2009, 01, 01), new ZDateTime(2017, 06, 06));
			RefCusExcludedTradeGroup excludedTradeGroup = Helper.CreateExcludedTradeGroup(testTradeGroup4);
			testApplicability11.ExcludedTradeGroups.Add(excludedTradeGroup);
			Factory.Save();
			var zaApplicableTradeGroups2016 = testRate1.GetApplicableTradeGroups("ZA", new ZDateTime(2016, 1, 1));
			AssertEquals("GetApplicableTradeGroups(ZA 1) - count - is not correct", 2, zaApplicableTradeGroups2016.Count());
			Assert("GetApplicableTradeGroups(ZA 1) - should return a TG1 item.", zaApplicableTradeGroups2016.Any(group => group.ZZA_TradeGroup == "TG1"));
			Assert("GetApplicableTradeGroups(ZA 1) - should return a TG2 item.", zaApplicableTradeGroups2016.Any(group => group.ZZA_TradeGroup == "TG2"));
			AssertEquals("GetApplicableTradeGroups(ZA 1) - date/count - is not correct", 1, testRate1.GetApplicableTradeGroups("ZA", new ZDateTime(2018, 1, 1)).Count());
			AssertEquals("GetApplicableTradeGroups(ZA 1) - date/count - is not correct", 0, testRate1.GetApplicableTradeGroups("ZA", new ZDateTime(2019, 1, 1)).Count());
			AssertEquals("GetApplicableTradeGroups(DE 1) - count - is not correct", 0, testRate1.GetApplicableTradeGroups("DE", new ZDateTime(2016, 1, 1)).Count());
			AssertEquals("GetApplicableTradeGroups(DE 1) - count ExcludedTradeGroups - is not correct", 0, testRate1.GetApplicableTradeGroups("DE", new ZDateTime(2016, 1, 1)).Count());
			AssertEquals("GetApplicableTradeGroups(UZ 1) - count - is not correct", 1, testRate1.GetApplicableTradeGroups("UZ", new ZDateTime(2016, 1, 1)).Count());
			AssertEquals("GetApplicableTradeGroups(UZ 1) - group 0 - is not correct", "TG2", testRate1.GetApplicableTradeGroups("UZ", new ZDateTime(2016, 1, 1)).First().ZZA_TradeGroup);
			AssertEquals("GetApplicableTradeGroups(XX 1) - count - is not correct", 0, testRate1.GetApplicableTradeGroups("XX", new ZDateTime(2016, 1, 1)).Count());
			AssertEquals("GetApplicableTradeGroups(ZA 2) - count - is not correct", 1, testRate2.GetApplicableTradeGroups("ZA", new ZDateTime(2016, 1, 1)).Count());
			AssertEquals("GetApplicableTradeGroups(ZA 2) - group 0 - is not correct", "TG2", testRate2.GetApplicableTradeGroups("ZA", new ZDateTime(2016, 1, 1)).First().ZZA_TradeGroup);
			AssertEquals("GetApplicableTradeGroups(GB 3) - count - is not correct", 1, testRate3.GetApplicableTradeGroups("GB", new ZDateTime(2016, 1, 1)).Count());
			AssertEquals("GetApplicableTradeGroups(GB 3) - group 0 - is not correct", "TG3", testRate3.GetApplicableTradeGroups("GB", new ZDateTime(2016, 1, 1)).First().ZZA_TradeGroup);
			AssertEquals("GetApplicableTradeGroups(UZ 3) - count - is not correct", 0, testRate3.GetApplicableTradeGroups("UX", new ZDateTime(2016, 1, 1)).Count());
		}

		public void TestRateType()
		{
			var testRate = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0");
			AssertEquals("ZZI_ZZR_RateTypeCode", "DTY", testRate.ZZ2_ZZR_RateTypeCode);
		}

		public void TestRateType_Manual()
		{
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Congo, "1P1", ensureDataGroupingExists: false);
			var dutyRateCode = helper.CreateCusRateCode(Factory, "DJC", ZGuid.Empty, false, cusRateType: "OTH", countryCode: Core.Constants.CountryCodes.Congo);
			var manualTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Congo, tariffType.PK, "ManualTariff", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "dummy Description 0", isSystem: false);
			var dutyRateView = helper.CreateRate(manualTariff, dutyRateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "0", isSystem: false);
			AssertEquals("ZZI_ZZR_RateTypeCode", "OTH", dutyRateView.ZZ2_ZZR_RateTypeCode);
		}

		public void TestZZ2_ZZR_RateTypeDesc_IsSystem()
		{
			var testRate = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0");
			AssertEquals("ZZ2_ZZR_RateTypeDesc", "Duty", testRate.ZZ2_ZZR_RateTypeDesc);
		}

		public void TestZZ2_ZZR_RateTypeDesc()
		{
			var rateCode = Factory.New<CusRefRateCode>();
			rateCode.CR7_RateCode = "AA";
			rateCode.CR7_Description = "AA Desc";
			rateCode.CR7_RateType = RefCusRateTypeCustomizableList.Codes.OTH;
			rateCode.CR7_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			Factory.Save();
			var rate = Factory.New<RateView>();
			rate.ZZ2_IsSystem = false;
			rate.ZZ2_ZY1_RateCode = rateCode.PK;
			AssertEquals("ZZ2_ZZR_RateTypeDesc", "Other", rate.ZZ2_ZZR_RateTypeDesc);
		}

		public void TestDelete()
		{
			var rateView = Factory.New<RateView>();
			var applicabilityView = rateView.RateApplicabilities.AddNew();
			rateView.Delete();
			CombineAssertions(() =>
			{
				AssertEquals("rateView.IsDeleted", true, rateView.IsDeleted);
				AssertEquals("applicabilityView.IsDeleted", true, applicabilityView.IsDeleted);
			}

			);
		}

		public void TestZZ2_ZY1_RateCode_Caption()
		{
			var rateView = Factory.New<RateView>();
			AssertEquals("Rate Code", DataBoundResourceStrings.GetDataForProperty(rateView.ZZ2_ZY1_RateCodeInfo).Caption);
		}

		public void TestZZ2_ZZR_RateTypeCode_Caption()
		{
			var rateView = Factory.New<RateView>();
			AssertEquals("Rate Type", DataBoundResourceStrings.GetDataForProperty(rateView.ZZ2_ZZR_RateTypeCodeInfo).Caption);
		}

		public override void TestCallsBaseSetDefaultValues()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObject() => TestRate;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => TestRate;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => TestRate;
		protected override bool CanPersistedObjectBeDeleted => false;
		protected override void SetUp()
		{
			base.SetUp();
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			dutyRateType = Helper.CreateCusRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty", ensureDataGroupingExists: false);
			s1p1TariffType = Helper.CreateTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1", ensureDataGroupingExists: false);
			Factory.Save();
			djcRateCode = Helper.CreateCusRateCode(Factory, "DJC", dutyRateType.PK);
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		RefCusRateType dutyRateType;
		CusRefRateCodeView djcRateCode;
		TariffView CusTariff => cusTariff ?? (cusTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0"));
		TariffView cusTariff;
		RateView TestRate => testRate ?? (testRate = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0"));
		RateView testRate;
		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}
