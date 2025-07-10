using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusCodeListCombined.Loader))]
	public class ZZRefCusCodeListCombinedLoaderTestCase : LoaderTestCase
	{
		public void TestLoadByCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateNewOrGetExistingCusCodeType("TT1", "TT1 DESC");
			var codeType2 = helper.CreateNewOrGetExistingCusCodeType("TT2", "TT2 DESC");
			var codeType3 = helper.CreateNewOrGetExistingCusCodeType("TT3", "TT3 DESC");
			var code1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT1", "BOB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT1", "JOE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT1", "JAY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT2", "BOB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT3", "JOE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var codeTypes = ZZRefCusCodeListCombined.Loader.LoadByCode(Factory, Core.Constants.CountryCodes.Ethiopia, new ZString[] { "TT1", "TT2", "TT3" }, "BOB", ZDateTime.Today).Select(x => x.ZZD_CodeType).ToArray();
			AssertEquals(2, codeTypes.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TT1", "TT2" }, codeTypes);
			codeTypes = ZZRefCusCodeListCombined.Loader.LoadByCode(Factory, Core.Constants.CountryCodes.Ethiopia, new ZString[] { "TT1", "TT2", "TT3" }, "JOE", ZDateTime.Today).Select(x => x.ZZD_CodeType).ToArray();
			AssertEquals(2, codeTypes.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TT1", "TT3" }, codeTypes);
			codeTypes = ZZRefCusCodeListCombined.Loader.LoadByCode(Factory, Core.Constants.CountryCodes.Ethiopia, new ZString[] { "TT1", "TT2", "TT3" }, "JAY", ZDateTime.Today).Select(x => x.ZZD_CodeType).ToArray();
			AssertEquals(1, codeTypes.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TT1" }, codeTypes);
		}

		public void TestLoad()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateNewOrGetExistingCusCodeType("TT1", "TT1 DESC");
			var codeType2 = helper.CreateNewOrGetExistingCusCodeType("TT2", "TT2 DESC");
			var code1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT1", "BOB", ZDateTime.BrettsBirthday, ZDateTime.Today);
			var code2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT1", "JOE", ZDateTime.BrettsBirthday.AddMonths(2), ZDateTime.Today);
			var code3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, "TT1", "JAY", ZDateTime.BrettsBirthday, ZDateTime.Today);
			var code4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT2", "JOY", ZDateTime.BrettsBirthday, ZDateTime.Today);
			Factory.Save();
			var codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Ethiopia, "TT1", ZDateTime.Today.AddMonths(-1), addAttributeFetchHints: false).Select(x => x.ZZD_Code).OrderBy(x => x).ToArray();
			AssertEquals(2, codes.Length);
			AssertEquals("BOB", codes[0]);
			AssertEquals("JOE", codes[1]);
			AssertEquals(0, Factory.ActiveFetchHintsForTable(ZZRefCusCodeListAttributeCombinedSchema.Constants.TableName));
			codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Ethiopia, "TT1", ZDateTime.Today.AddMonths(-1), new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.GreaterThan, ZDateTime.BrettsBirthday)).Select(x => x.ZZD_Code).OrderBy(x => x).ToArray();
			AssertEquals(1, codes.Length);
			AssertEquals("JOE", codes[0]);
			AssertEquals(0, Factory.ActiveFetchHintsForTable(ZZRefCusCodeListAttributeCombinedSchema.Constants.TableName));
			codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Ethiopia, "TT1", ZDateTime.Today.AddMonths(-1), addAttributeFetchHints: true).Select(x => x.ZZD_Code).OrderBy(x => x).ToArray();
			AssertEquals(2, codes.Length);
			AssertEquals("BOB", codes[0]);
			AssertEquals("JOE", codes[1]);
			AssertEquals(2, Factory.ActiveFetchHintsForTable(ZZRefCusCodeListAttributeCombinedSchema.Constants.TableName));
		}

		public void TestLoadForStartDateBeforeAndAfterToday()
		{
			// Setup some RefCusCodeLists
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateNewOrGetExistingCusCodeType("TT1", "TT1 DESC");
			//Create RefCusCodeList with start date 2 months in past and end date 5 months in the future
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT1", "BOB", "BOB DESC", ZDateTime.Today.AddMonths(-2), ZDateTime.Today.AddMonths(5));
			//Create RefCusCodeList with start date 10 months in past and end date 5 months in the future
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT1", "JOY", "JOY DESC", ZDateTime.Today.AddMonths(-10), ZDateTime.Today.AddMonths(5));
			//Create RefCusCodeList with start date 10 months in past and end date 1 month in the past
			var code3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT1", "JIM", "JIM DESC", ZDateTime.Today.AddMonths(-10), ZDateTime.Today.AddMonths(-1));
			Factory.Save();
			// Load codes with start date before 5 months ago
			var codes = ZZRefCusCodeListCombined.Loader.LoadForStartDateBeforeAndAfterToday(Factory, Core.Constants.CountryCodes.Ethiopia, "TT1", ZDateTime.Today.AddMonths(-5), addAttributeFetchHints: false).Select(x => x.ZZD_Code).OrderBy(x => x).ToArray();
			// Assert that 1 record is returned
			AssertEquals(1, codes.Length);
			//Assert that record returned is JOY
			AssertEquals("JOY", codes[0]);
			AssertEquals(0, Factory.ActiveFetchHintsForTable(ZZRefCusCodeListAttributeCombinedSchema.Constants.TableName));
			// Load codes with start date before 12 months ago
			codes = ZZRefCusCodeListCombined.Loader.LoadForStartDateBeforeAndAfterToday(Factory, Core.Constants.CountryCodes.Ethiopia, "TT1", ZDateTime.Today.AddMonths(-12), addAttributeFetchHints: false).Select(x => x.ZZD_Code).OrderBy(x => x).ToArray();
			// Assert that no records are returned
			AssertEquals(0, codes.Length);
			AssertEquals(0, Factory.ActiveFetchHintsForTable(ZZRefCusCodeListAttributeCombinedSchema.Constants.TableName));
		}

		public void TestLoadWithTransportModeAndPort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var vuSAIR = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "SAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var att1 = helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR.PK, "PORT", "VUAIR");
			helper.CreateTransportModeForCusCodeList(vuSAIR.PK, "AIR");
			var vuSSEA = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "SSEA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var att2 = helper.CreateNewOrGetExistingCusCodeListAttribute(vuSSEA.PK, "PORT", "VUSEA");
			helper.CreateTransportModeForCusCodeList(vuSSEA.PK, "SEA");
			var vuSAIRNoTransPort = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AIR2", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Vanuatu, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today, addAttributeFetchHints: true, transportMode: "MAI").Select(x => x.ZZD_Code).OrderBy(x => x).ToArray();
			AssertEquals(1, codes.Length);
			AssertEquals("AIR2", codes[0]);
			codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Vanuatu, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today, addAttributeFetchHints: true, transportMode: "AIR").Select(x => x.ZZD_Code).OrderBy(x => x).ToArray();
			AssertEquals(2, codes.Length);
			AssertEquals("AIR2", codes[0]);
			AssertEquals("SAIR", codes[1]);
			codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Vanuatu, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today, addAttributeFetchHints: true, transportMode: "").Select(x => x.ZZD_Code).OrderBy(x => x).ToArray();
			AssertEquals(3, codes.Length);
			codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Vanuatu, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today, addAttributeFetchHints: true, transportMode: "AIR").Select(x => x.ZZD_Code).OrderBy(x => x).ToArray();
			AssertEquals(2, codes.Length);
		}

		public void TestLoadAllCountries()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateNewOrGetExistingCusCodeType("TT1", "TT1 DESC");
			var codeType2 = helper.CreateNewOrGetExistingCusCodeType("TT2", "TT2 DESC");
			var code1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, "TT1", "BOB", ZDateTime.BrettsBirthday, ZDateTime.Today);
			var code2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Singapore, "TT1", "JOE", ZDateTime.BrettsBirthday, ZDateTime.Today);
			var code3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "TT2", "JAY", ZDateTime.BrettsBirthday, ZDateTime.Today);
			Factory.Save();
			var codes = ZZRefCusCodeListCombined.Loader.LoadAllCountries(Factory, "TT1", ZDateTime.Today.AddMonths(-1)).Select(x => x.ZZD_CountryOrGrouping).OrderBy(x => x).ToArray();
			AssertEquals(2, codes.Length);
			AssertEquals(Core.Constants.CountryCodes.Ethiopia, codes[0]);
			AssertEquals(Core.Constants.CountryCodes.Singapore, codes[1]);
			codes = ZZRefCusCodeListCombined.Loader.LoadAllCountries(Factory, "TT2", ZDateTime.Today.AddMonths(-1)).Select(x => x.ZZD_CountryOrGrouping).OrderBy(x => x).ToArray();
			AssertEquals(1, codes.Length);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, codes[0]);
		}

		public void TestLoadAndFallbackToParentDataGroupingIfNotFound()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue.Date;
			var maxDate = ZDateTime.MaxSmallDateTimeValue.Date;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: grouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType("TEST", "Test Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "TEST", "A", "DESC", minDate, maxDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "TEST", "B", "DESC", minDate, maxDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "TEST", "C", "DESC", minDate, maxDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "TEST", "D", "DESC", minDate, maxDate);
			helper.CreateNewOrGetExistingCusCodeType("TST2", "Test Code Type 2");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "TST2", "M", "DESC", minDate, maxDate);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Specific Latvia List", new ZString[] { "C", "D" }, ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, Core.Constants.CountryCodes.Latvia, "TEST", ZDateTime.Now).Select(x => x.ZZD_Code));
				AssertContainsExactElementsInAnyOrder("Fallback to Parent Data Grouping", new ZString[] { "A", "B" }, ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, Core.Constants.CountryCodes.Germany, "TEST", ZDateTime.Now).Select(x => x.ZZD_Code));
				AssertEquals("Empty as no specifc or parent data grouping", false, ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, Core.Constants.CountryCodes.Eritrea, "TEST", ZDateTime.Now).Any());
				AssertContainsExactElementsInAnyOrder("Specific Parent Data Goruping", new ZString[] { "A", "B" }, ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "TEST", ZDateTime.Now).Select(x => x.ZZD_Code));
				AssertContainsExactElementsInAnyOrder("Additional Filter", new ZString[] { "A" }, ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "TEST", ZDateTime.Now, new ZQuery(RefCusCodeListSchema.ZZD_Code, "A")).Select(x => x.ZZD_Code));
			}

			);
		}

		public void TestLoadAndFallbackToParentDataGroupingIfNotFound_CodeNotInDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: grouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType("TEST", "Test Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "TEST", "A", "DESC", ZDateTime.MinSmallDateTimeValue.Date, ZDate.Today.AddDays(-2));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "TEST", "B", "DESC", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "TEST", "C", "DESC", ZDate.Today.AddDays(2), ZDateTime.MaxSmallDateTime.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "TEST", "X", "DESC", ZDateTime.MinSmallDateTimeValue.Date, ZDate.Today.AddDays(-2));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "TEST", "Y", "DESC", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "TEST", "Z", "DESC", ZDate.Today.AddDays(2), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Latvia has a list only one current", new ZString[] { "Y" }, ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, Core.Constants.CountryCodes.Latvia, "TEST", ZDateTime.Now).Select(x => x.ZZD_Code));
				AssertContainsExactElementsInAnyOrder("Germany falls back to parent with only one current", new ZString[] { "B" }, ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, Core.Constants.CountryCodes.Germany, "TEST", ZDateTime.Now).Select(x => x.ZZD_Code));
				AssertContainsExactElementsInAnyOrder("EUN is parent and only one current", new ZString[] { "B" }, ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "TEST", ZDateTime.Now).Select(x => x.ZZD_Code));
			}

			);
		}

		public void TestLoadAndFallbackToParentDataGroupingIfNotFound_CountryHasNoParentDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeType("TEST", "Test Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "TEST", "A", "DESC", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
			ZZRefCusCodeListCombined[] result = null;
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("No null reference exception no parent data grouping", () => result = ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, Core.Constants.CountryCodes.Latvia, "TEST", ZDateTime.Now));
				AssertEquals("Latvia has nothing", false, result.Any());
			}

			);
		}

		public void TestLoadAndFallbackToParentDataGroupingIfNotFound_ArgumentNotNullOrEmpty()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Should throw an argument null exception as the factory provided was null.", () => ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(null, Core.Constants.CountryCodes.Latvia, "TEST", ZDateTime.Now));
				AssertExceptionThrown<ArgumentException>("Should throw an argument exception as the datagrouping provided was null.", () => ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, null, "TEST", ZDateTime.Now));
				AssertExceptionThrown<ArgumentException>("Should throw an argument exception as the datagrouping provided was empty.", () => ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, ZString.Empty, "TEST", ZDateTime.Now));
				AssertExceptionThrown<ArgumentException>("Should throw an argument exception as the codeType provided was null.", () => ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, Core.Constants.CountryCodes.Latvia, null, ZDateTime.Now));
				AssertExceptionThrown<ArgumentException>("Should throw an argument exception as the codeType provided was empty.", () => ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, Core.Constants.CountryCodes.Latvia, ZString.Empty, ZDateTime.Now));
			}

			);
		}

		public void TestLoadTop1ByParentDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE001023", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT001023", "Customs Office 1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT001024", "Customs Office 2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			AssertExceptionThrown<ArgumentException>(() => ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(Factory, "DE001023", ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
			AssertNull(ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(Factory, "GB001023", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
			AssertNull(ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(Factory, "DE001023", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZString.Empty, ZDate.Today));
			AssertNull(ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(Factory, ZString.Empty, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
			AssertEquals("Customs Office 1", ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(Factory, "IT001023", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today).ZZD_Description);
			AssertEquals("Customs Office 2", ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(Factory, "IT001024", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today).ZZD_Description);
		}

		public void TestLoadTop1ByCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy");
			var office1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT001023", "Customs Office 1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(office1.PK, RefCusCodeListAttributeTypes.Codes.Port, "DEBER");
			var office2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT001024", "Customs Office 2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(office2.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DES");
			Factory.Save();
			AssertExceptionThrown<ArgumentException>(() => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "DE001023", ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
			AssertExceptionThrown<ArgumentException>(() => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "DE001024", ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
			AssertNull(ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "GB001023", Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
			AssertNull(ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "IT001023", Core.Constants.CountryCodes.Italy, ZString.Empty, ZDate.Today));
			AssertNull(ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "IT001024", Core.Constants.CountryCodes.Italy, ZString.Empty, ZDate.Today));
			AssertNull(ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, ZString.Empty, Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
			AssertEquals("Customs Office 1", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "IT001023", Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today).ZZD_Description);
			AssertEquals("Customs Office 2", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "IT001024", Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today, null, new ZString[] { RefCusCodeListAttributeTypes.Codes.ROLE }).ZZD_Description);
		}

		public void TestLoadTop1ByCountryAndAttributes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			var office1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT001023", "Customs Office 1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(office1.PK, RefCusCodeListAttributeTypes.Codes.Port, "DEBER");
			var office2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT001024", "Customs Office 2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(office2.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DES");
			var office3 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "EU001025", "Customs Office 3", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeListAttribute(office3.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DES");
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("dataGroupingCode is empty", () => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, "DE001023", ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
				AssertNull("Result when code is invalid", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, "GB001023", Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
				AssertNull("Result when codeType is empty", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, "IT001023", Core.Constants.CountryCodes.Italy, ZString.Empty, ZDate.Today));
				AssertNull("Result when code is empty", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, ZString.Empty, Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
				AssertEquals("Without filter and attributeFilters", "Customs Office 1", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, "IT001023", Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today).ZZD_Description);
				AssertEquals("With filter", "Customs Office 1", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, "IT001023", Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today, new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Description, "Customs Office 1")).ZZD_Description);
				AssertNull("Result when doesn't match filter", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, "IT001023", Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today, new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Description, "Customs Office 2")));
				AssertEquals("With attributeFilters", "Customs Office 2", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, "IT001024", Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today, attributeFilters: new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.ROLE, JoinCondition.And, "DES") }).ZZD_Description);
				AssertNull("Result when doesn't match attributeFilters", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, "IT001024", Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today, attributeFilters: new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.ROLE, JoinCondition.And, "DES1") }));
				AssertEquals("IncludeParentDataGrouping is true", "Customs Office 3", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, "EU001025", Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today, attributeFilters: new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.ROLE, JoinCondition.And, "DES") }).ZZD_Description);
				AssertNull("IncludeParentDataGrouping is false", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, "EU001025", Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today, attributeFilters: new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.ROLE, JoinCondition.And, "DES") }, includeParentDataGrouping: false));
			});
		}

		public void TestLoadWithPriorityToUserEntered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Facility Code");
			var eun = helper.CreateNewOrGetExistingDataGrouping(eunCode, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			var cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, codeType, "N1", "N1 Des", new ZDateTime(2018, 7, 17), ZDateTime.MaxSmallDateTimeValue);
			var cusCodeList2 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList2.ZZD_CodeType = cusCodeList.ZZD_ZZK_NKCodeType;
			cusCodeList2.ZZD_Code = cusCodeList.ZZD_Code;
			cusCodeList2.ZZD_Description = cusCodeList.ZZD_Description + "2";
			cusCodeList2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Germany;
			cusCodeList2.ZZD_StartDate = new ZDateTime(2018, 9, 11);
			cusCodeList2.ZZD_EndDate = cusCodeList.ZZD_EndDate;
			var cusCodeList3 = helper.CreateCusCodeList(eunCode, codeType, "N1", "N1 Des3", new ZDateTime(2018, 7, 17), ZDateTime.MaxSmallDateTimeValue);
			var cusCodeList4 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList4.ZZD_CodeType = cusCodeList3.ZZD_ZZK_NKCodeType;
			cusCodeList4.ZZD_Code = cusCodeList3.ZZD_Code;
			cusCodeList4.ZZD_Description = "N1 Des4";
			cusCodeList4.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Italy;
			cusCodeList4.ZZD_StartDate = new ZDateTime(2018, 9, 11);
			cusCodeList4.ZZD_EndDate = cusCodeList3.ZZD_EndDate;
			Factory.Save();
			AssertEquals("N1 Des3", ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Germany, codeType, new ZDateTime(2018, 7, 18), includeParentDataGrouping: true).Single().ZZD_Description);
			AssertEquals("N1 Des2", ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Germany, codeType, new ZDateTime(2018, 9, 12), includeParentDataGrouping: true).Single().ZZD_Description);
			AssertEquals("N1 Des3", ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Italy, codeType, new ZDateTime(2018, 7, 18), includeParentDataGrouping: true).Single().ZZD_Description);
			AssertEquals("N1 Des4", ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Italy, codeType, new ZDateTime(2018, 9, 12), includeParentDataGrouping: true).Single().ZZD_Description);
			AssertEquals("N1 Des4", ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Italy, codeType, new ZDateTime(2018, 9, 12), includeParentDataGrouping: false).Single().ZZD_Description);
			Assert(!ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Italy, codeType, new ZDateTime(2018, 7, 18), includeParentDataGrouping: false).Any());
		}

		public void TestLoadTop1ByCountryWithPriorityToUserEntered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Facility Code");
			var cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, codeType, "N1", "N1 Des", new ZDateTime(2018, 7, 17), ZDateTime.MaxSmallDateTimeValue);
			var cusCodeList2 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList2.ZZD_CodeType = cusCodeList.ZZD_ZZK_NKCodeType;
			cusCodeList2.ZZD_Code = cusCodeList.ZZD_Code;
			cusCodeList2.ZZD_Description = cusCodeList.ZZD_Description + "2";
			cusCodeList2.ZZD_CountryOrGrouping = cusCodeList.ZZD_ZZZ_NKDataGrouping;
			cusCodeList2.ZZD_StartDate = new ZDateTime(2018, 9, 11);
			cusCodeList2.ZZD_EndDate = cusCodeList.ZZD_EndDate;
			Factory.Save();
			AssertNull(ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, cusCodeList.ZZD_Code, cusCodeList.ZZD_ZZZ_NKDataGrouping, cusCodeList.ZZD_ZZK_NKCodeType, new ZDateTime(2018, 7, 18)));
			AssertEquals("N1 Des2", ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, cusCodeList.ZZD_Code, cusCodeList.ZZD_ZZZ_NKDataGrouping, cusCodeList.ZZD_ZZK_NKCodeType, new ZDateTime(2018, 9, 12)).ZZD_Description);
		}

		public void TestLoadTop1ByCountryWithPriorityToDataGrouping()
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
				new string[] { codeType }, "9001", "9001 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(deCode,
				new string[] { codeType }, "9001", "9001 DES DE", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue.AddDays(1), ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			CombineAssertions(() =>
			{
				var deChildCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "9001", deCode, codeType, new ZDateTime(2018, 7, 18));
				AssertEquals("Should give priority to code for specified grouping", "9001 DES DE", deChildCode.ZZD_Description);
				var deForcedChildCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "9001", deCode, codeType, new ZDateTime(2018, 7, 18), includeParentDataGrouping: false);
				AssertEquals("Should give priority to code for specified grouping", "9001 DES DE", deForcedChildCode.ZZD_Description);
				var noItChildCodeFetchingParent = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "9001", itCode, codeType, new ZDateTime(2018, 7, 18));
				AssertEquals("No code for specified grouping, should load for parent", "9001 DES", noItChildCodeFetchingParent.ZZD_Description);
				var itForcedChildCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "9001", itCode, codeType, new ZDateTime(2018, 7, 18), includeParentDataGrouping: false);
				AssertNull("No code for specified grouping", itForcedChildCode);
			});
		}

		public void TestLoadTop1ByParentDataGroupingWithPriorityToUserEntered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Facility Code");
			var eun = helper.CreateNewOrGetExistingDataGrouping(eunCode, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			var cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, codeType, "N1", "N1 Des", new ZDateTime(2018, 7, 17), ZDateTime.MaxSmallDateTimeValue);
			var cusCodeList2 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList2.ZZD_CodeType = cusCodeList.ZZD_ZZK_NKCodeType;
			cusCodeList2.ZZD_Code = cusCodeList.ZZD_Code;
			cusCodeList2.ZZD_Description = cusCodeList.ZZD_Description + "2";
			cusCodeList2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Italy;
			cusCodeList2.ZZD_StartDate = new ZDateTime(2018, 9, 11);
			cusCodeList2.ZZD_EndDate = cusCodeList.ZZD_EndDate;
			Factory.Save();
			AssertEquals("N1 Des", ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(Factory, cusCodeList.ZZD_Code, eunCode, cusCodeList.ZZD_ZZK_NKCodeType, new ZDateTime(2018, 7, 18)).ZZD_Description);
			AssertEquals("N1 Des", ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(Factory, cusCodeList.ZZD_Code, eunCode, cusCodeList.ZZD_ZZK_NKCodeType, new ZDateTime(2018, 9, 12)).ZZD_Description);
			cusCodeList2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Germany;
			Factory.Save();
			AssertEquals("N1 Des2", ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(new BusinessObjectFactory(), cusCodeList.ZZD_Code, eunCode, cusCodeList.ZZD_ZZK_NKCodeType, new ZDateTime(2018, 9, 12)).ZZD_Description);
		}

		public void TestLoadAllCountriesWithPriorityToUserEntered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Facility Code");
			var eun = helper.CreateNewOrGetExistingDataGrouping(eunCode, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			var cusCodeList = helper.CreateCusCodeList(eunCode, codeType, "N1", "N1 Des", new ZDateTime(2018, 7, 17), ZDateTime.MaxSmallDateTimeValue);
			var cusCodeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, codeType, "N1", "N1 Des2", new ZDateTime(2018, 7, 17), ZDateTime.MaxSmallDateTimeValue);
			var cusCodeList3 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList3.ZZD_CodeType = cusCodeList.ZZD_ZZK_NKCodeType;
			cusCodeList3.ZZD_Code = cusCodeList.ZZD_Code;
			cusCodeList3.ZZD_Description = cusCodeList.ZZD_Description + "3";
			cusCodeList3.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Germany;
			cusCodeList3.ZZD_StartDate = new ZDateTime(2018, 9, 11);
			cusCodeList3.ZZD_EndDate = cusCodeList.ZZD_EndDate;
			Factory.Save();
			var codes = ZZRefCusCodeListCombined.Loader.LoadAllCountries(Factory, codeType, new ZDateTime(2018, 7, 18));
			AssertContainsExactElementsInAnyOrder(new[] { "N1 Des" }, codes.Select(x => x.ZZD_Description));
			codes = ZZRefCusCodeListCombined.Loader.LoadAllCountries(Factory, codeType, new ZDateTime(2018, 9, 12));
			AssertContainsExactElementsInAnyOrder(new[] { "N1 Des", "N1 Des3" }, codes.Select(x => x.ZZD_Description));
		}

		public void TestLoadForStartDateBeforeAndAfterTodayWithPriorityToUserEntered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Facility Code");
			var cusCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ethiopia, codeType, "N1", "N1 Des", new ZDateTime(2018, 7, 17), ZDateTime.MaxSmallDateTimeValue);
			var cusCodeList2 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList2.ZZD_CodeType = cusCodeList.ZZD_ZZK_NKCodeType;
			cusCodeList2.ZZD_Code = cusCodeList.ZZD_Code;
			cusCodeList2.ZZD_Description = cusCodeList.ZZD_Description + "2";
			cusCodeList2.ZZD_CountryOrGrouping = cusCodeList.ZZD_ZZZ_NKDataGrouping;
			cusCodeList2.ZZD_StartDate = new ZDateTime(2018, 9, 11);
			cusCodeList2.ZZD_EndDate = cusCodeList.ZZD_EndDate;
			Factory.Save();
			AssertEquals(0, ZZRefCusCodeListCombined.Loader.LoadForStartDateBeforeAndAfterToday(Factory, Core.Constants.CountryCodes.Ethiopia, codeType, new ZDateTime(2018, 7, 18), addAttributeFetchHints: false).Length);
			AssertEquals("N1 Des2", ZZRefCusCodeListCombined.Loader.LoadForStartDateBeforeAndAfterToday(Factory, Core.Constants.CountryCodes.Ethiopia, codeType, new ZDateTime(2018, 9, 12), addAttributeFetchHints: false).Single().ZZD_Description);
			cusCodeList2.ZZD_EndDate = ZDateTime.Today;
			Factory.Save();
			AssertEquals(0, ZZRefCusCodeListCombined.Loader.LoadForStartDateBeforeAndAfterToday(Factory, Core.Constants.CountryCodes.Ethiopia, codeType, new ZDateTime(2018, 9, 12), addAttributeFetchHints: false).Length);
		}

		public void TestNoResultQueryFilterByCountryCode()
		{
			var filter = ZZRefCusCodeListCombined.Loader.GetFilter(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Empty, (ZQuery)null, false);
			Assert(filter.IsNoResultQuery);
			filter = ZZRefCusCodeListCombined.Loader.GetFilter(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Invalid, (ZQuery)null, false);
			Assert(filter.IsNoResultQuery);
			filter = ZZRefCusCodeListCombined.Loader.GetFilter(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today, (ZQuery)null, false);
			Assert(!filter.IsNoResultQuery);
			filter = ZZRefCusCodeListCombined.Loader.GetFilter(Factory, ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today, (ZQuery)null, false);
			Assert(filter.IsNoResultQuery);
			filter = ZZRefCusCodeListCombined.Loader.GetFilter(Factory, Core.Constants.CountryCodes.Germany, ZString.Empty, ZDate.Today, (ZQuery)null, false);
			Assert(filter.IsNoResultQuery);
		}

		public void TestLoadWithCountryTypeAndAttributesFilter()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var customsOffice1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004079", "Customs Office 1", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(customsOffice1.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DES");
			var customsOffice2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004023", "Customs Office 2", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(customsOffice2.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DEP");
			var customsOffice3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004087", "Customs Office 3", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(customsOffice3.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DEX");
			var customsOffice4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004087", "Customs Office 4", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeListAttribute(customsOffice4.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DES");
			Factory.Save();
			var attributeFilters = new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.ROLE, JoinCondition.And, "DES", "DEP") };
			var customsOffices = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today, attributeFilters);
			AssertEquals(2, customsOffices.Length);
			AssertContainsExactElementsInAnyOrder(new[] { customsOffice1.PK, customsOffice2.PK }, customsOffices.Select(x => x.PK));
		}

		public void TestLoadForCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			var aa01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA01", "Loading place AA01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aa02 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA02", "Loading place AA02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			CombineAssertions(() =>
			{
				var codes = Array.Empty<ZString>();
				AssertEquals("Empty param codes", 0, GetCusCodeLists().Length);
				codes = new ZString[] { "AA01", "AA02", "invalid" };
				AssertContainsExactElementsInAnyOrder("Valid and invalid codes", new[] { aa01.PK, aa02.PK }, GetCusCodeLists().Select(x => x.PK));
				ZZRefCusCodeListCombined[] GetCusCodeLists()
				{
					return ZZRefCusCodeListCombined.Loader.LoadForCodes(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, codes, ZDateTime.Today, Enumerable.Empty<RefCusCodeListAttributeFilter>());
				}
			}

			);
		}

		public void TestLoadForCodes_AttributeFilters()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Valid Customs Offices for Location", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ROLE, "Role", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Germany);
			var aa01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA01", "Loading place AA01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aa02 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA02", "Loading place AA02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa01.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE003202");
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa02.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE003202");
			helper.CreateNewOrGetExistingCusCodeListAttribute(aa02.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "administrator");
			Factory.Save();
			CombineAssertions(() =>
			{
				var attributeFilterList = new List<RefCusCodeListAttributeFilter>();
				AssertContainsExactElementsInAnyOrder("No attribute filters", new[] { aa01.PK, aa02.PK }, GetCusCodeLists().Select(x => x.PK));
				attributeFilterList.Add(new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.CustomsOffice, SQLComparisonOperator.Equal, "invalid"));
				AssertEquals("Invalid attribute value", 0, GetCusCodeLists().Length);
				attributeFilterList[0] = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.ROLE, SQLComparisonOperator.Equal, "DE003202");
				AssertEquals("Invalid attribute name", 0, GetCusCodeLists().Length);
				attributeFilterList[0] = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.CustomsOffice, SQLComparisonOperator.Equal, "DE003202");
				AssertContainsExactElementsInAnyOrder("Valid attribute name and value", new[] { aa01.PK, aa02.PK }, GetCusCodeLists().Select(x => x.PK));
				attributeFilterList.Add(new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.ROLE, SQLComparisonOperator.Equal, "administrator"));
				AssertContainsExactElementsInAnyOrder("Two attribute filters", new[] { aa02.PK }, GetCusCodeLists().Select(x => x.PK));
				ZZRefCusCodeListCombined[] GetCusCodeLists()
				{
					return ZZRefCusCodeListCombined.Loader.LoadForCodes(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, new ZString[] { "AA01", "AA02" }, ZDateTime.Today, attributeFilterList);
				}
			}

			);
		}

		[TestDate(2020, 12, 30)]
		public void TestLoadCodesWithStartAndEndDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA01", "Loading place AA01", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			Factory.Save();
			AssertEquals(0, ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today, ZDateTime.Today.AddDays(20)).Length);
			AssertEquals("AA01", ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today, ZDateTime.Today.AddDays(9)).Single().ZZD_Code);
		}

		[TestDate(2020, 10, 10, 0, 0, 0)]
		public void TestLoadForCodes_InvalidParameters()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			var aa01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA01", "Loading place AA01", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Invalid country code", 0, ZZRefCusCodeListCombined.Loader.LoadForCodes(Factory, Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, new ZString[] { "AA01" }, ZDateTime.Today, Enumerable.Empty<RefCusCodeListAttributeFilter>()).Length);
				AssertEquals("Invalid type", 0, ZZRefCusCodeListCombined.Loader.LoadForCodes(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, new ZString[] { "AA01" }, ZDateTime.Today, Enumerable.Empty<RefCusCodeListAttributeFilter>()).Length);
				AssertEquals("Invalid date (before startDate)", 0, ZZRefCusCodeListCombined.Loader.LoadForCodes(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, new ZString[] { "AA01" }, ZDateTime.Today.AddDays(-2), Enumerable.Empty<RefCusCodeListAttributeFilter>()).Length);
				AssertEquals("Invalid date (after endDate)", 0, ZZRefCusCodeListCombined.Loader.LoadForCodes(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, new ZString[] { "AA01" }, ZDateTime.Today.AddDays(2), Enumerable.Empty<RefCusCodeListAttributeFilter>()).Length);
				AssertContainsExactElementsInAnyOrder("All valid", new ZGuid[] { aa01.PK }, ZZRefCusCodeListCombined.Loader.LoadForCodes(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, new ZString[] { "AA01" }, ZDateTime.Today, Enumerable.Empty<RefCusCodeListAttributeFilter>()).Select(x => x.PK));
			}

			);
		}

		public void TestExistsWithinEffectiveDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice", dataGrouping: Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "Currency", dataGrouping: Core.Constants.CountryCodes.Germany);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Test", "Description", new ZDateTime(2022, 5, 18), new ZDateTime(2022, 5, 20));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "Test", "Description", new ZDateTime(2022, 5, 18), new ZDateTime(2022, 5, 20));
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Date = startDate", true, ZZRefCusCodeListCombined.Loader.ExistsWithinEffectiveDate(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "Test", new ZDateTime(2022, 5, 18), false));
				AssertEquals("Date = endDate", true, ZZRefCusCodeListCombined.Loader.ExistsWithinEffectiveDate(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "Test", new ZDateTime(2022, 5, 20), false));
				AssertEquals("Date < startDate", false, ZZRefCusCodeListCombined.Loader.ExistsWithinEffectiveDate(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "Test", new ZDateTime(2022, 5, 17), false));
				AssertEquals("Date > endDate", false, ZZRefCusCodeListCombined.Loader.ExistsWithinEffectiveDate(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "Test", new ZDateTime(2022, 5, 21), false));
				AssertEquals("Invalid code", false, ZZRefCusCodeListCombined.Loader.ExistsWithinEffectiveDate(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "XYZ", ZDate.Today, false));
				AssertEquals("Invalid type", false, ZZRefCusCodeListCombined.Loader.ExistsWithinEffectiveDate(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Test", new ZDateTime(2022, 5, 18), false));
				AssertEquals("Invalid dataGroupingCode", false, ZZRefCusCodeListCombined.Loader.ExistsWithinEffectiveDate(Factory, Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "Test", new ZDateTime(2022, 5, 18), false));
				AssertEquals("includeParentDataGrouping", true, ZZRefCusCodeListCombined.Loader.ExistsWithinEffectiveDate(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Test", new ZDateTime(2022, 5, 18), true));
			});
		}

		public void TestLoadDataGroupingsCodes()
		{
			var today = ZDateTime.Today;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var ieCountryCode = "IE";
			var ie5GroupingCode = "IE5";
			var zzzGroupingCode = "ZZZ";
			var codeType = "COD";
			var ieCode = helper.CreateCusCodeList(ieCountryCode, codeType, "Y966", "IE Code", yesterday, tomorrow);
			var ie5Code = helper.CreateCusCodeList(ie5GroupingCode, codeType, "3LLB81E", "IE5 Code", yesterday, tomorrow);
			var zzzCode = helper.CreateCusCodeList(zzzGroupingCode, codeType, "Z999", "ZZZ Code", yesterday, tomorrow);

			CombineAssertions(() =>
			{
				AssertContains("ZZD_CountryOrGrouping in ('IE', 'IE5'))", ZZRefCusCodeListCombined.Loader.GetFilter(Factory, new ZString[] { ieCountryCode, ie5GroupingCode }, new ZString[] { codeType }, ZDateTime.Today, null, true).LiteralTextADO);
				AssertContains("ZZD_CountryOrGrouping in ('IE', 'IE5', 'ZZZ'))", ZZRefCusCodeListCombined.Loader.GetFilter(Factory, new ZString[] { ieCountryCode, ie5GroupingCode, zzzGroupingCode }, new ZString[] { codeType }, ZDateTime.Today, null, true).LiteralTextADO);
			});
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new ZZRefCusCodeListCombined.Loader(Factory);
	}
}
