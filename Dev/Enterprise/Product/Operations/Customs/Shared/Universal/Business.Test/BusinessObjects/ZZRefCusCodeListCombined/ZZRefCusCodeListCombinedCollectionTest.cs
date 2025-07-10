using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusCodeListCombinedCollection))]
	class ZZRefCusCodeListCombinedCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAttributeFilter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("BCG", "Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("EGD", "Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("BCD", "Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("GDE", "Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsOffice);
			var code1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsOffice, "A", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code1.Attributes.AddNew("BCD", "S");
			var code2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsOffice, "B", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code2.Attributes.AddNew("EGD", "S");
			var code3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsOffice, "C", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code3.Attributes.AddNew("BCD", "S");
			var code4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsOffice, "D", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code4.Attributes.AddNew("GDE", "S");
			var code5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsStatus, "A", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code5.Attributes.AddNew("BCD", "S");
			Factory.Save();
			CombineAssertions(() =>
			{
				var collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddMonths(-1));
				collection.Load();
				AssertEquals(false, collection.ShouldAddDefaultDataGroupFilter);
				AssertEquals(4, collection.Count);
				AssertNotNull(collection.FindByPK(code1.PK));
				AssertNotNull(collection.FindByPK(code2.PK));
				AssertNotNull(collection.FindByPK(code3.PK));
				AssertNotNull(collection.FindByPK(code4.PK));
				var collection1 = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddMonths(-1));
				var collection2 = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Ecuador, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddMonths(-1));
				AssertSame("IsCached", collection1, collection2);
				var filters = collection1.FilterBusinessObjectDefaults;
				var listTypeFilter = filters["List Type:Property"];
				AssertEquals("CUSOF", listTypeFilter.Value);
				var collectionMultipleTypes = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Ecuador, new[] { new ZString(C.RefCusCodeListTypes.Codes.CustomsOffice), new ZString(C.RefCusCodeListTypes.Codes.CustomsStatus) }, ZDateTime.Today.AddMonths(-1), null, true);
				var filtersMultipleTypes = collectionMultipleTypes.FilterBusinessObjectDefaults;
				var filter1 = filtersMultipleTypes["List Type:Property:1"];
				var filter2 = filtersMultipleTypes["List Type:Property:2"];
				AssertEquals("CSTA", filter1.Value);
				AssertEquals("CUSOF", filter2.Value);
			});
		}

		public void TestCollectionFromParentDataGroup()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(C.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var germanCustomsOffice = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, C.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", yesterday, tomorrow);
			var italianCustomsOffice = helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, C.RefCusCodeListTypes.Codes.CustomsOffice, "IT008734", yesterday, tomorrow);
			Factory.Save();
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, C.RefDataGrouping.Codes.EuropeanUnionEUN, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today, null);
			collection.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "IT", "DE" },  collection.DataGroupingCodes);
				AssertEquals(2, collection.Count);
				Assert(collection.Contains(germanCustomsOffice.PK));
				Assert(collection.Contains(italianCustomsOffice.PK));
				AssertEquals(true, collection.ShouldAddDefaultDataGroupFilter);
			});
		}

		public void TestCollectionFromParentDataGroup_OtherDataGroupingCodes()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(C.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, "Northern Ireland");
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var germanCustomsOffice = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, C.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", yesterday, tomorrow);
			var italianCustomsOffice = helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, C.RefCusCodeListTypes.Codes.CustomsOffice, "IT008734", yesterday, tomorrow);
			var northernIrelandCustomsOffice = helper.CreateCusCodeList(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, C.RefCusCodeListTypes.Codes.CustomsOffice, "XI012731", yesterday, tomorrow);
			Factory.Save();
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, C.RefDataGrouping.Codes.EuropeanUnionEUN, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today, null, new ZString[] { Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes });
			collection.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "IT", "DE", "XI" }, collection.DataGroupingCodes);
				AssertEquals("Count", 3, collection.Count);
				AssertEquals("germanCustomsOffice", true, collection.Contains(germanCustomsOffice.PK));
				AssertEquals("italianCustomsOffice", true, collection.Contains(italianCustomsOffice.PK));
				AssertEquals("northernIrelandCustomsOffice", true, collection.Contains(northernIrelandCustomsOffice.PK));
				AssertEquals(true, collection.ShouldAddDefaultDataGroupFilter);
			});
		}

		public void TestCollectionIncludingAttributeFilters()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(C.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var germanCustomsOffice = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, C.RefCusCodeListTypes.Codes.CustomsOffice, "DE004079", "Customs Office DE", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(germanCustomsOffice.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DES");
			var italianCustomsOffice = helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, C.RefCusCodeListTypes.Codes.CustomsOffice, "IT001023", "Customs Office IT", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(italianCustomsOffice.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DEP");
			Factory.Save();
			var attributeFilterList = new List<RefCusCodeListAttributeFilter>()
			{ new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.ROLE, JoinCondition.Or, "DEP") };
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, C.RefDataGrouping.Codes.EuropeanUnionEUN, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today, attributeFilterList);
			collection.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "IT", "DE" }, collection.DataGroupingCodes);
				AssertEquals(1, collection.Count);
				Assert(collection.Contains(italianCustomsOffice.PK));
				AssertEquals(true, collection.ShouldAddDefaultDataGroupFilter);
			});
		}

		public void TestCollectionWithMultipleCodeTypesAndIncludingAttributeFilters()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var importCodeType = C.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = C.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			helper.CreateNewOrGetExistingCusCodeType(importCodeType, "Document Type (EU Box 44 Imports)");
			helper.CreateNewOrGetExistingCusCodeType(exportCodeType, "Document Type (EU Box 44 Exports)");
			var levelAttributeName = RefCusCodeListAttributeTypes.Codes.Level;
			var headerAttributeValue = "HEADER";
			var itemAttributeValue = "ITEM";
			var cusCodeList = helper.CreateCusCodeList(countryCode, importCodeType, "9001", "9001 DESC", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, levelAttributeName, headerAttributeValue);
			var cusCodeList2 = helper.CreateCusCodeList(countryCode, importCodeType, "9002", "9002 DESC", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList2.PK, levelAttributeName, itemAttributeValue);
			var cusCodeList3 = helper.CreateCusCodeList(countryCode, importCodeType, "9003", "9003 DESC", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList3.PK, levelAttributeName, headerAttributeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList3.PK, levelAttributeName, itemAttributeValue);
			var cusCodeList4 = helper.CreateCusCodeList(countryCode, importCodeType, "9004", "9004 DESC", yesterday, tomorrow);
			var cusCodeList5 = helper.CreateCusCodeList(countryCode, exportCodeType, "3LLA231", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			CombineAssertions(() =>
			{
				var attributeFilterList = new List<RefCusCodeListAttributeFilter>()
			{ new RefCusCodeListAttributeFilter(levelAttributeName, JoinCondition.And, false) };
				var collection = new ZZRefCusCodeListCombinedCollection(Factory, countryCode, new ZString[] { importCodeType }, ZDateTime.Today, attributeFilterList);
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "9001", "9002", "9003" }, collection.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
				attributeFilterList = new List<RefCusCodeListAttributeFilter>()
			{ new RefCusCodeListAttributeFilter(levelAttributeName, JoinCondition.And, false, null, itemAttributeValue) };
				collection = new ZZRefCusCodeListCombinedCollection(Factory, countryCode, new ZString[] { importCodeType }, ZDateTime.Today, attributeFilterList);
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "9002", "9003" }, collection.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
				attributeFilterList = new List<RefCusCodeListAttributeFilter>()
			{ new RefCusCodeListAttributeFilter(levelAttributeName, JoinCondition.And, false, new ZString[] { itemAttributeValue }) };
				collection = new ZZRefCusCodeListCombinedCollection(Factory, countryCode, new ZString[] { importCodeType }, ZDateTime.Today, attributeFilterList);
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "9001" }, collection.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
				attributeFilterList = new List<RefCusCodeListAttributeFilter>()
			{ new RefCusCodeListAttributeFilter(levelAttributeName, JoinCondition.And, false, new ZString[] { itemAttributeValue }, headerAttributeValue) };
				collection = new ZZRefCusCodeListCombinedCollection(Factory, countryCode, new ZString[] { importCodeType }, ZDateTime.Today, attributeFilterList);
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "9001" }, collection.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
				attributeFilterList = new List<RefCusCodeListAttributeFilter>()
			{ new RefCusCodeListAttributeFilter(levelAttributeName, JoinCondition.And, true) };
				collection = new ZZRefCusCodeListCombinedCollection(Factory, countryCode, new ZString[] { importCodeType }, ZDateTime.Today, attributeFilterList);
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "9001", "9002", "9003", "9004" }, collection.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
				attributeFilterList = new List<RefCusCodeListAttributeFilter>()
			{ new RefCusCodeListAttributeFilter(levelAttributeName, JoinCondition.And, true, null, itemAttributeValue) };
				collection = new ZZRefCusCodeListCombinedCollection(Factory, countryCode, new ZString[] { importCodeType }, ZDateTime.Today, attributeFilterList);
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "9002", "9003", "9004" }, collection.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
				attributeFilterList = new List<RefCusCodeListAttributeFilter>()
			{ new RefCusCodeListAttributeFilter(levelAttributeName, JoinCondition.And, true, new ZString[] { itemAttributeValue }) };
				collection = new ZZRefCusCodeListCombinedCollection(Factory, countryCode, new ZString[] { importCodeType }, ZDateTime.Today, attributeFilterList);
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "9001", "9004" }, collection.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
				attributeFilterList = new List<RefCusCodeListAttributeFilter>()
			{ new RefCusCodeListAttributeFilter(levelAttributeName, JoinCondition.And, true, new ZString[] { itemAttributeValue }, headerAttributeValue) };
				collection = new ZZRefCusCodeListCombinedCollection(Factory, countryCode, new ZString[] { importCodeType }, ZDateTime.Today, attributeFilterList);
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "9001", "9004" }, collection.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
				collection = new ZZRefCusCodeListCombinedCollection(Factory, countryCode, new ZString[] { importCodeType, exportCodeType }, ZDateTime.Today, null);
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "9001", "9002", "9003", "9004", "3LLA231" }, collection.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
			});
		}

		public void TestCollectionWithAdditionalFilter()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Germany;
			var exportCodeType = C.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			helper.CreateNewOrGetExistingCusCodeType(exportCodeType, "Document Type (EU Box 44 Exports)");
			var levelAttributeName = RefCusCodeListAttributeTypes.Codes.Level;
			var headerAttributeValue = "HEADER";
			var itemAttributeValue = "ITEM";
			var headerCusCodeListToBeFiltered = helper.CreateCusCodeList(countryCode, exportCodeType, "CodeHFiltered", "Header Code to be filtered", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(headerCusCodeListToBeFiltered.PK, levelAttributeName, headerAttributeValue);
			var itemCusCodeListToBeFiltered = helper.CreateCusCodeList(countryCode, exportCodeType, "CodeIFiltered", "Item Code to be filtered", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(itemCusCodeListToBeFiltered.PK, levelAttributeName, itemAttributeValue);
			var headerCusCodeList = helper.CreateCusCodeList(countryCode, exportCodeType, "CodeH", "Header Code", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(headerCusCodeList.PK, levelAttributeName, headerAttributeValue);
			var itemCusCodeList = helper.CreateCusCodeList(countryCode, exportCodeType, "CodeI", "Item Code", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(itemCusCodeList.PK, levelAttributeName, itemAttributeValue);
			Factory.Save();

			var codesQuery = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.NotEqual, new[] { "CodeIFiltered", "CodeHFiltered" });
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, codesQuery, new ZString[] { countryCode }, new ZString[] { exportCodeType }, ZDateTime.Today, null);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "CodeH", "CodeI", }, collection.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
		}

		public void TestGetCachedCollectionWithAdditionalFilter()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Germany;
			var exportCodeType = C.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			helper.CreateNewOrGetExistingCusCodeType(exportCodeType, "Document Type (EU Box 44 Exports)");
			var levelAttributeName = RefCusCodeListAttributeTypes.Codes.Level;
			var headerAttributeValue = "HEADER";
			var itemAttributeValue = "ITEM";
			var headerCusCodeListToBeFiltered = helper.CreateCusCodeList(countryCode, exportCodeType, "CodeHFiltered", "Header Code to be filtered", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(headerCusCodeListToBeFiltered.PK, levelAttributeName, headerAttributeValue);
			var itemCusCodeListToBeFiltered = helper.CreateCusCodeList(countryCode, exportCodeType, "CodeIFiltered", "Item Code to be filtered", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(itemCusCodeListToBeFiltered.PK, levelAttributeName, itemAttributeValue);
			var headerCusCodeList = helper.CreateCusCodeList(countryCode, exportCodeType, "CodeH", "Header Code", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(headerCusCodeList.PK, levelAttributeName, headerAttributeValue);
			var itemCusCodeList = helper.CreateCusCodeList(countryCode, exportCodeType, "CodeI", "Item Code", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(itemCusCodeList.PK, levelAttributeName, itemAttributeValue);
			Factory.Save();

			var codesQuery = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.NotEqual, new[] { "CodeIFiltered", "CodeHFiltered" });
			var collectionCached = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, codesQuery, countryCode, new ZString[] { exportCodeType }, ZDateTime.Today, null);
			collectionCached.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "CodeH", "CodeI", }, collectionCached.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
				var filters = collectionCached.FilterBusinessObjectDefaults;
				AssertEquals("List Type:Property", "DC44E", filters["List Type:Property"].Value);
				AssertEquals("EffectiveDate:Property1", ZDateTime.Today, filters["Effective Date:Property1"].Value);

				codesQuery = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.NotEqual, new[] { "CodeH", "CodeI" });
				collectionCached = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, codesQuery, countryCode, new ZString[] { exportCodeType }, ZDateTime.Today, null);
				collectionCached.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "CodeHFiltered", "CodeIFiltered", }, collectionCached.Cast<ZZRefCusCodeListCombined>().Select(a => a.ZZD_Code));
				filters = collectionCached.FilterBusinessObjectDefaults;
				AssertEquals("List Type:Property", "DC44E", filters["List Type:Property"].Value);
				AssertEquals("EffectiveDate:Property1", ZDateTime.Today, filters["Effective Date:Property1"].Value);
				AssertSame("Collection cached", collectionCached, ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, codesQuery, countryCode, new ZString[] { exportCodeType }, ZDateTime.Today, null));
			});
		}

		public void TestGetCachedCollectionWithDataGroupingCodes()
		{
			var today = ZDateTime.Today;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var ieCountryCode = "IE";
			var ie5GroupingCode = "IE5";
			var zzzGroupingCode = "ZZZ";
			var codeType = "COD";
			var ieCode = helper.CreateCusCodeList(ieCountryCode, codeType, "Y966", "UK Code", yesterday, tomorrow);
			var ie5Code = helper.CreateCusCodeList(ie5GroupingCode, codeType, "3LLB81E", "CDS Code", yesterday, tomorrow);
			var zzzCode = helper.CreateCusCodeList(zzzGroupingCode, codeType, "Z999", "ZZZ Code", yesterday, tomorrow);
			Factory.Save();

			CombineAssertions(() =>
			{
				var collectionWithOtherDataGroupingCode = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, new ZString[] { ieCountryCode, ie5GroupingCode }, new ZString[] { codeType }, today, null, true);
				AssertSame("IsCached OtherDataGroupingCode", collectionWithOtherDataGroupingCode, ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, new ZString[] { ieCountryCode, ie5GroupingCode }, new ZString[] { codeType }, today, null, true));
				collectionWithOtherDataGroupingCode.Load();
				AssertEquals(2, collectionWithOtherDataGroupingCode.Count);
				AssertNotNull(collectionWithOtherDataGroupingCode.FindByPK(ieCode.PK));
				AssertNotNull(collectionWithOtherDataGroupingCode.FindByPK(ie5Code.PK));
				AssertNull(collectionWithOtherDataGroupingCode.FindByPK(zzzCode.PK));

				var collectionWithOtherDataGroupingCodes = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, new ZString[] { ieCountryCode, ie5GroupingCode, zzzGroupingCode }, new ZString[] { codeType }, today, null, true);
				AssertSame("IsCached OtherDataGroupingCodes", collectionWithOtherDataGroupingCodes, ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, new ZString[] { ieCountryCode, ie5GroupingCode, zzzGroupingCode }, new ZString[] { codeType }, today, null, true));
				collectionWithOtherDataGroupingCodes.Load();
				AssertEquals(3, collectionWithOtherDataGroupingCodes.Count);
				AssertNotNull(collectionWithOtherDataGroupingCodes.FindByPK(ieCode.PK));
				AssertNotNull(collectionWithOtherDataGroupingCodes.FindByPK(ie5Code.PK));
				AssertNotNull(collectionWithOtherDataGroupingCodes.FindByPK(zzzCode.PK));
			});
		}

		public void TestGetCachedCollectionWithParentGroupingAdvanced()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eun = helper.CreateNewOrGetExistingDataGrouping(eunCode, "European Union");
			var deCode = Core.Constants.CountryCodes.Germany;
			var de = helper.CreateNewOrGetExistingDataGrouping(deCode, "Germany", eun);
			var itCode = Core.Constants.CountryCodes.Italy;
			var it = helper.CreateNewOrGetExistingDataGrouping(itCode, "Italy", eun);

			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(eunCode,
				new string[] { codeType }, "9001", "9001 DES EU", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(deCode,
				new string[] { codeType }, "9001", "9001 DES DE", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue.AddDays(1), ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			CombineAssertions(() =>
			{
				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, deCode, new ZString[] { codeType }, ZDateTime.Today, null, RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildOnly);
				collection.Load();
				AssertEquals("Should retrieve child grouping code", 1, collection.Count);
				AssertEquals("Should retrieve child grouping code", deCode, collection[0].ZZD_CountryOrGrouping);
				AssertEquals("Should retrieve child grouping code", "9001 DES DE", collection[0].ZZD_Description);
				collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, deCode, new ZString[] { codeType }, ZDateTime.Today, null, RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
				collection.Load();
				AssertEquals("Should retrieve child grouping code", 1, collection.Count);
				AssertEquals("Should retrieve child grouping code", deCode, collection[0].ZZD_CountryOrGrouping);
				AssertEquals("Should retrieve child grouping code", "9001 DES DE", collection[0].ZZD_Description);
				collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, deCode, new ZString[] { codeType }, ZDateTime.Today, null, RefCusCodeListTypes.IncludeParentDataGroupingOptions.Union);
				collection.Load();
				AssertEquals("Should retrieve parent and child codes", 2, collection.Count);
				AssertContainsExactElementsInExactOrder("Should retrieve parent and child codes", new[] { "9001", "9001" }, collection.Select(x => x.ZZD_Code));
				AssertContainsExactElementsInAnyOrder("Should retrieve parent and child codes", new[] { "9001 DES EU", "9001 DES DE" }, collection.Select(x => x.ZZD_Description));
				collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, itCode, new ZString[] { codeType }, ZDateTime.Today, null, RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildOnly);
				collection.Load();
				AssertEquals("Forced to look for child, should be empty", 0, collection.Count);
				collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, itCode, new ZString[] { codeType }, ZDateTime.Today, null, RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
				collection.Load();
				AssertEquals("No child data, should retrieve from parent grouping", 1, collection.Count);
				AssertEquals("No child data, should retrieve from parent grouping", eunCode, collection[0].ZZD_CountryOrGrouping);
				AssertEquals("No child data, should retrieve from parent grouping", "9001 DES EU", collection[0].ZZD_Description);
				collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, itCode, new ZString[] { codeType }, ZDateTime.Today, null, RefCusCodeListTypes.IncludeParentDataGroupingOptions.Union);
				collection.Load();
				AssertEquals("No child data, should retrieve from parent grouping", 1, collection.Count);
				AssertEquals("No child data, should retrieve from parent grouping", eunCode, collection[0].ZZD_CountryOrGrouping);
				AssertEquals("No child data, should retrieve from parent grouping", "9001 DES EU", collection[0].ZZD_Description);
			});
		}

		public void TestMandatoryAttributes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var type1 = helper.CreateCusCodeType("TP1", "Ref Code Type 1");
			var type2 = helper.CreateCusCodeType("TP2", "Ref Code Type 2");
			var attrNameUnMatchedCountry = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "Attribute 1", "TP1", Core.Constants.CountryCodes.Ecuador);
			attrNameUnMatchedCountry.ZXE_ColumnCaption = "Attribute 1";
			attrNameUnMatchedCountry.ZXE_IsValueMandatory = true;
			var attrNameUnMatchedType = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT2", "Attribute 2", "TP2", Core.Constants.CountryCodes.China);
			attrNameUnMatchedType.ZXE_ColumnCaption = "Attribute 2";
			attrNameUnMatchedType.ZXE_IsValueMandatory = true;
			var attrNameEmptyCaption = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT3", "Attribute 3", "TP1", Core.Constants.CountryCodes.China);
			attrNameEmptyCaption.ZXE_ColumnCaption = ZString.Empty;
			attrNameEmptyCaption.ZXE_IsValueMandatory = true;
			var attrNameNotMandatory = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT4", "Attribute 4", "TP1", Core.Constants.CountryCodes.China);
			attrNameNotMandatory.ZXE_ColumnCaption = "Attribute 3";
			attrNameNotMandatory.ZXE_IsValueMandatory = false;
			var attrMatched = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT5", "Attribute 5", "TP1", Core.Constants.CountryCodes.China);
			attrMatched.ZXE_ColumnCaption = "Attribute 5";
			attrMatched.ZXE_IsValueMandatory = true;
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.China, "TP1", ZDateTime.Today);
			CombineAssertions(() =>
			{
				AssertEquals("Should be readonly including Items.", true, collection.MandatoryAttributeNames.ReadOnly);
				AssertEquals("Should have the only on matched AttrName.", "ATT5", collection.MandatoryAttributeNames.Single().ZXE_Name);
			});
		}

		protected override Type GetExpectedCollectionType() => typeof(ZZRefCusCodeListCombinedCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.Estonia, "ABC", ZDateTime.Today.AddMonths(-1));
	}
}
