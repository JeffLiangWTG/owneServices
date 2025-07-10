using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using C = Enterprise.Core.Constants.Customs.Universal;
using ZZRefCusCodeListFilters = Enterprise.Customs.Universal.Constants.ZZRefCusCodeListFilters;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusCodeListFilterStripBusinessObject))]
	public class ZZRefCusCodeListFilterStripBusinessObjectTests : ZArchitecture.Modules.Testing.FilterStripBusinessObjectTestCase
	{
		public void TestDefaultingOfCountryFilterShouldTakeCareOfJurisdictionCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Martinique))
			{
				var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
				var countryFilter = (ModuleNkFilter)filterStrip[ZZRefCusCodeListFilters.CountryOrGrouping];
				AssertEquals("Default country filter should be the Customs Jurisdiction Country if any.", Core.Constants.CountryCodes.France, countryFilter.Property);
			}
		}

		public void TestCountryOrGroupingQueryShouldIncludeParentDataGroupingIfNeeded()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string belgiumCountryCode = Core.Constants.CountryCodes.Belgium;
			const string germanyCountryCode = Core.Constants.CountryCodes.Germany;
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(C.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(belgiumCountryCode, "Belgium", parentDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(germanyCountryCode, "Germany", parentDataGrouping);
			Factory.Save();

			var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, belgiumCountryCode, "TST", ZDateTime.Today);
			AssertEquals("Precondition: ", true, collection.IncludeParentDataGroupings);
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			var countryFilter = (ModuleNkFilter)filterStrip[ZZRefCusCodeListFilters.CountryOrGrouping];
			countryFilter.IsActive = true;
			countryFilter.Property = belgiumCountryCode;
			AssertEquals("If ZZRefCusCodeListCombinedCollection.IncludeParentDataGroupings = IncludeParentDataGroupingOptions.Union then filter should include parents.",
				MasterFiles.Business.RefDataGrouping.GetQueryIncludeParentDataGrouping(Factory, ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, belgiumCountryCode).LiteralTextSqlFormatted,
				filterStrip.Filter.LiteralTextSqlFormatted);

			collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, germanyCountryCode, new ZString[] { "TST" }, ZDateTime.Today, null, RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildOnly);
			AssertEquals("Precondition: ", false, collection.IncludeParentDataGroupings);
			filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			countryFilter = (ModuleNkFilter)filterStrip[ZZRefCusCodeListFilters.CountryOrGrouping];
			countryFilter.IsActive = true;
			countryFilter.Property = germanyCountryCode;
			AssertEquals("If ZZRefCusCodeListCombinedCollection.IncludeParentDataGroupings = IncludeParentDataGroupingOptions.ChildOnly then filter should not include parents.",
				new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, germanyCountryCode).LiteralTextSqlFormatted,
				filterStrip.Filter.LiteralTextSqlFormatted);
		}

		[ExpectNoExceptions]
		public void TestDynamicFilters_ValueDateTypeCaseInsensitive()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string dataGrouping = Core.Constants.CountryCodes.China;
			helper.CreateCusCodeType("TP1", "Ref Code Type 1", dataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "String Attribute Name 1", "TP1", dataGrouping, "", isValueMandatory: true);
			attributeName.ZXE_ColumnCaption = "String Attribute Name 1";
			const string stringType = Constants.RefCusCodeListAttributeName.ValueDataTypes.String;
			attributeName.ZXE_ValueDataType = stringType.Substring(0, 1).ToLower() + stringType.Substring(1);
			attributeName.ZXE_MaxLengthOrValue = 5;
			Factory.Save();

			var collection = new ZZRefCusCodeListCombinedCollection(Factory, dataGrouping, "TP1", ZDateTime.Today);
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			var filter1 = (ModuleTextFilter)filterStrip["String Attribute Name 1"];
			filter1.IsActive = true;
			filter1.Property = "VAL1";

			_ = filterStrip.Filter;
		}

		public void TestDynamicFilters_HandleEmptyLanguagueTranslation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var languageCode = TranslationHelper.GetCurrentLanguageCode();
			helper.CreateOrGetLanguage(languageCode, "Test");
			const string dataGrouping = Core.Constants.CountryCodes.China;
			helper.CreateCusCodeType("TP1", "Ref Code Type 1", dataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "Attribute Name 1", "TP1", dataGrouping, isValueMandatory: true);
			var attributeName1Language = helper.CreateCusCodeListAttributeNameLanguage(attributeName1, languageCode, "CH ATT1", "CH Ref Code Type 1 Description", "CH Caption ATT1");
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT2", "Attribute Name 2", "TP1", dataGrouping, isValueMandatory: true);
			var attributeName2Language = helper.CreateCusCodeListAttributeNameLanguage(attributeName2, languageCode, "CH ATT2", "CH Ref Code Type 2 Description", ZString.Empty);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT3", "Attribute Name 3", "TP1", dataGrouping, isValueMandatory: true);
			Factory.Save();

			var collection = new ZZRefCusCodeListCombinedCollection(new BusinessObjectFactory(), dataGrouping, "TP1", ZDateTime.Today);
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			AssertNull("ATT1", filterStrip["ATT1"]);
			AssertNotNull("CH Caption ATT1", filterStrip["CH Caption ATT1"]);
			AssertNotNull("ATT2", filterStrip["ATT2"]);
			AssertNotNull("ATT3", filterStrip["ATT3"]);
		}

		public void TestGetDataGrouping()
		{
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, "DJC", "TP1", ZDateTime.Today);
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			CombineAssertions(() =>
			{
				var filterDJC = filterStrip.ModuleFilters.FirstOrDefault(f => f.Query.LiteralTextSqlFormatted.Contains("ZZD_CountryOrGrouping = 'DJC'"));
				AssertNotNull(filterDJC);
				AssertEquals(ZArchitecture.Business.FilterOrCategory.None, filterDJC.OrCategory);
			});
		}

		public void TestGetDataGroupings()
		{
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, new ZString[] { "DJC", "PVE" }, new ZString[] { "TP1" }, ZDateTime.Today, null);
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);

			CombineAssertions(() =>
			{
				var filterDJC = filterStrip.ModuleFilters.FirstOrDefault(f => f.Query.LiteralTextSqlFormatted.Contains("ZZD_CountryOrGrouping = 'DJC'"));
				AssertNotNull(filterDJC);
				AssertEquals(ZArchitecture.Business.FilterOrCategory.Green, filterDJC.OrCategory);
				var filterPVE = filterStrip.ModuleFilters.FirstOrDefault(f => f.Query.LiteralTextSqlFormatted.Contains("ZZD_CountryOrGrouping = 'PVE'"));
				AssertNotNull(filterPVE);
				AssertEquals(ZArchitecture.Business.FilterOrCategory.Green, filterPVE.OrCategory);
			});
		}

		public void TestEmptyDataGroupingFilterAdded_SingleDataGrouping()
		{
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.China, "CT1", ZDateTime.Today, Array.Empty<RefCusCodeListAttributeFilter>());
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);

			CombineAssertions(() =>
			{
				var countryGroupingFilter = filterStrip.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith(Constants.ZZRefCusCodeListFilters.CountryOrGrouping)) as ModuleNkFilter;
				AssertNotNull(countryGroupingFilter);
				AssertEquals("Grouping Defaulted from Current Company", "AU", countryGroupingFilter.Property);
			});
		}

		public void TestEmptyDataGroupingFilterAdded_MultipleDataGroupings()
		{
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, new ZString[] { Core.Constants.CountryCodes.China, Core.Constants.CountryCodes.Germany }, "CT1", ZDateTime.Today, Array.Empty<RefCusCodeListAttributeFilter>());
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);

			CombineAssertions(() =>
			{
				var countryGroupingFilter = filterStrip.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith(Constants.ZZRefCusCodeListFilters.CountryOrGrouping)) as ModuleNkFilter;
				AssertNotNull(countryGroupingFilter);
				AssertEquals("Grouping Defaulted from Current Company", "AU", countryGroupingFilter.Property);
			});
		}

		public void TestGetDataGrouping_NoCollection()
		{
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			var filterDefaultCountry = filterStrip.ModuleFilters.FirstOrDefault(f => f.Query.LiteralTextSqlFormatted.Contains("ZZD_CountryOrGrouping = 'AU'"));

			CombineAssertions(() =>
			{
				AssertNotNull(filterDefaultCountry);
				AssertEquals(ZArchitecture.Business.FilterOrCategory.None, filterDefaultCountry.OrCategory);
			});
		}

		public void TestDynamicFilters()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TP1", "Ref Code Type 1");
			helper.CreateCusCodeType("TP2", "Ref Code Type 2");
			var dataGrouping = Core.Constants.CountryCodes.China;
			var attrName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "String Attribute Name 1", "TP1", dataGrouping, "", isValueMandatory: true);
			attrName1.ZXE_ColumnCaption = "String Attribute Name 1";
			attrName1.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.String;
			attrName1.ZXE_MaxLengthOrValue = 5;
			var attrName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT2", "String Attribute Name 2", "TP1", dataGrouping, "TP2", isValueMandatory: true);
			attrName2.ZXE_ColumnCaption = "String Attribute Name 2";
			attrName2.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.String;
			attrName2.ZXE_ZZK_NKCodeTypeForValueList = "TP2";
			var attrName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT3", "Boolean Attribute Name 3", "TP1", dataGrouping, "", isValueMandatory: true);
			attrName3.ZXE_ColumnCaption = "Boolean Attribute Name 3";
			attrName3.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean;
			var attrName4 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT4", "Integer Attribute Name 4", "TP1", dataGrouping, "", isValueMandatory: true);
			attrName4.ZXE_ColumnCaption = "Integer Attribute Name 4";
			attrName4.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer;
			attrName4.ZXE_MinLengthOrValue = 100;
			attrName4.ZXE_MaxLengthOrValue = 200;
			var attrName5 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT5", "Decimal Attribute Name 5", "TP1", dataGrouping, "", isValueMandatory: true);
			attrName5.ZXE_ColumnCaption = "Decimal Attribute Name 5";
			attrName5.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal;
			attrName5.ZXE_DecimalPlaces = 3;
			attrName5.ZXE_MinLengthOrValue = 2;
			attrName5.ZXE_MaxLengthOrValue = 10.1;

			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TP1", "CD1", "CD1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList1.PK, "ATT1", "VAL1", false);
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TP1", "CD2", "CD2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList2.PK, "ATT2", "VAL2", false);
			helper.CreateCusCodeListAttribute(codeList2.PK, "OTH1", "VAL1", true);
			var codeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TP1", "CD3", "CD3", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList3.PK, "ATT3", "Y", false);
			var codeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TP1", "CD4", "CD4", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList4.PK, "ATT4", "123", false);
			var codeList5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TP1", "CD5", "CD5", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList5.PK, "ATT5", "123.45", false);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TP2", "A", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TP2", "B", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();

			var collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.China, "TP1", ZDateTime.Today);
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			var filter1 = (ModuleTextFilter)filterStrip["String Attribute Name 1"];
			filter1.IsActive = true;
			filter1.Property = "VAL1";
			AssertEquals(5, filter1.MaxLength);
			AssertEquals(FilterCategories.TextSearch, filter1.Category);
			AssertEquals(true, codeList1.MatchesFilter(filterStrip.Filter));
			AssertEquals(false, codeList2.MatchesFilter(filterStrip.Filter));

			filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			var filter2 = (ModuleFilterWithListAndComparisonOperators<ZString>)filterStrip["String Attribute Name 2"];
			filter2.IsActive = true;
			filter2.Property = "VAL2";
			AssertEquals(FilterCategories.TextSearch, filter2.Category);
			AssertEquals(false, codeList1.MatchesFilter(filterStrip.Filter));
			AssertEquals(true, codeList2.MatchesFilter(filterStrip.Filter));
			AssertEquals(typeof(CodeDescriptionPairList), filter2.List.GetType());
			AssertEquals("A, B", ((CodeDescriptionPairList)filter2.List).CodesAsString);

			filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			var filter3 = (ModuleFlagsFilter)filterStrip["Boolean Attribute Name 3"];
			filter3.IsActive = true;
			filter3.Property0 = true;
			AssertEquals(FilterCategories.StatusAndFlags, filter3.Category);
			AssertEquals(false, codeList2.MatchesFilter(filterStrip.Filter));
			AssertEquals(true, codeList3.MatchesFilter(filterStrip.Filter));

			filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			var filter4 = (ModuleNumberRangeFilter)filterStrip["Integer Attribute Name 4"];
			filter4.IsActive = true;
			filter4.Property1 = 122;
			filter4.Property2 = 124;
			AssertEquals(ZByte.Zero, filter4.Decimals);
			AssertEquals((ZDecimal)100, filter4.MinValue);
			AssertEquals((ZDecimal)200, filter4.MaxValue);
			AssertEquals(FilterCategories.NumbersAndReferences, filter4.Category);
			AssertEquals(false, codeList3.MatchesFilter(filterStrip.Filter));
			AssertEquals(true, codeList4.MatchesFilter(filterStrip.Filter));

			filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			var filter5 = (ModuleNumberRangeFilter)filterStrip["Decimal Attribute Name 5"];
			filter5.IsActive = true;
			filter5.Property1 = 123.44;
			filter5.Property2 = 123.46;
			AssertEquals((byte)3, filter5.Decimals);
			AssertEquals((ZDecimal)2, filter5.MinValue);
			AssertEquals((ZDecimal)10.1, filter5.MaxValue);
			AssertEquals(FilterCategories.NumbersAndReferences, filter5.Category);
			AssertEquals(false, codeList4.MatchesFilter(filterStrip.Filter));
			AssertEquals(true, codeList5.MatchesFilter(filterStrip.Filter));
		}

		public void TestDynamicFilters_AllowColumnCaptionsWithSameNameAsOtherFilters()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TP1", "Ref Code Type 1");
			var dataGrouping = Core.Constants.CountryCodes.China;
			var attrName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "Description Attribute", "TP1", dataGrouping, "", isValueMandatory: true);
			attrName1.ZXE_ColumnCaption = "Description";
			attrName1.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.String;
			attrName1.ZXE_MaxLengthOrValue = 5;

			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TP1", "CD1", "CD1 DESCRIPTION", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList1.PK, "ATT1", "VAL1", false);

			Factory.Save();

			var collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.China, "TP1", ZDateTime.Today);
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			AssertNotNull("Filter name for ZZD_Description", filterStrip["Description"]);
			AssertNotNull("Filter name for Attribute ATT1 with Column Caption 'Description'", filterStrip["Description - Attribute"]);
		}

		public void TestDynamicFiltersOnlyAppliesToOneCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TP1", "Ref Code Type 1");
			helper.CreateCusCodeType("TP2", "Ref Code Type 2");
			var dataGrouping = Core.Constants.CountryCodes.China;
			var attrName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "String Attribute Name 1", "TP1", dataGrouping, "", isValueMandatory: true);
			attrName1.ZXE_ColumnCaption = "String Attribute Name 1";
			attrName1.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.String;
			attrName1.ZXE_MaxLengthOrValue = 5;

			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TP1", "CD1", "CD1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList1.PK, "ATT1", "VAL1", false);

			Factory.Save();

			var collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.China, "TP1", ZDateTime.Today);
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			AssertNotNull(filterStrip["String Attribute Name 1"]);

			collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.China, new ZString[] { "TP1", "TP2" }, ZDateTime.Today, null);
			filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			AssertNull(filterStrip["String Attribute Name 1"]);
		}

		public void TestDynamicFilters_ShouldReportError_WhenColumnCaptionAlreadyExists()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var attr1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "Attribute 1", "TP1", Core.Constants.CountryCodes.Belgium);
			attr1.ZXE_ColumnCaption = "Attribute 1";
			attr1.ZXE_IsValueMandatory = true;

			var attr2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT2", "Attribute 2", "TP1", Core.Constants.CountryCodes.Belgium);
			attr2.ZXE_ColumnCaption = "Attribute 1";
			attr2.ZXE_IsValueMandatory = true;

			var collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.Belgium, "TP1", ZDateTime.Today);

			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("[PreCondition]: No error should be reported", ErrorReporter.LastMessageReported);

				AssertNoExceptionThrown(() => filterStrip.GetModuleFilters());
				AssertEquals("An error should be reported with the key", "Duplicate ZXE_ColumnCaption='Attribute 1'", ErrorReporter.LastKeyReported);
				AssertStartsWith("An error should be reported", "ZXE_ZZZ_NKDataGrouping='BE', ZXE_ZZK_NKCodeType='TP1'", ErrorReporter.LastMessageReported);
				AssertContains("An error should be reported", "Filter: ZZD_CountryOrGrouping = 'BE'", ErrorReporter.LastMessageReported);
			});

			ErrorReporter.Clear();
		}

		public void TestSystemFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			filterStrip.QueryObjectType = typeof(ZZRefCusCodeListCombined);
			var systemFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.System];
			systemFilter.IsActive = true;
			systemFilter.Property = "System";
			var filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			systemFilter.Property = "Not System";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));

			systemFilter.Property = "All";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));
		}

		public void TestAttributeValueFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			var attributeNameFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.AttributeValue];
			attributeNameFilter.IsActive = true;
			attributeNameFilter.Property = "123";
			var filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));

			attributeNameFilter.Property = "456";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));

			attributeNameFilter.Property = "789";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));
		}

		public void TestAttributeNameFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			var attributeNameFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.AttributeName];
			attributeNameFilter.IsActive = true;
			attributeNameFilter.Property = "ABC";
			var filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));

			attributeNameFilter.Property = "DEF";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			attributeNameFilter.Property = "GHI";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			attributeNameFilter.Property = "JKL";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));

			attributeNameFilter.Property = "DSK";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			var list = (ICodeDescriptionPairList)attributeNameFilter.List;
			AssertEquals("list.Count", true, list.Count >= 4);
			AssertEquals("ABC", list.GetDescriptionFromCode("ABC"));
			AssertEquals("DEF", list.GetDescriptionFromCode("DEF"));
			AssertEquals("GHI", list.GetDescriptionFromCode("GHI"));
			AssertEquals("JKL", list.GetDescriptionFromCode("JKL"));
			AssertEquals("MNO", list.GetDescriptionFromCode("MNO"));

			var countryFilter = (ModuleNkFilter)filterStrip[ZZRefCusCodeListFilters.CountryOrGrouping];
			countryFilter.IsActive = true;
			countryFilter.Property = Core.Constants.CountryCodes.Eritrea;
			list = (ICodeDescriptionPairList)attributeNameFilter.List;
			AssertEquals("list.Count", 4, list.Count);
			AssertEquals("ABC", list.GetDescriptionFromCode("ABC"));
			AssertEquals("DEF", list.GetDescriptionFromCode("DEF"));
			AssertEquals("MNO", list.GetDescriptionFromCode("MNO"));
			AssertEquals("JKL", list.GetDescriptionFromCode("JKL"));

			countryFilter.IsActive = false;
			var listTypeFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.ListType];
			listTypeFilter.IsActive = true;
			listTypeFilter.Property = "Z#@";
			list = (ICodeDescriptionPairList)attributeNameFilter.List;
			AssertEquals("list.Count", 1, list.Count);
			AssertEquals("MNO", list.GetDescriptionFromCode("MNO"));
		}

		public void TestListTypeShouldIncludeParentDataGroupingIfNeeded()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string belgiumCountryCode = Core.Constants.CountryCodes.Belgium;
			const string germanyCountryCode = Core.Constants.CountryCodes.Germany;
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(C.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(belgiumCountryCode, "Belgium", parentDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(germanyCountryCode, "Germany", parentDataGrouping);
			helper.CreateNewOrGetExistingCusCodeType("AAA", "DESC", belgiumCountryCode);
			helper.CreateNewOrGetExistingCusCodeType("BBB", "DESC", C.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, belgiumCountryCode, "TST", ZDateTime.Today);
			AssertEquals("Precondition: ", true, collection.IncludeParentDataGroupings);
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			var listTypeFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.ListType];
			var list = (ICodeDescriptionPairList)listTypeFilter.List;
			AssertEquals("If ZZRefCusCodeListCombinedCollection.IncludeParentDataGroupings = IncludeParentDataGroupingOptions.Union then type list should include parents.",
				true,
				list.ContainsCode("BBB"));

			collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, germanyCountryCode, new ZString[] { "TST" }, ZDateTime.Today, null, RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildOnly);
			AssertEquals("Precondition: ", false, collection.IncludeParentDataGroupings);
			filterStrip = new ZZRefCusCodeListFilterStripBusinessObject(collection);
			listTypeFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.ListType];
			list = (ICodeDescriptionPairList)listTypeFilter.List;
			AssertEquals("If ZZRefCusCodeListCombinedCollection.IncludeParentDataGroupings = IncludeParentDataGroupingOptions.ChildOnly then type list should not include parents.",
				false,
				list.ContainsCode("BBB"));
		}

		public void TestEffectiveDateFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			var effectiveDateFilter = (ModuleSingleDateFilter)filterStrip[ZZRefCusCodeListFilters.EffectiveDate];
			AssertEquals("effectiveDateFilter.Visibility", FilterVisibility.AlwaysVisible, effectiveDateFilter.Visibility);
			effectiveDateFilter.IsActive = true;
			effectiveDateFilter.Property1 = new ZDateTime(2016, 5, 30);
			var filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));

			effectiveDateFilter.Property1 = new ZDateTime(2016, 8, 1);
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));

			effectiveDateFilter.Property1 = new ZDateTime(2016, 2, 1);
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));
		}

		public void TestListTypeFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			var listTypeFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.ListType];
			listTypeFilter.IsActive = true;
			listTypeFilter.Property = "Z#@";
			var filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			listTypeFilter.Property = C.RefCusCodeListTypes.Codes.CustomsOffice;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));

			listTypeFilter.Property = C.RefCusCodeListTypes.Codes.AdditionalInformation;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));
		}

		public void TestListTypeFilter_AlwaysVisible()
		{
			SetupData();
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			var listTypeFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.ListType];
			AssertEquals("listTypeFilter.Visibility", FilterVisibility.AlwaysVisible, listTypeFilter.Visibility);
		}

		public void TestListTypeDescriptionFilter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string BelgiumCountryCode = Core.Constants.CountryCodes.Belgium;
			const string GermanyCountryCode = Core.Constants.CountryCodes.Germany;
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(C.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(BelgiumCountryCode, "Belgium", parentDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(GermanyCountryCode, "Germany", parentDataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ValuationMethod, "ValuationMethod", Core.Constants.CountryCodes.Belgium);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ValuationMethod, "ValuationMethod", Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.MethodOfPayment, "MethodOfPayment", Core.Constants.CountryCodes.Belgium);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.BankCode, "BankCode", Core.Constants.CountryCodes.Belgium);

			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, C.RefCusCodeListTypes.Codes.ValuationMethod, "B1", "B1 THE BUILDER", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, C.RefCusCodeListTypes.Codes.ValuationMethod, "B2", "B2 THE BUILDER", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var codeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, C.RefCusCodeListTypes.Codes.MethodOfPayment, "B3", "B3 THE BUILDER", new ZDateTime(2016, 7, 1), new ZDateTime(2016, 12, 1));
			var codeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, C.RefCusCodeListTypes.Codes.BankCode, "B4", "B4 THE BUILDER", new ZDateTime(2016, 5, 1), new ZDateTime(2016, 6, 30));

			Factory.Save();

			var zzCodeList1 = Factory.Load<ZZRefCusCodeListCombined>(codeList1.PK);
			var zzCodeList2 = Factory.Load<ZZRefCusCodeListCombined>(codeList2.PK);
			var zzCodeList3 = Factory.Load<ZZRefCusCodeListCombined>(codeList3.PK);
			var zzCodeList4 = Factory.Load<ZZRefCusCodeListCombined>(codeList4.PK);

			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();

			var countryFilter = (ModuleNkFilter)filterStrip[ZZRefCusCodeListFilters.CountryOrGrouping];
			countryFilter.IsActive = true;
			countryFilter.Property = Core.Constants.CountryCodes.Belgium;

			var listTypeDescriptionFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.ListTypeDescription];
			listTypeDescriptionFilter.IsActive = true;
			listTypeDescriptionFilter.Property = "Method";

			listTypeDescriptionFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			var filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));

			listTypeDescriptionFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));

			listTypeDescriptionFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));

			listTypeDescriptionFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));

			listTypeDescriptionFilter.Property = "ValuationMethod";
			listTypeDescriptionFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));

			listTypeDescriptionFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
		}

		public void TestCountryFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			var countryFilter = (ModuleNkFilter)filterStrip[ZZRefCusCodeListFilters.CountryOrGrouping];
			AssertEquals("countryFilter.Visibility", FilterVisibility.AlwaysVisible, countryFilter.Visibility);
			countryFilter.IsActive = true;
			countryFilter.Property = Core.Constants.CountryCodes.NewZealand;
			var filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			countryFilter.Property = Core.Constants.CountryCodes.Eritrea;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));

			countryFilter.Property = Core.Constants.CountryCodes.Ethiopia;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));
		}

		public void TestCodeFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			var codeFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.Code];
			AssertEquals("codeFilter.Visibility", FilterVisibility.AlwaysVisible, codeFilter.Visibility);
			codeFilter.IsActive = true;
			codeFilter.Property = "B";
			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			var filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			codeFilter.Property = "BA";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			codeFilter.Property = "C";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));

			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			codeFilter.Property = "B3BA";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));
		}

		public void TestDescriptionFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			var descFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.Description];
			descFilter.IsActive = true;
			descFilter.Property = "B";
			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			var filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			descFilter.Property = "BA";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			descFilter.Property = "C";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));

			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			descFilter.Property = "B3BA THE BUILDER";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", true, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", false, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", false, zzCodeList8.MatchesFilter(filter));

			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", true, zzCodeList3.MatchesFilter(filter));
			AssertEquals("zzCodeList4", false, zzCodeList4.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList6.MatchesFilter(filter));
			AssertEquals("zzCodeList7", true, zzCodeList7.MatchesFilter(filter));
			AssertEquals("zzCodeList8", true, zzCodeList8.MatchesFilter(filter));
		}

		public void TestTransportModeFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			var codeFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.TransportMode];
			AssertEquals("filter show error when code not present", true, codeFilter.ErrorOnCodeNotPresent);

			codeFilter.IsActive = true;
			codeFilter.Property = "ROA";
			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			var filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));

			codeFilter.Property = "RAI";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));

			codeFilter.Property = "SEA";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", true, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3", false, zzCodeList3.MatchesFilter(filter));

			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1, Transport Mode != SEA", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2, Transport Mode != SEA", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList3, Transport Mode != SEA", true, zzCodeList3.MatchesFilter(filter));
		}

		public void TestAttributeTransportModeFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusCodeListFilterStripBusinessObject();
			var codeFilter = (ModuleTextFilter)filterStrip[ZZRefCusCodeListFilters.AttributeTransportMode];
			AssertEquals("filter show error when code not present", true, codeFilter.ErrorOnCodeNotPresent);
			codeFilter.IsActive = true;
			codeFilter.Property = "ROA";
			var filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", false, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList8.MatchesFilter(filter));

			codeFilter.Property = "SEA";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList5", true, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", false, zzCodeList8.MatchesFilter(filter));

			codeFilter.Property = "INW";
			filter = filterStrip.Filter;
			AssertEquals("zzCodeList1", true, zzCodeList1.MatchesFilter(filter));
			AssertEquals("zzCodeList2", false, zzCodeList2.MatchesFilter(filter));
			AssertEquals("zzCodeList5", false, zzCodeList5.MatchesFilter(filter));
			AssertEquals("zzCodeList6", true, zzCodeList8.MatchesFilter(filter));
		}

		public void TestSettingLayoutContext()
		{
			var collectionFSIS = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, C.RefCusCodeListTypes.Codes.USFSISEstablishmentNumbers, ZDateTime.Today.AddMonths(-1));
			var collectionFAC = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1));
			var collectionFACFSIS = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, new ZString[] { C.RefCusCodeListTypes.Codes.Facilities, C.RefCusCodeListTypes.Codes.USFSISEstablishmentNumbers }, ZDateTime.Today.AddMonths(-1), null);
			var collectionFSISFAC = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, new ZString[] { C.RefCusCodeListTypes.Codes.USFSISEstablishmentNumbers, C.RefCusCodeListTypes.Codes.Facilities }, ZDateTime.Today.AddMonths(-1), null);

			var filterStripBizObjFSIS = (IFilterStripBusinessObjectInternals)new ZZRefCusCodeListFilterStripBusinessObject(collectionFSIS);
			var filterStripBizObjFAC = (IFilterStripBusinessObjectInternals)new ZZRefCusCodeListFilterStripBusinessObject(collectionFAC);
			var filterStripBizObjFACFSIS = (IFilterStripBusinessObjectInternals)new ZZRefCusCodeListFilterStripBusinessObject(collectionFACFSIS);
			var filterStripBizObjFSISFAC = (IFilterStripBusinessObjectInternals)new ZZRefCusCodeListFilterStripBusinessObject(collectionFSISFAC);

			AssertEquals("US_FSIS", filterStripBizObjFSIS.LayoutContext);
			AssertEquals("ZA_FAC", filterStripBizObjFAC.LayoutContext);
			AssertEquals("US_FAC_FSIS", filterStripBizObjFACFSIS.LayoutContext);
			AssertEquals("US_FAC_FSIS", filterStripBizObjFSISFAC.LayoutContext);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ZZRefCusCodeListFilterStripBusinessObject();
		}

		protected override ModuleFilter SetupFilterForTest(ModuleFilter moduleFilter)
		{
			ModuleFilter result = moduleFilter;

			if (moduleFilter.Description == ZZRefCusCodeListFilters.TransportMode || moduleFilter.Description == ZZRefCusCodeListFilters.AttributeTransportMode)
			{
				(moduleFilter as ModuleTextFilter).Property = RefTransportModeList.Codes.AIR;
			}
			else
			{
				result = base.SetupFilterForTest(moduleFilter);
			}

			return result;
		}

		void SetupData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice", Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice", Core.Constants.CountryCodes.Ethiopia);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.BankCode, "BankCode");
			helper.CreateNewOrGetExistingCusCodeType("Z#@", "DESC", Core.Constants.CountryCodes.Eritrea);

			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice, "BABA", "BABA THE BUILDER", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice, "B1BA", "B1BA THE BUILDER", new ZDateTime(2016, 7, 1), new ZDateTime(2016, 12, 1));
			var codeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice, "B2BA", "B2BA THE BUILDER", new ZDateTime(2016, 5, 1), new ZDateTime(2016, 6, 30));
			var codeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice, "B3BA", "B3BA THE BUILDER", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 6, 1));
			var codeList5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, C.RefCusCodeListTypes.Codes.CustomsOffice, "C4BB", "C4BB THE BUILDER", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var codeList6 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, "Z#@", "C5BB", "C5BB THE BUILDER", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var codeList7 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice, "C6BB", "C6BB THE BUILDER", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ABC", "ABC", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice);
			codeList1.Attributes.AddNew("ABC", "123");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DEF", "DEF", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice);
			codeList1.Attributes.AddNew("DEF", "456");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("GHI", "GHI", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Ethiopia, C.RefCusCodeListTypes.Codes.CustomsOffice);
			codeList5.Attributes.AddNew("GHI", "123");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("MNO", "MNO", "Z#@", Core.Constants.CountryCodes.Eritrea, "Z#@");
			codeList6.Attributes.AddNew("MNO", "123");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DEF", "DEF", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice);
			codeList7.Attributes.AddNew("DEF", "123");

			Factory.Save();

			zzCodeList8 = Factory.New<ZZRefCusCodeListCombined>();
			zzCodeList8.ZZD_CodeType = C.RefCusCodeListTypes.Codes.CustomsOffice;
			zzCodeList8.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			zzCodeList8.ZZD_Code = "C7BB";
			zzCodeList8.ZZD_Description = "C7BB THE BUILDER";
			zzCodeList8.ZZD_StartDate = new ZDateTime(2016, 1, 1);
			zzCodeList8.ZZD_EndDate = new ZDateTime(2016, 12, 1);
			zzCodeList8.Attributes.AddNew("ABC", "123");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ABC", "ABC", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice);
			zzCodeList8.Attributes.AddNew("JKL", "456");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("JKL", "JKL", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateTransportModeForCusCodeList(codeList1.PK, "ROA");
			helper.CreateTransportModeForCusCodeList(codeList1.PK, "RAI");
			helper.CreateTransportModeForCusCodeList(codeList2.PK, "ROA");
			helper.CreateTransportModeForCusCodeList(codeList2.PK, "SEA");
			helper.CreateTransportModeForCusCodeAttribute(codeList1.Attributes[0].PK, "SEA");
			helper.CreateTransportModeForCusCodeAttribute(codeList1.Attributes[1].PK, "INW");
			helper.CreateTransportModeForCusCodeAttribute(codeList5.Attributes[0].PK, "SEA");
			zzCodeList8.Attributes[0].ZZE_IsInw = true;

			Factory.Save();

			zzCodeList1 = Factory.Load<ZZRefCusCodeListCombined>(codeList1.PK);
			zzCodeList2 = Factory.Load<ZZRefCusCodeListCombined>(codeList2.PK);
			zzCodeList3 = Factory.Load<ZZRefCusCodeListCombined>(codeList3.PK);
			zzCodeList4 = Factory.Load<ZZRefCusCodeListCombined>(codeList4.PK);
			zzCodeList5 = Factory.Load<ZZRefCusCodeListCombined>(codeList5.PK);
			zzCodeList6 = Factory.Load<ZZRefCusCodeListCombined>(codeList6.PK);
			zzCodeList7 = Factory.Load<ZZRefCusCodeListCombined>(codeList7.PK);
		}

		ZZRefCusCodeListCombined zzCodeList1;
		ZZRefCusCodeListCombined zzCodeList2;
		ZZRefCusCodeListCombined zzCodeList3;
		ZZRefCusCodeListCombined zzCodeList4;
		ZZRefCusCodeListCombined zzCodeList5;
		ZZRefCusCodeListCombined zzCodeList6;
		ZZRefCusCodeListCombined zzCodeList7;
		ZZRefCusCodeListCombined zzCodeList8;
	}
}
