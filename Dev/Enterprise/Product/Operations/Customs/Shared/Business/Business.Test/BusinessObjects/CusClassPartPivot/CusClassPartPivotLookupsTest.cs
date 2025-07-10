using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusClassPartPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPrimaryPreferenceList_UseUniversalTariff()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;

			var tradeGroupStandard = RefDataHelper.CreateTradeGroup(currentCountry, "STANDARD", date1, date4);
			RefDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date4);
			Factory.Save();

			var hsnTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(currentCountry, "HSN");
			Factory.Save();
			var dutyRateType = RefDataHelper.CreateNewOrGetExistingRateType(currentCountry, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			Factory.Save();
			var preferenceSTD = RefDataHelper.CreatePreferenceForCountry("STD", "Standard", currentCountry);
			var preferenceRED = RefDataHelper.CreatePreferenceForCountry("RED", "Reduced", currentCountry);
			RefDataHelper.CreatePreferenceForCountry("MFN", "Most-favored Nation Duty", currentCountry);
			Factory.Save();
			var cusTariff = RefDataHelper.CreateTariff(currentCountry, hsnTariffType.PK, "123456789", date1, date4, "dummy Description 0");
			Factory.Save();
			var testRate1 = RefDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add11", "ord11");
			var testRate2 = RefDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "add21", "ord21");
			Factory.Save();

			pivot.CI_TariffNum = ZString.Empty;
			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			AssertContainsExactElementsInAnyOrder("UseUniversalTariff but tariff is empty", new string[] { "STD", "RED", "MFN" }, (pivot.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());

			pivot.CI_TariffNum = "XXXX";
			pivot.CI_RN_NKCountryOfOrigin = ZString.Empty;
			AssertContainsExactElementsInAnyOrder("UseUniversalTariff but CountryOfOrigin  is empty", new string[] { "STD", "RED", "MFN" }, (pivot.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());

			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			AssertContainsExactElementsInAnyOrder("UseUniversalTariff but No univiersal tariff", Array.Empty<string>(), (pivot.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());

			pivot.CI_TariffNum = "123456789";
			AssertContainsExactElementsInAnyOrder("match country using univiersal tariff", new string[] { "STD", "RED" }, (pivot.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());

			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.AlandIslands;
			AssertContainsExactElementsInAnyOrder("no match country using univiersal tariff", Array.Empty<string>(), (pivot.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());

			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			pivot.CI_ConcessionOrder = "ord11";
			AssertContainsExactElementsInAnyOrder("match ordernumber using univiersal tariff", new string[] { "STD" }, (pivot.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());
			pivot.CI_ConcessionOrder = "ord52";
			AssertContainsExactElementsInAnyOrder("no match ordernumber using univiersal tariff", Array.Empty<string>(), (pivot.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());

			pivot.CI_ConcessionOrder = "";
			pivot.CI_PrimaryPreference = "RED";
			AssertContainsExactElementsInAnyOrder("get preference lookup list regardless selected preference value", new string[] { "STD", "RED" }, (pivot.Lookups.PrimaryPreferenceList as CodeDescriptionPairList).GetAllCodes());
		}

		public void TestPrimaryPreferenceList_NotUseUniversalTariff()
		{
			var eunId = RefDataHelper.CreateNewOrGetExistingDataGrouping("EUN");
			RefDataHelper.CreateNewOrGetExistingDataGrouping(Env.CurrentCompany.Country.Code, parent: eunId);

			var cusPref1 = RefDataHelper.CreatePreferenceForCountry("140", "Exemption for End-Use Resulting from the CCT", "EUN");
			var cusPref2 = RefDataHelper.CreatePreferenceForCountry("200", "GSP Rate Without Conditions Or Limits (Including Ceilings)", "EUN");

			var wcoId = RefDataHelper.CreateNewOrGetExistingDataGrouping("WCO");
			RefDataHelper.CreateNewOrGetExistingDataGrouping("JP", parent: wcoId);

			var cusPref3 = RefDataHelper.CreatePreferenceForCountry("123", "123Description", "WCO");

			Factory.Save();

			var op = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			var ci = Factory.New<NonUniversalCusClassPartPivot_ForTest>();
			ci.CI_OP = op.PK;
			ci.CI_ChildType = "IMP";
			var preferencesList = ci.Lookups.PrimaryPreferenceList;

			AssertNotNull(preferencesList);
			AssertCollectionContains(cusPref1, preferencesList);
			AssertCollectionContains(cusPref2, preferencesList);
			AssertCollectionNotContains(cusPref3, preferencesList);

			ci.CI_RN_NKCountry = "JP";
			var preferencesListJP = ci.Lookups.PrimaryPreferenceList;

			AssertNotNull(preferencesListJP);
			AssertCollectionContains(cusPref3, preferencesListJP);
			AssertCollectionNotContains(cusPref1, preferencesListJP);
			AssertCollectionNotContains(cusPref2, preferencesListJP);
		}

		public void TestClassificationTypes()
		{
			AssertType<ClassificationTypeList>(pivot.Lookups.ClassificationTypes);
			AssertEquals(3, pivot.Lookups.ClassificationTypes.Count);
		}

		public void TestRelatedIndicatorList()
		{
			AssertEquals("RelatedIndicatorList", Factory.GetCachedValue<RelatedIndicatorList>(), pivot.Lookups.RelatedIndicatorList);
		}

		public void TestClassificationList()
		{
			AssertType<BaseClassificationCollection<BaseCusClassification>>(pivot.Lookups.ClassificationList);
		}

		public void TestTariffs()
		{
			AssertType<Universal.TariffViewCollection>(pivot.Lookups.Tariffs);
		}

		public void TestRelatedOrgs()
		{
			var filter = pivot.Lookups.RelatedOrgs.CompleteFilter;
			var org = Factory.New<OrgHeader>();
			AssertEquals(false, org.MatchesFilter(filter));
			org.OH_IsConsignee = true;
			AssertEquals(true, org.MatchesFilter(filter));
			org.OH_IsConsignee = false;
			org.OH_IsConsignor = true;
			AssertEquals(true, org.MatchesFilter(filter));
		}

		public void TestGoodsCatalogList()
		{
			var goodsCatalog1 = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			var goodsCatalog2 = Factory.NewWithValidTestData<BaseCusGoodsCatalog>().CGC_GC_Company = Factory.New<GlbCompany>().PK;
			var goodsCatalogList = pivot.Lookups.GoodsCatalogList;
			goodsCatalogList.Load();

			AssertContainsExactElementsInAnyOrder(goodsCatalog1, goodsCatalogList);
		}

		#region Implementation

		OrgSupplierPart part;
		BaseCusClassPartPivot pivot;

		protected override void SetUp()
		{
			base.SetUp();
			part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "P1";
			pivot = part.PivotsForBinding.AddNew();
		}

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;

		#endregion
	}
}
