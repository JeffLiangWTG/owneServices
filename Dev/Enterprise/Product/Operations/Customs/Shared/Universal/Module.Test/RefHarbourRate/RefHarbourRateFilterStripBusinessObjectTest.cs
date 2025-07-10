using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(RefHarbourRateFilterStripBusinessObject))]
	public class RefHarbourRateFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		RefHarbourRate refHarbourRate1;
		RefHarbourRate refHarbourRate2;

		public void TestTypeFilter()
		{
			SetupData();
			var filterStrip = new RefHarbourRateFilterStripBusinessObject();
			var typeFilter = (ModuleTextFilter)filterStrip[RefHarbourRateFilters.Type];
			AssertEquals("typeFilter.Visibility", FilterVisibility.Visible, typeFilter.Visibility);
			typeFilter.IsActive = true;
			typeFilter.Property = "IMP";
			var filter = filterStrip.Filter;
			AssertEquals("refHarbourRate1", true, refHarbourRate1.MatchesFilter(filter));
			AssertEquals("refHarbourRate2", false, refHarbourRate2.MatchesFilter(filter));
		}

		public void TestPortFilter()
		{
			SetupData();
			var filterStrip = new RefHarbourRateFilterStripBusinessObject();
			var portFilter = (ModuleTextFilter)filterStrip[RefHarbourRateFilters.Port];
			AssertEquals("portFilter.Visibility", FilterVisibility.Visible, portFilter.Visibility);
			portFilter.IsActive = true;
			portFilter.Property = "123";
			var filter = filterStrip.Filter;
			AssertEquals("refHarbourRate1", true, refHarbourRate1.MatchesFilter(filter));
			AssertEquals("refHarbourRate2", false, refHarbourRate2.MatchesFilter(filter));
		}

		public void TestModeFilter()
		{
			SetupData();
			var filterStrip = new RefHarbourRateFilterStripBusinessObject();
			var modeFilter = (ModuleTextFilter)filterStrip[RefHarbourRateFilters.Mode];
			modeFilter.IsActive = true;
			modeFilter.Property = RefHarbourRateModeList.Codes.CON;
			var filter = filterStrip.Filter;
			AssertEquals("refHarbourRate1", true, refHarbourRate1.MatchesFilter(filter));
			AssertEquals("refHarbourRate2", false, refHarbourRate2.MatchesFilter(filter));
		}

		public void TestCommodityFilter()
		{
			SetupData();
			var filterStrip = new RefHarbourRateFilterStripBusinessObject();
			var commodityFilter = (ModuleTextFilter)filterStrip[RefHarbourRateFilters.Commodity];
			AssertEquals("commodityFilter.Visibility", FilterVisibility.Visible, commodityFilter.Visibility);
			commodityFilter.IsActive = true;
			commodityFilter.Property = "111";
			var filter = filterStrip.Filter;
			AssertEquals("refHarbourRate1", true, refHarbourRate1.MatchesFilter(filter));
			AssertEquals("refHarbourRate2", false, refHarbourRate2.MatchesFilter(filter));
		}

		public void TestPortTaxTypeFilter()
		{
			SetupData();
			var filterStrip = new RefHarbourRateFilterStripBusinessObject();
			var portTaxTypeFilter = (ModuleTextFilter)filterStrip[RefHarbourRateFilters.PortTaxType];
			AssertEquals("portTaxTypeFilter.Visibility", FilterVisibility.Visible, portTaxTypeFilter.Visibility);
			portTaxTypeFilter.IsActive = true;
			portTaxTypeFilter.Property = "AAA";
			var filter = filterStrip.Filter;
			AssertEquals("refHarbourRate1", true, refHarbourRate1.MatchesFilter(filter));
			AssertEquals("refHarbourRate2", false, refHarbourRate2.MatchesFilter(filter));
		}

		public void TestEffectiveDateFilter()
		{
			SetupData();
			var filterStrip = new RefHarbourRateFilterStripBusinessObject();
			var effectiveDateFilter = (ModuleSingleDateFilter)filterStrip[RefHarbourRateFilters.EffectiveDate];
			AssertEquals("effectiveDateFilter.Visibility", FilterVisibility.Visible, effectiveDateFilter.Visibility);
			AssertEquals("effectiveDateFilter.Property1", ZDateTime.Today, effectiveDateFilter.Property1);
			effectiveDateFilter.IsActive = true;

			effectiveDateFilter.Property1 = ZDateTime.Today;
			var filter = filterStrip.Filter;
			AssertEquals("refHarbourRate1", true, refHarbourRate1.MatchesFilter(filter));
			AssertEquals("refHarbourRate2", false, refHarbourRate2.MatchesFilter(filter));

			effectiveDateFilter.Property1 = ZDateTime.Today.AddDays(2);
			filter = filterStrip.Filter;
			AssertEquals("refHarbourRate1", false, refHarbourRate1.MatchesFilter(filter));
			AssertEquals("refHarbourRate2", true, refHarbourRate2.MatchesFilter(filter));

			effectiveDateFilter.Property1 = ZDateTime.Invalid;
			filter = filterStrip.Filter;
			AssertEquals("refHarbourRate1: empty filter", true, refHarbourRate1.MatchesFilter(filter));
			AssertEquals("refHarbourRate2: empty filter", true, refHarbourRate2.MatchesFilter(filter));
		}

		public void TestCountryOrGroupingFilter()
		{
			SetupData();
			var filterStrip = new RefHarbourRateFilterStripBusinessObject();
			var countryOrGroupingFilter = (ModuleNkFilter)filterStrip[RefHarbourRateFilters.CountryOrGrouping];
			AssertEquals("countryOrGroupingFilter.Visibility", FilterVisibility.AlwaysVisible, countryOrGroupingFilter.Visibility);
			AssertEquals("countryOrGroupingFilter.ModuleId", ModuleIDs.Customs.Universal.RefDataGrouping, countryOrGroupingFilter.ModuleId);
			AssertType<RefDataGroupingCollection>("countryOrGroupingFilter.list", countryOrGroupingFilter.List);
			AssertEquals("countryOrGroupingFilter.DefaultProperty", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, countryOrGroupingFilter.DefaultProperty);
			countryOrGroupingFilter.IsActive = true;
			countryOrGroupingFilter.Property = Core.Constants.CountryCodes.France;
			var filter = filterStrip.Filter;
			AssertEquals("refHarbourRate1", true, refHarbourRate1.MatchesFilter(filter));
			AssertEquals("refHarbourRate2", false, refHarbourRate2.MatchesFilter(filter));
		}

		public void TestRateFormulaFilter()
		{
			SetupData();
			var filterStrip = new RefHarbourRateFilterStripBusinessObject();
			var rateFormulaFilter = (ModuleTextFilter)filterStrip[RefHarbourRateFilters.RateFormula];
			AssertEquals("rateFormulaFilter.Visibility", FilterVisibility.Visible, rateFormulaFilter.Visibility);
			rateFormulaFilter.IsActive = true;
			rateFormulaFilter.Property = "[ABC]*1";
			var filter = filterStrip.Filter;
			AssertEquals("refHarbourRate1", true, refHarbourRate1.MatchesFilter(filter));
			AssertEquals("refHarbourRate2", false, refHarbourRate2.MatchesFilter(filter));
		}

		void SetupData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var record1 = helper.CreateHarbourRate("IMP", "123", RefHarbourRateModeList.Codes.CON, "111", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), "[ABC]*1", Core.Constants.CountryCodes.France, "AAA");
			var record2 = helper.CreateHarbourRate("TAX", "456", RefHarbourRateModeList.Codes.BLK, "222", ZDate.Today.AddDays(1), ZDate.Today.AddDays(10), "[ABC]*2", Core.Constants.CountryCodes.Italy, "BBB");
			Factory.Save();
			refHarbourRate1 = Factory.Load<RefHarbourRate>(record1.PK);
			refHarbourRate2 = Factory.Load<RefHarbourRate>(record2.PK);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new RefHarbourRateFilterStripBusinessObject();
	}
}
