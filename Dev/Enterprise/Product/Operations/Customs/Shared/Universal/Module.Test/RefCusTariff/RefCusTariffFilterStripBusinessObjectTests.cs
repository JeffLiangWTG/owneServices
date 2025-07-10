using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(RefCusTariffFilterStripBusinessObject))]
	public class RefCusTariffFilterStripBusinessObjectTests : FilterStripBusinessObjectTestCase
	{
		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestIsSelfManagedTariffCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				var bizObj = new RefCusTariffFilterStripBusinessObject();
				bizObj.QueryObjectType = typeof(TariffView);
				var isSystemDefinedFilter = (ModuleTextFilter)bizObj.ModuleFilters["Is System Defined"];
				AssertEquals("If the country is self managed, the 'System Defined' filter is always visible.", FilterVisibility.AlwaysVisible, isSystemDefinedFilter.Visibility);
				AssertEquals("If the country is self managed, the value of 'System Defined' is set to 'Not System'.", "Not System", isSystemDefinedFilter.Property);
			}
		}

		public void TestIsNotSelfManagedTariffCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				var bizObj = new RefCusTariffFilterStripBusinessObject();
				bizObj.QueryObjectType = typeof(TariffView);
				var isSystemDefinedFilter = (ModuleTextFilter)bizObj.ModuleFilters["Is System Defined"];
				AssertEquals("If the country is not self managed, the 'System Defined' filter is collapsed.", FilterVisibility.Visible, isSystemDefinedFilter.Visibility);
				AssertEquals("If the country is not self managed, the value of 'System Defined' is set by default.", "System", isSystemDefinedFilter.Property);
			}
		}

		public void TestShouldIncludeNomenclatureGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupingSKK = helper.CreateNewOrGetExistingDataGrouping("SKK");
			var dataGroupingER = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			dataGroupingER.ZZZ_ZZZ_Grouping = dataGroupingSKK.PK;
			Factory.Save();
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "1020304050", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariff2 = helper.CreateTariff("SKK", tariffType.PK, "2030405060", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			var bizObj = new RefCusTariffFilterStripBusinessObject();
			var moduleFilter = (ModuleNkFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.CountryOrGrouping];
			moduleFilter.IsActive = true;
			moduleFilter.Property = Core.Constants.CountryCodes.Eritrea;
			var filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(true, tariff2.MatchesFilter(filter));
			((INomenclatureEnabler)bizObj).Enable = false;
			filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(false, tariff2.MatchesFilter(filter));

			var codeFilter = (ModuleTextFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.TariffCode];
			codeFilter.IsActive = true;
			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			codeFilter.Property = "1020.";
			filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
		}

		public void TestEffectiveDateFilter_Visibility()
		{
			var bizObj = new RefCusTariffFilterStripBusinessObject();
			bizObj.QueryObjectType = typeof(TariffView);
			var effectiveDateFilter = (ModuleSingleDateFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.EffectiveDate];
			AssertEquals("AlwaysVisible", true, effectiveDateFilter.Visible);
		}

		public void TestEffectiveDateFilter_IncludeVersionFilter_ZZTariffHaveEmptyVersion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType1 = helper.CreateTariffType(Core.Constants.CountryCodes.Eritrea, "TS1");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType1.PK, "1020304051", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType1.PK, "1020304052", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(10));
			Factory.Save();

			var bizObj = new RefCusTariffFilterStripBusinessObject();
			bizObj.QueryObjectType = typeof(TariffView);
			var effectiveDateFilter = (ModuleSingleDateFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.EffectiveDate];
			effectiveDateFilter.IsActive = true;

			effectiveDateFilter.Property1 = ZDateTime.Today;
			var filter = bizObj.Filter;
			AssertEquals("Tariff1", true, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2", false, tariff2.MatchesFilter(filter));

			effectiveDateFilter.Property1 = ZDateTime.Today.AddDays(2);
			filter = bizObj.Filter;
			AssertEquals("Tariff1", false, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2", true, tariff2.MatchesFilter(filter));

			effectiveDateFilter.Property1 = ZDateTime.Invalid;
			filter = bizObj.Filter;
			AssertEquals("Tariff1: search all out for empty filter", true, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2: search all out for empty filter", true, tariff2.MatchesFilter(filter));
		}

		public void TestEffectiveDateFilter_IncludeVersionFilter_ManualTariffHaveVersion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var version1 = helper.CreateTariffVersion("CG2020", "CG2020", ZDateTime.Today.AddDays(-1).Date);
			version1.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Congo;
			var version2 = helper.CreateTariffVersion("CG2022", "CG2022", ZDateTime.Today.AddDays(5).Date);
			version2.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Congo;
			Factory.Save();
			var tariff1 = helper.CreateManualTariff(Core.Constants.CountryCodes.Congo, "HSN", "1020304051", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), tariffVersion: "CG2020");
			var tariff2 = helper.CreateManualTariff(Core.Constants.CountryCodes.Congo, "HSN", "1020304052", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(10), tariffVersion: "CG2020");
			var tariff3 = helper.CreateManualTariff(Core.Constants.CountryCodes.Congo, "HSN", "1020304052", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(10), tariffVersion: "CG2022");

			var bizObj = new RefCusTariffFilterStripBusinessObject();
			bizObj.QueryObjectType = typeof(TariffView);
			var effectiveDateFilter = (ModuleSingleDateFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.EffectiveDate];
			effectiveDateFilter.IsActive = true;

			effectiveDateFilter.Property1 = ZDateTime.Today;
			var filter = bizObj.Filter;
			AssertEquals("Tariff1", true, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2: tariff is not effective yet today", false, tariff2.MatchesFilter(filter));
			AssertEquals("Tariff3: tariff version is not effective yet today", false, tariff3.MatchesFilter(filter));

			effectiveDateFilter.Property1 = ZDateTime.Today.AddDays(2);
			filter = bizObj.Filter;
			AssertEquals("Tariff1: tariff is already expired at filter date", false, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2", true, tariff2.MatchesFilter(filter));
			AssertEquals("Tariff3: tariff version is not effective yet at filter date", false, tariff3.MatchesFilter(filter));

			effectiveDateFilter.Property1 = ZDateTime.Today.AddDays(5);
			filter = bizObj.Filter;
			AssertEquals("Tariff1: tariff is already expired at filter date", false, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2: the tariff version CG2020 is not effective as CG2022 takes into effect", false, tariff2.MatchesFilter(filter));
			AssertEquals("Tariff3: only the latest tariff version CG2022 is effective", true, tariff3.MatchesFilter(filter));

			effectiveDateFilter.Property1 = ZDateTime.Invalid;
			filter = bizObj.Filter;
			AssertEquals("Tariff1: search all out for empty filter", true, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2: search all out for empty filter", true, tariff2.MatchesFilter(filter));
			AssertEquals("Tariff3: search all out for empty filter", true, tariff3.MatchesFilter(filter));
		}

		public void TestCountryOrGroupingFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var er = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
				var za = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
				var tdg = helper.CreateNewOrGetExistingDataGrouping("TDG", parent: za);
				Factory.Save();
				var tariffType1 = helper.CreateTariffType(Core.Constants.CountryCodes.Eritrea, "TS1", ensureDataGroupingExists: false);
				var tariffType2 = helper.CreateTariffType(Core.Constants.CountryCodes.Eritrea, "TS2", ensureDataGroupingExists: false);
				var tariffType3 = helper.CreateTariffType("TDG", "TS2", ensureDataGroupingExists: false);
				Factory.Save();
				var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType1.PK, "1020304051", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), ensureDataGroupingExists: false);
				var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType2.PK, "1020304052", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), ensureDataGroupingExists: false);
				var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3.PK, "1020304053", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), ensureDataGroupingExists: false);
				Factory.Save();
				CombineAssertions(() =>
				{
					var bizObj = new RefCusTariffFilterStripBusinessObject();
					var countryOrGroupingFilter = (ModuleNkFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.CountryOrGrouping];
					countryOrGroupingFilter.IsActive = true;
					AssertEquals("countryOrGroupingFilter.Property", ZString.Empty, countryOrGroupingFilter.Property);
					AssertEquals("countryOrGroupingFilter.PropertyInfo.HasMessageErrors()", false, countryOrGroupingFilter.PropertyInfo.HasMessageErrors());
					AssertEquals("countryOrGroupingFilter.ModuleId", ModuleIDs.Customs.Universal.RefDataGrouping, countryOrGroupingFilter.ModuleId);
					AssertEquals("countryOrGroupingFilter.ForeignCodeColumnOverride", RefDataGroupingSchema.ZZZ_DataGrouping, countryOrGroupingFilter.ForeignCodeColumnOverride);
					AssertEquals("countryOrGroupingFilter.Category", FilterCategories.Other, countryOrGroupingFilter.Category);
					AssertEquals("countryOrGroupingFilter.Description", "Country/Region or Grouping", countryOrGroupingFilter.Description);
					AssertEquals("countryOrGroupingFilter.MultilingualDescription", "Country/Region or Grouping", countryOrGroupingFilter.MultilingualDescription);
					AssertEquals("countryOrGroupingFilter.Visibility", FilterVisibility.AlwaysVisible, countryOrGroupingFilter.Visibility);

					countryOrGroupingFilter.Property = "TDG";
					AssertEquals("countryOrGroupingFilter.PropertyInfo.HasMessageErrors", false, countryOrGroupingFilter.PropertyInfo.HasMessageErrors());

					var filter = bizObj.Filter;
					AssertEquals("Tariff1", false, tariff1.MatchesFilter(filter));
					AssertEquals("Tariff2", false, tariff2.MatchesFilter(filter));
					AssertEquals("Tariff3", true, tariff3.MatchesFilter(filter));

					countryOrGroupingFilter.Property = Core.Constants.CountryCodes.Eritrea;
					AssertEquals("countryOrGroupingFilter.PropertyInfo.HasMessageErrors", false, countryOrGroupingFilter.PropertyInfo.HasMessageErrors());

					filter = bizObj.Filter;
					AssertEquals("Tariff1", true, tariff1.MatchesFilter(filter));
					AssertEquals("Tariff2", true, tariff2.MatchesFilter(filter));
					AssertEquals("Tariff3", false, tariff3.MatchesFilter(filter));

					countryOrGroupingFilter.Property = Core.Constants.CountryCodes.SouthAfrica;
					AssertEquals("countryOrGroupingFilter.PropertyInfo.HasMessageErrors", false, countryOrGroupingFilter.PropertyInfo.HasMessageErrors());

					filter = bizObj.Filter;
					AssertEquals("Tariff1", false, tariff1.MatchesFilter(filter));
					AssertEquals("Tariff2", false, tariff2.MatchesFilter(filter));
					AssertEquals("Tariff3", true, tariff3.MatchesFilter(filter));
				});
			}
		}

		public void TestCountryOrGrouping_IncludeVersionFilter_ManualTariffHaveVersion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Congo);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Namibia);
			var version1 = helper.CreateTariffVersion("CG2020", "CG2020", ZDateTime.Today.AddDays(-1).Date);
			version1.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Congo;
			var version2 = helper.CreateTariffVersion("CG2022", "CG2022", ZDateTime.Today.AddDays(5).Date);
			version2.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Congo;
			var version3 = helper.CreateTariffVersion("NA2020", "NA2020", ZDateTime.Today.AddDays(-1).Date);
			version3.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Namibia;
			var version4 = helper.CreateTariffVersion("NA2022", "NA2022", ZDateTime.Today.AddDays(5).Date);
			version4.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Namibia;
			Factory.Save();

			var tariff1 = helper.CreateManualTariff(Core.Constants.CountryCodes.Congo, "HSN", "1020304052", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(10), tariffVersion: "CG2020");
			var tariff2 = helper.CreateManualTariff(Core.Constants.CountryCodes.Congo, "HSN", "1020304052", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(10), tariffVersion: "CG2022");
			var tariff3 = helper.CreateManualTariff(Core.Constants.CountryCodes.Namibia, "HSN", "1020304051", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(10), tariffVersion: "NA2020");
			var tariff4 = helper.CreateManualTariff(Core.Constants.CountryCodes.Namibia, "HSN", "1020304051", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(10), tariffVersion: "NA2022");

			var bizObj = new RefCusTariffFilterStripBusinessObject();
			bizObj.QueryObjectType = typeof(TariffView);
			var countryOrGroupingFilter = (ModuleNkFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.CountryOrGrouping];
			countryOrGroupingFilter.IsActive = true;

			countryOrGroupingFilter.Property = Core.Constants.CountryCodes.Congo;
			var filter = bizObj.Filter;
			AssertEquals("Tariff1: Congo filter", true, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2: Congo filter", true, tariff2.MatchesFilter(filter));
			AssertEquals("Tariff3: tariff country is Namibia", false, tariff3.MatchesFilter(filter));
			AssertEquals("Tariff4: tariff country is Namibia", false, tariff4.MatchesFilter(filter));

			countryOrGroupingFilter.Property = Core.Constants.CountryCodes.Namibia;
			filter = bizObj.Filter;
			AssertEquals("Tariff1: tariff country is Congo", false, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2: tariff country is Congo", false, tariff2.MatchesFilter(filter));
			AssertEquals("Tariff3: Namibia filter", true, tariff3.MatchesFilter(filter));
			AssertEquals("Tariff4: Namibia filter: ", true, tariff4.MatchesFilter(filter));

			countryOrGroupingFilter.Property = ZString.Empty;
			filter = bizObj.Filter;
			AssertEquals("Tariff1: search all out for empty filter", true, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2: search all out for empty filter", true, tariff2.MatchesFilter(filter));
			AssertEquals("Tariff3: search all out for empty filter", true, tariff3.MatchesFilter(filter));
			AssertEquals("Tariff4: search all out for empty filter", true, tariff4.MatchesFilter(filter));
		}

		public void TestCountryOrGrouping_IncludeVersionFilter_ZZTariffHaveEmptyVersion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType1 = helper.CreateTariffType(Core.Constants.CountryCodes.Eritrea, "TS1");
			var tariffType2 = helper.CreateTariffType(Core.Constants.CountryCodes.SouthAfrica, "TS2");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType1.PK, "1020304051", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2.PK, "1020304052", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var bizObj = new RefCusTariffFilterStripBusinessObject();
			bizObj.QueryObjectType = typeof(TariffView);
			var countryOrGroupingFilter = (ModuleNkFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.CountryOrGrouping];
			countryOrGroupingFilter.IsActive = true;

			countryOrGroupingFilter.Property = Core.Constants.CountryCodes.Eritrea;
			var filter = bizObj.Filter;
			AssertEquals("Tariff1: Eritrea", true, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2: SouthAfrica", false, tariff2.MatchesFilter(filter));

			countryOrGroupingFilter.Property = Core.Constants.CountryCodes.SouthAfrica;
			filter = bizObj.Filter;
			AssertEquals("Tariff1: Eritrea", false, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2: SouthAfrica", true, tariff2.MatchesFilter(filter));

			countryOrGroupingFilter.Property = ZString.Empty;
			filter = bizObj.Filter;
			AssertEquals("Tariff1: Eritrea", true, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2: SouthAfrica", true, tariff2.MatchesFilter(filter));
		}

		public void TestTariffTypeFilter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TS1");
			var tariffType2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TS2");
			var tariffType3 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "TS2");
			var tariffType4 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "AB1");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType1.PK, "1020304051", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType2.PK, "1020304052", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3.PK, "1020304053", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2.PK, "1020304054", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariff5 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType4.PK, "1020304055", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			var tariff6 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Eritrea, tariffType1.PK, "1020304056", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), isSystem: false, description: "Desc");
			Factory.Save();
			CombineAssertions(() =>
			{
				var bizObj = new RefCusTariffFilterStripBusinessObject();
				var tariffTypeFilter = (DataGroupingRelatedFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.TariffType];
				tariffTypeFilter.IsActive = true;
				tariffTypeFilter.Property1 = Core.Constants.CountryCodes.Eritrea;
				tariffTypeFilter.Property2 = "T"; // StartsWith Match
				AssertEquals("tariffTypeFilter.Property2FieldType", ZArchitecture.FieldType.TextDropEdit, tariffTypeFilter.Property2FieldType);
				AssertEquals("tariffTypeFilter.Property2MaxLength", TariffViewSchema.ZZ1_ZZI_TariffType.MaxLength, tariffTypeFilter.Property2MaxLength);
				AssertEquals("tariffTypeFilter.Property2ResourceString", "Tariff Type", tariffTypeFilter.Property2ResourceString.Caption);

				AssertEquals("tariffTypeFilter.Category", FilterCategories.ModesAndTypes, tariffTypeFilter.Category);
				AssertEquals("tariffTypeFilter.Description", "Tariff Type", tariffTypeFilter.Description);
				AssertEquals("tariffTypeFilter.MultilingualDescription", "Tariff Type", tariffTypeFilter.MultilingualDescription);

				var filter = bizObj.Filter;
				AssertEquals("Tariff1", true, tariff1.MatchesFilter(filter));
				AssertEquals("Tariff2", true, tariff2.MatchesFilter(filter));
				AssertEquals("Tariff3", false, tariff3.MatchesFilter(filter));
				AssertEquals("Tariff4", false, tariff4.MatchesFilter(filter));
				AssertEquals("Tariff5", false, tariff5.MatchesFilter(filter));

				AssertEquals("Tariff6", true, tariff6.MatchesFilter(filter));

				var tariffTypeFilter2 = (DataGroupingRelatedFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.TariffType];
				tariffTypeFilter2.IsActive = true;
				tariffTypeFilter2.Property1 = Core.Constants.CountryCodes.Eritrea;
				tariffTypeFilter2.Property2 = "T"; // StartsWith Match
				AssertSame("Property2List Cached", tariffTypeFilter.Property2List, tariffTypeFilter2.Property2List);
			});
		}

		public void TestUnitOfQuantityFilters()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "1020304051", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(tariff1, Constants.UnitOfMeasureTypes.StatisticalUOMType, "A1");
			helper.CreateTariffUOM(tariff1, Constants.UnitOfMeasureTypes.AdditionalUOMType, "B1");
			helper.CreateTariffUOM(tariff1, Constants.UnitOfMeasureTypes.CustomsUOM3Type, "C1");
			helper.CreateTariffUOM(tariff1, "RU1", "D1");
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "1020304052", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(tariff2, Constants.UnitOfMeasureTypes.StatisticalUOMType, "A12");
			helper.CreateTariffUOM(tariff2, Constants.UnitOfMeasureTypes.CustomsUOM3Type, "C2");
			helper.CreateTariffUOM(tariff2, "RU1", "D1");
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "1020304053", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(tariff3, Constants.UnitOfMeasureTypes.StatisticalUOMType, "A1");
			helper.CreateTariffUOM(tariff3, Constants.UnitOfMeasureTypes.AdditionalUOMType, "B2");
			helper.CreateTariffUOM(tariff3, "RU1", "D1");
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "1020304054", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(tariff4, Constants.UnitOfMeasureTypes.StatisticalUOMType, "A2");
			helper.CreateTariffUOM(tariff4, Constants.UnitOfMeasureTypes.CustomsUOM3Type, "C1");
			helper.CreateTariffUOM(tariff4, "RU1", "D1");
			var tariff5 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "1020304055", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(tariff5, Constants.UnitOfMeasureTypes.AdditionalUOMType, "B1");
			helper.CreateTariffUOM(tariff5, "RU1", "D1");
			Factory.Save();
			var bizObj = new RefCusTariffFilterStripBusinessObject();
			var countryFilter = (ModuleNkFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.CountryOrGrouping];
			countryFilter.Property = Core.Constants.CountryCodes.Eritrea;
			var uq1Filter = (ModuleTextFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.UnitOfQuantity1];
			uq1Filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank;
			uq1Filter.IsActive = true;

			var filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(true, tariff2.MatchesFilter(filter));
			AssertEquals(true, tariff3.MatchesFilter(filter));
			AssertEquals(true, tariff4.MatchesFilter(filter));
			AssertEquals(false, tariff5.MatchesFilter(filter));

			uq1Filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			uq1Filter.Property = "A";
			filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(true, tariff2.MatchesFilter(filter));
			AssertEquals(true, tariff3.MatchesFilter(filter));
			AssertEquals(true, tariff4.MatchesFilter(filter));
			AssertEquals(false, tariff5.MatchesFilter(filter));

			uq1Filter.Property = "A1";
			filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(true, tariff2.MatchesFilter(filter));
			AssertEquals(true, tariff3.MatchesFilter(filter));
			AssertEquals(false, tariff4.MatchesFilter(filter));
			AssertEquals(false, tariff5.MatchesFilter(filter));

			uq1Filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(false, tariff2.MatchesFilter(filter));
			AssertEquals(true, tariff3.MatchesFilter(filter));
			AssertEquals(false, tariff4.MatchesFilter(filter));
			AssertEquals(false, tariff5.MatchesFilter(filter));

			uq1Filter.Property = "";
			uq1Filter.IsActive = false;
			var uq2Filter = (ModuleTextFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.UnitOfQuantity2];
			uq2Filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank;
			uq2Filter.IsActive = true;
			filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(false, tariff2.MatchesFilter(filter));
			AssertEquals(true, tariff3.MatchesFilter(filter));
			AssertEquals(false, tariff4.MatchesFilter(filter));
			AssertEquals(true, tariff5.MatchesFilter(filter));

			uq2Filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			uq2Filter.Property = "B1";
			filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(false, tariff2.MatchesFilter(filter));
			AssertEquals(false, tariff3.MatchesFilter(filter));
			AssertEquals(false, tariff4.MatchesFilter(filter));
			AssertEquals(true, tariff5.MatchesFilter(filter));

			uq2Filter.Property = "";
			uq2Filter.IsActive = false;
			var uq3Filter = (ModuleTextFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.UnitOfQuantity3];
			uq3Filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank;
			uq3Filter.IsActive = true;
			filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(true, tariff2.MatchesFilter(filter));
			AssertEquals(false, tariff3.MatchesFilter(filter));
			AssertEquals(true, tariff4.MatchesFilter(filter));
			AssertEquals(false, tariff5.MatchesFilter(filter));

			uq3Filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			uq3Filter.Property = "C1";
			filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(false, tariff2.MatchesFilter(filter));
			AssertEquals(false, tariff3.MatchesFilter(filter));
			AssertEquals(true, tariff4.MatchesFilter(filter));
			AssertEquals(false, tariff5.MatchesFilter(filter));
		}

		public void TestDescriptionFilters()
		{
			var jaBranch = CreateCompanyWithBranch(Core.Constants.CountryCodes.Japan);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, jaBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateOrGetLanguage("ZHT", "Traditional Chinese");
				helper.CreateOrGetLanguage("JP", "Japanese");
				var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "EN");
				Factory.Save();
				var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "1020304051", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

				var zhtLanguage = helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariff.PK, "ZHT", "預設描述");
				var jpLanguage = helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariff.PK, "JP", "デフォルトの説明");
				Factory.Save();

				var bizObj = new RefCusTariffFilterStripBusinessObject();
				var defaultLanguageDescriptionFilter = (ModuleTextFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.DefaultLanguageDescription];
				defaultLanguageDescriptionFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				defaultLanguageDescriptionFilter.IsActive = true;

				var orginalLanguage = GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage];
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = "ZH-TW";
				defaultLanguageDescriptionFilter.Property = "預設描述";
				AssertEquals(false, tariff.MatchesFilter(bizObj.Filter));

				defaultLanguageDescriptionFilter.Property = "デフォルトの説明";
				AssertEquals(false, tariff.MatchesFilter(bizObj.Filter));

				defaultLanguageDescriptionFilter.Property = "Default Description";
				AssertEquals(true, tariff.MatchesFilter(bizObj.Filter));

				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = "JP";
				defaultLanguageDescriptionFilter.Property = "預設描述";
				AssertEquals(false, tariff.MatchesFilter(bizObj.Filter));

				defaultLanguageDescriptionFilter.Property = "デフォルトの説明";
				AssertEquals(false, tariff.MatchesFilter(bizObj.Filter));

				defaultLanguageDescriptionFilter.Property = "Default Description";
				AssertEquals(true, tariff.MatchesFilter(bizObj.Filter));

				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = "EN";
				defaultLanguageDescriptionFilter.Property = "預設描述";
				AssertEquals(false, tariff.MatchesFilter(bizObj.Filter));

				defaultLanguageDescriptionFilter.Property = "デフォルトの説明";
				AssertEquals(false, tariff.MatchesFilter(bizObj.Filter));

				defaultLanguageDescriptionFilter.Property = "Default Description";
				AssertEquals(true, tariff.MatchesFilter(bizObj.Filter));

				var alternateLanguageDescriptionFilter = (ModuleTextFilter)bizObj.ModuleFilters[Constants.RefCusTariffFilters.AlternateLanguageDescription];
				alternateLanguageDescriptionFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				alternateLanguageDescriptionFilter.IsActive = true;

				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = "ZH-TW";
				alternateLanguageDescriptionFilter.Property = "預設描述";
				AssertEquals(true, tariff.MatchesFilter(bizObj.Filter));

				alternateLanguageDescriptionFilter.Property = "デフォルトの説明";
				AssertEquals(true, tariff.MatchesFilter(bizObj.Filter));

				alternateLanguageDescriptionFilter.Property = "Default Description";
				AssertEquals(false, tariff.MatchesFilter(bizObj.Filter));

				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = "JA-JP";
				alternateLanguageDescriptionFilter.Property = "預設描述";
				AssertEquals(false, tariff.MatchesFilter(bizObj.Filter));

				alternateLanguageDescriptionFilter.Property = "デフォルトの説明";
				AssertEquals(true, tariff.MatchesFilter(bizObj.Filter));

				alternateLanguageDescriptionFilter.Property = "Default Description";
				AssertEquals(false, tariff.MatchesFilter(bizObj.Filter));

				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = "EN";
				alternateLanguageDescriptionFilter.Property = "預設描述";
				AssertEquals(false, tariff.MatchesFilter(bizObj.Filter));

				alternateLanguageDescriptionFilter.Property = "デフォルトの説明";
				AssertEquals(true, tariff.MatchesFilter(bizObj.Filter));

				alternateLanguageDescriptionFilter.Property = "Default Description";
				AssertEquals(false, tariff.MatchesFilter(bizObj.Filter));
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = orginalLanguage;
			}
		}

		public void TestAttributeNameFilter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "TS1");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1.PK, "TRF1", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var attName1 = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, "TEST1", "TEST1V1", "TEST1", Core.Constants.CountryCodes.SouthAfrica, tariffType1.ZZI_TariffType);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1.PK, "TRF2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var attName2 = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, "TEST1", "TEST1V1", "TEST1", Core.Constants.CountryCodes.SouthAfrica, tariffType1.ZZI_TariffType);
			Factory.Save();
			var attr1 = UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAttribute(Factory, "TEST1", "ABC", false, tariff1.PK);
			var attr2 = UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAttribute(Factory, "TEST1", "DEF", false, tariff2.PK);

			Factory.Save();

			var collection = ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, "TS1", ZDateTime.Today, new[] { new KeyValuePair<ZString, ZString>() });
			var filterStrip = new RefCusTariffFilterStripBusinessObject(collection);
			var attributeNameFilter = (ModuleTextFilter)filterStrip["TEST1"];
			attributeNameFilter.IsActive = true;
			attributeNameFilter.Property = "ABC";
			var filter = filterStrip.Filter;
			AssertEquals("Tariff1", true, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2", false, tariff2.MatchesFilter(filter));

			attributeNameFilter.Property = "DEF";
			filter = filterStrip.Filter;
			AssertEquals("Tariff1", false, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2", true, tariff2.MatchesFilter(filter));

			attributeNameFilter.Property = "GHI";
			filter = filterStrip.Filter;
			AssertEquals("Tariff1", false, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff2", false, tariff2.MatchesFilter(filter));
		}

		GlbBranch CreateCompanyWithBranch(ZString companyCountryCode)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = companyCountryCode;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			Factory.Save();

			return branch;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefCusTariffFilterStripBusinessObject();
		}
	}
}
