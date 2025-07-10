using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.RefCusCodeListTypes;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	class RefCusCodeListTypesTest : TestCaseWithFactory
	{
		//This test is here because MasterFiles.Biz could not test this lookup
		public void TestGetCustomsSupervisingOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.SupervisingOffice, "Supervising OFfice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, C.RefCusCodeListTypes.Codes.SupervisingOffice, "GBCK001", "My Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				OrgCusCode cusCode = Factory.New<OrgCusCode>();
				Assert("Customs Supervising Office should have a non-empty list", cusCode.Lookups.CustomsSupervisingOfficeList.Count == 1);
				Assert("Customs Supervising Office should be 'GBCK001'", cusCode.Lookups.CustomsSupervisingOfficeList[0].Code == "GBCK001");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				OrgCusCode cusCode = Factory.New<OrgCusCode>();
				Assert("Customs Supervising Office should have an empty list", cusCode.Lookups.CustomsSupervisingOfficeList.Count == 0);
			}
		}

		//This test is here because MasterFiles.Biz could not test this lookup
		public void TestGetCustomsOfficeOfExitList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, "Northern Ireland");
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, C.RefCusCodeListTypes.Codes.CustomsOffice, "GBCK001", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, C.RefCusCodeListTypes.Codes.CustomsOffice, "GBCO001", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var cusCode = Factory.New<OrgCusCode>();
				var lookups = cusCode.Lookups.CustomsOfficeOfExitList as BusinessObjectCollection;
				lookups.Load();

				AssertEquals("Customs Office Of Exit should have a non-empty list", 2, lookups.Count);
				Assert(lookups.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "GBCK001"));
				Assert(lookups.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "Customs Office"));

				Assert(lookups.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "GBCO001"));
				Assert(lookups.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "Customs Office"));

				Assert(lookups.FilterBusinessObjectDefaults.ContainsDefaultFor("Country/Region or Grouping GB:Property"));
				Assert(lookups.FilterBusinessObjectDefaults.ContainsDefaultFor("Country/Region or Grouping XI:Property"));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var cusCode = Factory.New<OrgCusCode>();
				var lookups = cusCode.Lookups.CustomsOfficeOfExitList as BusinessObjectCollection;
				lookups.Load();

				AssertEquals("Customs Office Of Exit List should have an empty list", 1, lookups.Count);
				Assert(lookups.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_CountryOrGrouping == Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes));
				Assert(lookups.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "GBCO001"));
				Assert(lookups.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "Customs Office"));

				Assert(lookups.FilterBusinessObjectDefaults.ContainsDefaultFor("Country/Region or Grouping AU:Property"));
				Assert(lookups.FilterBusinessObjectDefaults.ContainsDefaultFor("Country/Region or Grouping XI:Property"));
			}
		}

		//This test is here because MasterFiles.Biz could not test this lookup
		public void TestGetKRIndustrialParkCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeType(KoreaSouthComplianceInfo.IndustrialParkCusCodeType, "Industrial Park Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, KoreaSouthComplianceInfo.IndustrialParkCusCodeType, "101", "한국수출", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.KoreaSouth))
			{
				OrgCusCode cusCode = Factory.New<OrgCusCode>();
				var lookups = cusCode.Lookups.KRIndustrialParkCodeList as BusinessObjectCollection;
				lookups.Load();

				AssertEquals("KR Industrial Park Code List should have a non-empty list", 1, lookups.Count);
				Assert(lookups.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "101"));
				Assert(lookups.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "한국수출"));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				OrgCusCode cusCode = Factory.New<OrgCusCode>();
				var lookups = cusCode.Lookups.KRIndustrialParkCodeList as BusinessObjectCollection;
				lookups.Reload(true);

				AssertEquals("KR Industrial Park Code List should have an empty list", 0, lookups.Count);
			}
		}

		//This test is here because MasterFiles.Biz could not test this lookup
		public void TestGetKRBondedAreaCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01001001", "경의선철도 입출경검사장(지상)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.KoreaSouth))
			{
				OrgCusCode cusCode = Factory.New<OrgCusCode>();
				var lookups = cusCode.Lookups.KRBondedAreaCodeList as BusinessObjectCollection;
				lookups.Load();

				AssertEquals("KR Bonded Area Code List should have a non-empty list", 1, lookups.Count);
				Assert(lookups.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "01001001"));
				Assert(lookups.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "경의선철도 입출경검사장(지상)"));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				OrgCusCode cusCode = Factory.New<OrgCusCode>();
				var lookups = cusCode.Lookups.KRBondedAreaCodeList as BusinessObjectCollection;
				lookups.Reload(true);

				AssertEquals("KR Bonded Area Code List should have an empty list", 0, lookups.Count);
			}
		}

		public void TestGetCachedListWithLanguageCode_Empty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("AUPUM", "AU Packing Units of Measurement");
			var cusCodeList = helper.CreateNewOrGetExistingCusCodeList("AU", "AUPUM", "BLK", "Block", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = ZString.Empty;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var list = RefCusCodeListTypes.GetCachedList(Factory, "AU", "AUPUM", ZDateTime.Today);
				AssertEquals(1, list.Count);
				AssertEquals("BLK", list[0].Code);
				AssertEquals("Block", list[0].Description);
				list = RefCusCodeListTypes.GetCachedList(Factory, "AU", "AUPUM", ZDateTime.Today, languageCode: "EN");
				AssertEquals(1, list.Count);
				AssertEquals("BLK", list[0].Code);
				AssertEquals("Block", list[0].Description);
			}
		}

		public void TestGetCachedListWithLanguageCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ZHT", "繁體中文");
			helper.CreateNewOrGetExistingCusCodeType("TWPUM", "Taiwan Packing Units of Measurement");
			var cusCodeList = helper.CreateNewOrGetExistingCusCodeList("TW", "TWPUM", "BLK", "Block", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListLanguage(cusCodeList, "ZHT", "塊");
			Factory.Save();
			GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = "ZH-TW";
			var list = RefCusCodeListTypes.GetCachedList(Factory, "TW", "TWPUM", ZDateTime.Today);
			AssertEquals(1, list.Count);
			AssertEquals("BLK", list[0].Code);
			AssertEquals("塊", list[0].Description);
			list = RefCusCodeListTypes.GetCachedList(Factory, "TW", "TWPUM", ZDateTime.Today, languageCode: "EN");
			AssertEquals(1, list.Count);
			AssertEquals("BLK", list[0].Code);
			AssertEquals("Block", list[0].Description);
		}

		public void TestGetCachedListOnlyThisLanguage()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ZHT", "ChineseTraditional");
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Declaration Units of Quantity");
			var cusCodeList = helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "009", "头", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListLanguage(cusCodeList, "ZHT", "頭");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "029", "井", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var list = RefCusCodeListTypes.GetCachedList(Factory, "CN", "CUSUQ", ZDateTime.Today, languageCode: "ZHT");
			AssertEquals(2, list.Count);
			AssertEquals("029", list[1].Code);
			AssertEquals("井", list[1].Description);
			AssertSame(list, RefCusCodeListTypes.GetCachedList(Factory, "CN", "CUSUQ", ZDateTime.Today, languageCode: "ZHT"));

			list = RefCusCodeListTypes.GetCachedList(Factory, "CN", "CUSUQ", ZDateTime.Today, languageCode: "ZHT", onlyThisLanguage: true);
			AssertEquals(1, list.Count);
			AssertEquals("009", list[0].Code);
			AssertEquals("頭", list[0].Description);
			AssertSame(list, RefCusCodeListTypes.GetCachedList(Factory, "CN", "CUSUQ", ZDateTime.Today, languageCode: "ZHT", onlyThisLanguage: true));
		}

		public void TestGetCachedListShouldSpecifyLanguageCodeWhenOnlyThisLanguageIsTrue()
		{
			AssertExceptionThrown<ArgumentException>("When onlyThisLanguage is true but languageCode is empty", "Should specify languageCode when onlyThisLanguage is true.\r\nParameter name: languageCode", () => RefCusCodeListTypes.GetCachedList(Factory, "CN", "CUSUQ", ZDateTime.Today, onlyThisLanguage: true));
			AssertNoExceptionThrown("When onlyThisLanguage is true but languageCode has value", () => RefCusCodeListTypes.GetCachedList(Factory, "CN", "CUSUQ", ZDateTime.Today, languageCode: "ZHT", onlyThisLanguage: true));
		}

		public void TestGetCustomsPackListIncludingForRefPack()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, C.RefCusCodeListTypes.Codes.PackageTypes, "TINY", "My Tiny Package", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var refPacks = Factory.New<CusRefPacks>();
				Assert("Customs Pack should have a non-empty list", refPacks.RP_CustomsPack_List.Count == 21);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var refPacks = Factory.New<CusRefPacks>();
				AssertEquals("TINY", refPacks.RP_CustomsPack_List[0].Code);
			}
		}

		public void TestGetListIsCached()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			codeList.ZZD_Code = "#ZZ";
			codeList.ZZD_Description = "BOB THE BUILDER";
			codeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.SouthAfrica;
			codeList.ZZD_CodeType = C.RefCusCodeListTypes.Codes.CustomsOffice;
			codeList.ZZD_StartDate = ZDateTime.Today.AddDays(-1);
			codeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			Factory.Save();
			var list1 = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddMonths(-1));
			var list2 = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddMonths(-1));
			AssertEquals("IsCached", true, ReferenceEquals(list1, list2));
			var list3 = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			var list4 = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			AssertEquals("IsCached", false, ReferenceEquals(list3, list1));
			AssertEquals("IsCached", true, ReferenceEquals(list3, list4));
			AssertEquals("#ZZ, " + list1.CodesAsString, list3.CodesAsString);
			var dictionary = Factory.GetCachedValue("", () => new Dictionary<ZString, CodeDescriptionPairList>());
			AssertEquals(0, dictionary.Count);
		}

		public void TestGetListWithAttributeNameIsCached()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("A", "Desc.", C.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("B", "Desc.", C.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("C", "Desc.", C.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("S", "Desc.", C.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code1.Attributes.AddNew("A", "S");
			code1.Attributes.AddNew("B", "G");
			helper.CreateTransportModeForCusCodeList(code1.PK, "SEA");
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code2.Attributes.AddNew("A", "S");
			code2.Attributes.AddNew("C", "S");
			helper.CreateTransportModeForCusCodeList(code2.PK, "SEA");
			var code3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "3", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code3.Attributes.AddNew("A", "S");
			code3.Attributes.AddNew("B", "S");
			helper.CreateTransportModeForCusCodeList(code3.PK, "SEA");
			var code4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "4", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code4.Attributes.AddNew("A", "S");
			code4.Attributes.AddNew("S", "D");
			helper.CreateTransportModeForCusCodeList(code4.PK, "AIR");
			Factory.Save();
			var list1 = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new ZString[] { "A", "B" }, "SEA");
			var list2 = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new ZString[] { "B", "A" }, "SEA");
			var list3 = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new ZString[] { "B", "A", "" }, "SEA");
			var list4 = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new ZString[] { "A", "S" }, "AIR");
			AssertEquals("IsCached", true, ReferenceEquals(list1, list2));
			AssertEquals("IsCached", false, ReferenceEquals(list1, list3));
			AssertEquals("IsCached", false, ReferenceEquals(list1, list4));
			AssertEquals(2, list1.Count);
			AssertEquals(true, list1.ContainsCode("1"));
			AssertEquals(false, list1.ContainsCode("2"));
			AssertEquals(true, list1.ContainsCode("3"));
			AssertEquals(false, list1.ContainsCode("4"));
			AssertEquals(0, list3.Count);
			AssertEquals(1, list4.Count);
		}

		public void TestGetListMatchAllAttributeIsCached()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("A", "Desc.", C.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code1.Attributes.AddNew("A", "B");
			code1.Attributes.AddNew("A", "C");
			helper.CreateTransportModeForCusCodeList(code1.PK, "SEA");
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code2.Attributes.AddNew("A", "B");
			code2.Attributes.AddNew("A", "D");
			helper.CreateTransportModeForCusCodeList(code2.PK, "SEA");
			var code3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "3", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code3.Attributes.AddNew("A", "C");
			code3.Attributes.AddNew("A", "D");
			helper.CreateTransportModeForCusCodeList(code3.PK, "SEA");
			var code4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "4", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code4.Attributes.AddNew("A", "B");
			code4.Attributes.AddNew("A", "C");
			code4.Attributes.AddNew("A", "D");
			helper.CreateTransportModeForCusCodeList(code4.PK, "AIR");
			Factory.Save();
			var list1 = RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "B") }, "SEA");
			var list2 = RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "B"), new KeyValuePair<ZString, ZString>("A", "C") }, "SEA");
			var list3 = RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "C") }, "AIR");
			var list4 = RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "B"), new KeyValuePair<ZString, ZString>("A", "C") }, "AIR");
			var list5 = RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "C"), new KeyValuePair<ZString, ZString>("A", "B") }, "AIR");
			AssertEquals("IsCached", false, ReferenceEquals(list1, list2));
			AssertEquals("IsCached", false, ReferenceEquals(list1, list3));
			AssertEquals("IsCached", false, ReferenceEquals(list1, list4));
			AssertEquals("IsCached", true, ReferenceEquals(list4, list5));
			Assert("SEA-A:B", list1.ContainsCode("1") && list1.ContainsCode("2") && !list1.ContainsCode("3") && !list1.ContainsCode("4"));
			Assert("SEA-A:B-A:C", list2.ContainsCode("1") && !list2.ContainsCode("2") && !list2.ContainsCode("3") && !list2.ContainsCode("4"));
			Assert("AIR-A:C", !list3.ContainsCode("1") && !list3.ContainsCode("2") && !list3.ContainsCode("3") && list3.ContainsCode("4"));
			Assert("AIR-A:B-A:C", !list4.ContainsCode("1") && !list4.ContainsCode("2") && !list4.ContainsCode("3") && list4.ContainsCode("4"));
		}

		public void TestGetCachedListMatchAllAttributesParentData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("FR", "French");
			var eun = helper.CreateNewOrGetExistingDataGrouping(C.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia, "Australia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", eun);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.TranNature, "Tran Nature");
			helper.CreateNewOrGetExistingCusCodeList(C.RefDataGrouping.Codes.EuropeanUnionEUN, C.RefCusCodeListTypes.Codes.TranNature, "AAA", "AAA DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, C.RefCusCodeListTypes.Codes.TranNature, "BBB", "BBB DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, C.RefCusCodeListTypes.Codes.TranNature, "CCC", "CCC DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var frenchCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, C.RefCusCodeListTypes.Codes.TranNature, "DDD", "DDD DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateOrGetLanguage("FR", "French");
			helper.CreateNewOrGetExistingCusCodeListLanguage(frenchCode, "FR", "Description du code DDD");
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = "FR";
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var listUK_Unknown = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.UnitedKingdom, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.Unknown);
				var listDE_Unknown = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.Germany, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.Unknown);
				var listAU_Unknown = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.Australia, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.Unknown);
				var listFR_Unknown = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.France, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.Unknown);
				AssertNullOrEmpty("Unknown type of list should always be empty", listUK_Unknown.CodesAsString);
				AssertNullOrEmpty("Unknown type of list should always be empty", listDE_Unknown.CodesAsString);
				AssertNullOrEmpty("Unknown type of list should always be empty", listAU_Unknown.CodesAsString);
				AssertNullOrEmpty("Unknown type of list should always be empty", listFR_Unknown.CodesAsString);
				var listUK_ChildOnly = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.UnitedKingdom, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildOnly);
				var listDE_ChildOnly = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.Germany, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildOnly);
				var listAU_ChildOnly = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.Australia, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildOnly);
				var listFR_ChildOnly = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.France, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildOnly);
				Assert("UK Child only type of list should only feature UK codes", listUK_ChildOnly.ContainsCode("BBB") && !listUK_ChildOnly.ContainsCode("AAA"));
				AssertNullOrEmpty("DE Child only type of list should be empty as there has been no code defined for DE", listDE_ChildOnly.CodesAsString);
				Assert("AU Child only type of list should only feature AU codes", listAU_ChildOnly.ContainsCode("CCC") && !listAU_ChildOnly.ContainsCode("AAA"));
				Assert("FR Child only type of list should only feature FR codes", listFR_ChildOnly.ContainsCode("DDD") && !listFR_ChildOnly.ContainsCode("AAA"));
				AssertContains("The FR list should contain the description in FR language", "Description du code DDD", listFR_ChildOnly.GetDescriptionFromCode("DDD"));
				var listUK_Union = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.UnitedKingdom, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.Union);
				var listDE_Union = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.Germany, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.Union);
				var listAU_Union = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.Australia, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.Union);
				var listFR_Union = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.France, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.Union);
				Assert("UK Union type of list should feature UK + EU codes", listUK_Union.ContainsCode("BBB") && listUK_Union.ContainsCode("AAA"));
				Assert("DE Union type of list should only feature EU codes as there has been no code defined for DE", listDE_Union.ContainsCode("AAA"));
				Assert("AU Union list should only feature AU codes as EU is not a valid parent for AU", listAU_Union.ContainsCode("CCC"));
				Assert("FR Union type of list should feature FR + EU codes", listFR_Union.ContainsCode("DDD") && listFR_Union.ContainsCode("AAA"));
				AssertContains("The FR list should contain the description in FR language", "Description du code DDD", listFR_Union.GetDescriptionFromCode("DDD"));
				var listUK_ChildFirstThenParent = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.UnitedKingdom, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildFirstThenParent);
				var listDE_ChildFirstThenParent = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.Germany, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildFirstThenParent);
				var listAU_ChildFirstThenParent = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.Australia, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildFirstThenParent);
				var listFR_ChildFirstThenParent = GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.France, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new Dictionary<ZString, ZString>().ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildFirstThenParent);
				Assert("UK ChildFirstThenParent type list should only feature UK codes, because there are UK codes", listUK_ChildFirstThenParent.ContainsCode("BBB") && !listUK_ChildFirstThenParent.ContainsCode("AAA"));
				Assert("DE ChildFirstThenParent list should only feature EU codes as there has been no code defined for DE", listDE_ChildFirstThenParent.ContainsCode("AAA"));
				Assert("AU ChildFirstThenParent type list should only feature AU codes, because there are AU codes", listAU_ChildFirstThenParent.ContainsCode("CCC") && !listAU_ChildFirstThenParent.ContainsCode("AAA"));
				Assert("FR ChildFirstThenParent type list should only feature FR codes, because there are FR codes", listFR_ChildFirstThenParent.ContainsCode("DDD") && !listFR_ChildFirstThenParent.ContainsCode("AAA"));
				AssertContains("The FR list should contain the description in FR language", "Description du code DDD", listFR_ChildFirstThenParent.GetDescriptionFromCode("DDD"));
			}
		}

		public void TestGetListMatchAnyAttributeIsCached()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("A", "Desc.", C.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code1.Attributes.AddNew("A", "B");
			code1.Attributes.AddNew("A", "C");
			helper.CreateTransportModeForCusCodeList(code1.PK, "SEA");
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code2.Attributes.AddNew("A", "B");
			code2.Attributes.AddNew("A", "D");
			helper.CreateTransportModeForCusCodeList(code2.PK, "SEA");
			var code3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "3", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code3.Attributes.AddNew("A", "C");
			code3.Attributes.AddNew("A", "D");
			helper.CreateTransportModeForCusCodeList(code3.PK, "SEA");
			var code4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "4", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code4.Attributes.AddNew("A", "B");
			code4.Attributes.AddNew("A", "C");
			code4.Attributes.AddNew("A", "D");
			helper.CreateTransportModeForCusCodeList(code4.PK, "AIR");
			Factory.Save();
			var list1 = RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "B") }, "SEA");
			var list2 = RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "B"), new KeyValuePair<ZString, ZString>("A", "C") }, "SEA");
			var list3 = RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "D") }, "AIR");
			var list4 = RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "B"), new KeyValuePair<ZString, ZString>("A", "E") }, "AIR");
			var list5 = RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "B"), new KeyValuePair<ZString, ZString>("A", "E") }, "AIR");
			AssertEquals("IsCached", false, ReferenceEquals(list1, list2));
			AssertEquals("IsCached", false, ReferenceEquals(list1, list3));
			AssertEquals("IsCached", false, ReferenceEquals(list1, list4));
			AssertEquals("IsCached", false, ReferenceEquals(list4, list5));
			Assert("SEA-A:B", list1.ContainsCode("1") && list1.ContainsCode("2") && !list1.ContainsCode("3") && !list1.ContainsCode("4"));
			Assert("SEA-A:B-A:C", list2.ContainsCode("1") && list2.ContainsCode("2") && list2.ContainsCode("3") && !list2.ContainsCode("4"));
			Assert("AIR-A:D", !list3.ContainsCode("1") && !list3.ContainsCode("2") && !list3.ContainsCode("3") && list3.ContainsCode("4"));
			Assert("AIR-A:B-A:E", !list4.ContainsCode("1") && !list4.ContainsCode("2") && !list4.ContainsCode("3") && list4.ContainsCode("4"));
		}

		public void TestGetListMatchAnyAttributeParentData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(C.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", eun);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.TranNature, "Tran Nature");
			var code1 = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.France, C.RefCusCodeListTypes.Codes.TranNature, "AAA", "AAA DESC", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "A", "B");
			code1.Attributes.AddNew("A", "C");
			var code2 = helper.CreateCusCodeListWithAttribute(C.RefDataGrouping.Codes.EuropeanUnionEUN, C.RefCusCodeListTypes.Codes.TranNature, "DDD", "DDD DESC", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "A", "B");
			code2.Attributes.AddNew("A", "D");
			Factory.Save();

			var listFR_ChildOnly = RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.France, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "B") }, includeParentDataGrouping: false);
			var listFR_Union = RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.France, C.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today.AddMonths(-1), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "B") }, includeParentDataGrouping: true);
			AssertEquals("FR Child only type of list should only feature FR codes", "AAA", listFR_ChildOnly.CodesAsString);
			Assert("FR Union type of list should feature FR + EU codes", listFR_Union.ContainsCode("AAA") && listFR_Union.ContainsCode("DDD"));
		}

		public void TestGetListMatchSingleAttributeIsCached()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("A", "Desc.", C.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code1.Attributes.AddNew("A", "B");
			code1.Attributes.AddNew("A", "C");
			helper.CreateTransportModeForCusCodeList(code1.PK, "SEA");
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTransportModeForCusCodeList(code2.PK, "SEA");
			var code3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "3", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code3.Attributes.AddNew("A", "B");
			code3.Attributes.AddNew("A", "C");
			helper.CreateTransportModeForCusCodeList(code3.PK, "SEA");
			var code4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "4", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code4.Attributes.AddNew("A", "B");
			code4.Attributes.AddNew("A", "C");
			code4.Attributes.AddNew("A", "D");
			helper.CreateTransportModeForCusCodeList(code4.PK, "AIR");
			Factory.Save();
			var list1 = RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), true, "A", new ZString[] { "B" }, "SEA");
			var list2 = RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), false, "A", new ZString[] { "B" }, "SEA");
			var list3 = RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), false, "A", new ZString[] { "C" }, "SEA");
			var list4 = RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), false, "A", new ZString[] { "B", "C" }, "AIR");
			var list5 = RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), false, "A", new ZString[] { "C", "B" }, "AIR");
			AssertEquals("IsCached", false, ReferenceEquals(list1, list2));
			AssertEquals("IsCached", false, ReferenceEquals(list1, list3));
			AssertEquals("IsCached", false, ReferenceEquals(list1, list4));
			AssertEquals("IsCached", true, ReferenceEquals(list5, list4));
			AssertEquals(3, list1.Count);
			AssertEquals(2, list2.Count);
			AssertEquals(2, list3.Count);
			AssertEquals(1, list4.Count);
			AssertEquals(true, list1.ContainsCode("1"));
			AssertEquals(true, list1.ContainsCode("2"));
			AssertEquals(true, list1.ContainsCode("3"));
			AssertEquals(false, list1.ContainsCode("4"));
			AssertEquals(true, list2.ContainsCode("1"));
			AssertEquals(false, list2.ContainsCode("2"));
			AssertEquals(true, list2.ContainsCode("3"));
			AssertEquals(false, list2.ContainsCode("4"));
			AssertEquals(false, list3.ContainsCode("4"));
			AssertEquals(true, list4.ContainsCode("4"));
		}

		public void TestGetCachedListByTransportMode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTransportModeForCusCodeList(code1.PK, "SEA");
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTransportModeForCusCodeList(code2.PK, "AIR");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "3", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			var list1 = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1));
			var list2 = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), transportMode: "SEA");
			var list3 = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-1), transportMode: "AIR");
			AssertEquals("IsCached", false, ReferenceEquals(list1, list2));
			AssertEquals("IsCached", false, ReferenceEquals(list1, list3));
			AssertEquals(3, list1.Count);
			AssertEquals(true, list1.ContainsCode("1"));
			AssertEquals(true, list1.ContainsCode("2"));
			AssertEquals(true, list1.ContainsCode("3"));
			AssertEquals(2, list2.Count);
			AssertEquals(true, list2.ContainsCode("1"));
			AssertEquals(false, list2.ContainsCode("2"));
			AssertEquals(true, list2.ContainsCode("3"));
			AssertEquals(2, list3.Count);
			AssertEquals(false, list3.ContainsCode("1"));
			AssertEquals(true, list3.ContainsCode("2"));
			AssertEquals(true, list3.ContainsCode("3"));
		}

		public void TestMultilingualList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("A", "Desc.", C.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("B", "Desc.", C.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "1", "One", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code1.Attributes.AddNew("A", "A");
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "2", "Two", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code2.Attributes.AddNew("B", "B");
			Factory.Save();
			helper.CreateOrGetLanguage("DE", "German");
			helper.CreateNewOrGetExistingCusCodeListLanguage(code1, "DE", "Eins");
			helper.CreateOrGetLanguage("AF", "Afrikaans");
			helper.CreateNewOrGetExistingCusCodeListLanguage(code2, "AF", "Twee");
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = "EN";
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var list = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-2));
				AssertEquals("Code in English", "One", list.GetDescriptionFromCode("1"));
				AssertEquals("Code in English", "Two", list.GetDescriptionFromCode("2"));
			}

			currentUser.GS_WorkingLanguage = "DE-DE";
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var list = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-2));
				AssertEquals("Code in German", "Eins", list.GetDescriptionFromCode("1"));
				AssertEquals("Code in English since German is not set", "Two", list.GetDescriptionFromCode("2"));
				list = RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-2), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "A") });
				AssertEquals("Only one match", 1, list.Count);
				AssertEquals("Code in German", "Eins", list.GetDescriptionFromCode("1"));
				list = RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-2), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("A", "A") });
				AssertEquals("Only one match", 1, list.Count);
				AssertEquals("Code in German", "Eins", list.GetDescriptionFromCode("1"));
				list = RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-2), false, "A", new ZString[] { "A" });
				AssertEquals("Only one match", 1, list.Count);
				AssertEquals("Code in German", "Eins", list.GetDescriptionFromCode("1"));
			}

			currentUser.GS_WorkingLanguage = "AF-ZA";
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var list = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-2));
				AssertEquals("Code in English since Afrikaans is not set", "One", list.GetDescriptionFromCode("1"));
				AssertEquals("Code in Afrikaans", "Twee", list.GetDescriptionFromCode("2"));
				list = RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-2), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("B", "B") });
				AssertEquals("Only one match", 1, list.Count);
				AssertEquals("Code in Afrikaans", "Twee", list.GetDescriptionFromCode("2"));
				list = RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-2), new KeyValuePair<ZString, ZString>[] { new KeyValuePair<ZString, ZString>("B", "B") });
				AssertEquals("Only one match", 1, list.Count);
				AssertEquals("Code in Afrikaans", "Twee", list.GetDescriptionFromCode("2"));
				list = RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today.AddMonths(-2), false, "B", new ZString[] { "B" });
				AssertEquals("Only one match", 1, list.Count);
				AssertEquals("Code in Afrikaans", "Twee", list.GetDescriptionFromCode("2"));
			}
		}

		public void TestSortRefCusCodeList()
		{
			const string zzCodeType = "Z!Z";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(zzCodeType, "DESC");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, zzCodeType, "21", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, zzCodeType, "2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, zzCodeType, "11", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, zzCodeType, "1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			var list = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, ZDateTime.Today.AddMonths(-1));
			AssertEquals("1", list[0].Code);
			AssertEquals("2", list[1].Code);
			AssertEquals("11", list[2].Code);
			AssertEquals("21", list[3].Code);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, zzCodeType, "X", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			list = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, ZDateTime.Today.AddMonths(-2));
			AssertEquals("1", list[0].Code);
			AssertEquals("11", list[1].Code);
			AssertEquals("2", list[2].Code);
			AssertEquals("21", list[3].Code);
			AssertEquals("X", list[4].Code);
		}

		public void TestCorrectCachingInFactory()
		{
			const string zzCodeType = "Z!Z";
			var date = ZDateTime.Today.AddYears(-1);
			var list = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date);
			AssertSame(list, RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date));
			var dictionary = Factory.GetCachedValue("ZZRefCusCodeList_" + RefCusCodeListTypes.GetKey(Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date.Date, "", true, null), () => new Dictionary<ZString, CodeDescriptionPairList>());
			AssertEquals(0, dictionary.Count);
			list = RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date, null, "", includeParentDataGrouping: IncludeParentDataGroupingOptions.Union);
			AssertSame(list, RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date, null, "", includeParentDataGrouping: IncludeParentDataGroupingOptions.Union));
			dictionary = Factory.GetCachedValue("ZZRefCusCodeList_All_" + RefCusCodeListTypes.GetKey(Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date.Date, "", true), () => new Dictionary<ZString, CodeDescriptionPairList>());
			AssertEquals(0, dictionary.Count);
			list = RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date, null, null, "", includeParentDataGrouping: IncludeParentDataGroupingOptions.Union);
			AssertSame(list, RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date, null, null, "", includeParentDataGrouping: IncludeParentDataGroupingOptions.Union));
			dictionary = Factory.GetCachedValue("ZZRefCusCodeList_All_NoExists_" + RefCusCodeListTypes.GetKey(Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date.Date, "", true), () => new Dictionary<ZString, CodeDescriptionPairList>());
			AssertEquals(0, dictionary.Count);
			list = RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date, null, "");
			AssertSame(list, RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date, null, ""));
			dictionary = Factory.GetCachedValue("ZZRefCusCodeList_Any_" + RefCusCodeListTypes.GetKey(Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date.Date, "", true), () => new Dictionary<ZString, CodeDescriptionPairList>());
			AssertEquals(0, dictionary.Count);
			list = RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date, true, ZString.Empty, null, "");
			AssertSame(list, RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date, true, ZString.Empty, null, ""));
			dictionary = Factory.GetCachedValue("ZZRefCusCodeList_Single_" + RefCusCodeListTypes.GetKey(Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date.Date, "", true), () => new Dictionary<ZString, CodeDescriptionPairList>());
			AssertEquals(0, dictionary.Count);
			list = RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date, null, "", true);
			AssertSame(list, RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date, null, "", true));
			dictionary = Factory.GetCachedValue("ZZRefCusCodeList_Before_" + RefCusCodeListTypes.GetKey(Core.Constants.CountryCodes.SouthAfrica, zzCodeType, date.Date, "", true, null), () => new Dictionary<ZString, CodeDescriptionPairList>());
			AssertEquals(0, dictionary.Count);
		}

		public void TestEmptyCountryQueries()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var codeA = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, "AAA", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("A", "Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice);
			codeA.Attributes.AddNew("A", "B");
			codeA.Attributes.AddNew("A", "C");
			helper.CreateTransportModeForCusCodeList(codeA.PK, "SEA");
			var codeB = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities, "BBB", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("A", "Desc.", C.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.Facilities);
			codeB.Attributes.AddNew("A", "B");
			codeB.Attributes.AddNew("A", "C");
			helper.CreateTransportModeForCusCodeList(codeB.PK, "SEA");
			Factory.Save();
			var attributes = new[] { new KeyValuePair<ZString, ZString>("A", "B") };
			AssertEquals("AAA", RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddDays(1)).CodesAsString);
			AssertEquals("AAA", RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddDays(2)).CodesAsString);
			AssertEquals("AAA", RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddDays(3), attributes).CodesAsString);
			AssertEquals("AAA", RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddDays(4), attributes).CodesAsString);
			AssertEquals("AAA", RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddDays(5), true, "A", new ZString[] { "B" }, "SEA").CodesAsString);
			AssertEquals(ZString.Empty, RefCusCodeListTypes.GetCachedList(Factory, ZString.Empty, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddDays(1)).CodesAsString);
			AssertEquals(ZString.Empty, RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, ZString.Empty, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddDays(2)).CodesAsString);
			AssertEquals(ZString.Empty, RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, ZString.Empty, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddDays(3), attributes).CodesAsString);
			AssertEquals(ZString.Empty, RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory, ZString.Empty, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddDays(4), attributes).CodesAsString);
			AssertEquals(ZString.Empty, RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, ZString.Empty, C.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today.AddDays(5), true, "A", new ZString[] { "B" }, "SEA").CodesAsString);
		}
	}
}
