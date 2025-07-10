using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ZARefCusCodeListTypesTest : TestCaseWithFactory
	{
		public void TestGetZADocumentTypeList()
		{
			var list1 = ZARefCusCodeListTypes.GetZADocumentTypeList(Factory);
			var list2 = ZARefCusCodeListTypes.GetZADocumentTypeList(Factory);
			AssertSame("IsCached", list1, list2);
			var collection = new ZARefCusCodeListCollection(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ZADocumentType, ZDateTime.Today);
			collection.Load();
			var count = collection.Count;
			AssertEquals("PreCondition", count, list1.Count);
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			codeList.ZZD_Code = "ZZZ";
			codeList.ZZD_Description = "Doc Type ZZ";
			codeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.SouthAfrica;
			codeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ZADocumentType;
			codeList.ZZD_StartDate = ZDateTime.Today;
			codeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			Factory.Save();
			list1 = ZARefCusCodeListTypes.GetZADocumentTypeList(new BusinessObjectFactory());
			AssertEquals(count + 1, list1.Count);
			AssertEquals(codeList.ZZD_Description, list1.GetDescriptionFromCode(codeList.ZZD_Code));
		}

		public void TestGetBankCodeList()
		{
			var list1 = ZARefCusCodeListTypes.GetBankCodeList(Factory);
			var list2 = ZARefCusCodeListTypes.GetBankCodeList(Factory);
			AssertSame("IsCached", list1, list2);
			var collection = new ZARefCusCodeListCollection(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, ZDateTime.Today);
			collection.Load();
			var count = collection.Count;
			AssertEquals("PreCondition", count, list1.Count);
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			codeList.ZZD_Code = "091";
			codeList.ZZD_Description = "STANDARD CHARTERED BANK";
			codeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.SouthAfrica;
			codeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode;
			codeList.ZZD_StartDate = ZDateTime.Today;
			codeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			Factory.Save();
			list1 = ZARefCusCodeListTypes.GetBankCodeList(new BusinessObjectFactory());
			AssertEquals(count + 1, list1.Count);
			AssertEquals("STANDARD CHARTERED BANK", list1.GetDescriptionFromCode("091"));
		}

		public void TestGetAdditionalInformationAttributeValuesFor_Pair()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry("DCC");
			testHelper.CreateAdditionalInformationCusCodeEntry("DCV");
			testHelper.CreateAdditionalInformationCusCodeEntry("RCC");
			testHelper.CreateAdditionalInformationCusCodeEntry("RCV");
			Factory.Save();
			var codes1 = ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(Factory, ZDateTime.Today, "DCC", RefCusCodeListAttributeTypes.Codes.Pair);
			var codes2 = ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(Factory, ZDateTime.Today, "DCC", RefCusCodeListAttributeTypes.Codes.Pair);
			AssertEquals("IsCached", true, object.ReferenceEquals(codes1, codes2));
			AssertEquals(1, codes1.Length);
			AssertEquals("DCV", codes1[0]);
			codes1 = ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(Factory, ZDateTime.Today, "DCV", RefCusCodeListAttributeTypes.Codes.Pair);
			codes2 = ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(Factory, ZDateTime.Today, "DCV", RefCusCodeListAttributeTypes.Codes.Pair);
			AssertEquals("IsCached", true, object.ReferenceEquals(codes1, codes2));
			AssertEquals(1, codes1.Length);
			AssertEquals("DCC", codes1[0]);
			codes1 = ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(Factory, ZDateTime.Today, "RCC", RefCusCodeListAttributeTypes.Codes.Pair);
			codes2 = ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(Factory, ZDateTime.Today, "RCC", RefCusCodeListAttributeTypes.Codes.Pair);
			AssertEquals("IsCached", true, object.ReferenceEquals(codes1, codes2));
			AssertEquals(1, codes1.Length);
			AssertEquals("RCV", codes1[0]);
			codes1 = ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(Factory, ZDateTime.Today, "RCV", RefCusCodeListAttributeTypes.Codes.Pair);
			codes2 = ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(Factory, ZDateTime.Today, "RCV", RefCusCodeListAttributeTypes.Codes.Pair);
			AssertEquals("IsCached", true, object.ReferenceEquals(codes1, codes2));
			AssertEquals(1, codes1.Length);
			AssertEquals("RCC", codes1[0]);
		}

		public void TestGetAdditionalInformationsMappedToSchedule()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry("ADI");
			testHelper.CreateAdditionalInformationCusCodeEntry("CVI");
			testHelper.CreateAdditionalInformationCusCodeEntry("SGI");
			Factory.Save();
			var list1 = ZARefCusCodeListTypes.GetAdditionalInformationsMappedToSchedule(Factory, ZDateTime.Today, "2P1");
			var list2 = ZARefCusCodeListTypes.GetAdditionalInformationsMappedToSchedule(Factory, ZDateTime.Today, "2P1");
			AssertEquals("IsCached", true, object.ReferenceEquals(list1, list2));
			AssertEquals("Length", 1, list1.Length);
			AssertEquals("ADI", list1[0]);
			var list3 = ZARefCusCodeListTypes.GetAdditionalInformationsMappedToSchedule(Factory, ZDateTime.Today, "2P2");
			var list4 = ZARefCusCodeListTypes.GetAdditionalInformationsMappedToSchedule(Factory, ZDateTime.Today, "2P2");
			AssertEquals("Not cached", false, object.ReferenceEquals(list3, list1));
			AssertEquals("IsCached", true, object.ReferenceEquals(list3, list4));
			AssertEquals("Length", 1, list3.Length);
			AssertEquals("CVI", list3[0]);
			var list5 = ZARefCusCodeListTypes.GetAdditionalInformationsMappedToSchedule(Factory, ZDateTime.Today, "2P3");
			var list6 = ZARefCusCodeListTypes.GetAdditionalInformationsMappedToSchedule(Factory, ZDateTime.Today, "2P3");
			AssertEquals("Not cached", false, object.ReferenceEquals(list5, list1));
			AssertEquals("Not cached", false, object.ReferenceEquals(list5, list3));
			AssertEquals("IsCached", true, object.ReferenceEquals(list5, list6));
			AssertEquals("Length", 1, list5.Length);
			AssertEquals("SGI", list5[0]);
		}

		public void TestGetCustomsOfficeList()
		{
			var list1 = ZARefCusCodeListTypes.GetCustomsOfficeList(Factory);
			var list2 = ZARefCusCodeListTypes.GetCustomsOfficeList(Factory);
			AssertSame("IsCached", list1, list2);
			var collection = new ZARefCusCodeListCollection(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			collection.Load();
			var count = collection.Count;
			AssertEquals("PreCondition", count, list1.Count);
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			codeList.ZZD_Code = "A#@";
			codeList.ZZD_Description = "BOB THE BUILDER";
			codeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.SouthAfrica;
			codeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			codeList.ZZD_StartDate = ZDateTime.Today;
			codeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			Factory.Save();
			list1 = ZARefCusCodeListTypes.GetCustomsOfficeList(new BusinessObjectFactory());
			AssertEquals(count + 1, list1.Count);
			AssertEquals("BOB THE BUILDER", list1.GetDescriptionFromCode("A#@"));
		}

		public void TestGetCustomsStatusList()
		{
			var list1 = ZARefCusCodeListTypes.GetCustomsStatusList(Factory);
			var list2 = ZARefCusCodeListTypes.GetCustomsStatusList(Factory);
			AssertSame("IsCached", list1, list2);
			var collection = new ZARefCusCodeListCollection(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today);
			collection.Load();
			var count = collection.Count;
			AssertEquals("PreCondition", count, list1.Count);
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			codeList.ZZD_Code = "99";
			codeList.ZZD_Description = "BOB THE BUILDER";
			codeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.SouthAfrica;
			codeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus;
			codeList.ZZD_StartDate = ZDateTime.Today.AddYears(-1);
			codeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			Factory.Save();
			list1 = ZARefCusCodeListTypes.GetCustomsStatusList(new BusinessObjectFactory());
			AssertEquals(count + 1, list1.Count);
			AssertEquals("BOB THE BUILDER", list1.GetDescriptionFromCode("99"));
		}

		public void TestGetAddInWithROOTypeAttribute()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			testHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "AdditionalInformation");
			var codeList1 = testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "XX", startDate, endDate);
			testHelper.CreateCusCodeListAttribute(codeList1.PK, RefCusCodeListAttributeTypes.Codes.ROOType, "");
			var codeList2 = testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "YY", startDate, endDate);
			testHelper.CreateCusCodeListAttribute(codeList2.PK, RefCusCodeListAttributeTypes.Codes.ROOType, "");
			Factory.Save();
			var list1 = ZARefCusCodeListTypes.GetAddInWithROOTypeAttribute(Factory, ZDateTime.Today);
			var list2 = ZARefCusCodeListTypes.GetAddInWithROOTypeAttribute(Factory, ZDateTime.Today);
			AssertEquals("IsCached", true, object.ReferenceEquals(list1, list2));
			Assert("Test 1", list1.ContainsCode("XX"));
			Assert("Test 2", list1.ContainsCode("YY"));
			AssertEquals("Description", "XX DESC", list1.GetDescriptionFromCode("XX"));
		}

		public void TestGetAdditionalInformationList()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "AdditionalInformation");
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var codeList1 = testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "IMPL1", startDate, endDate);
			testHelper.CreateCusCodeListAttribute(codeList1.PK, RefCusCodeListAttributeTypes.Codes.OnlyForLine1, "");
			testHelper.CreateCusCodeListAttribute(codeList1.PK, RefCusCodeListAttributeTypes.Codes.Import, "");
			var codeList2 = testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "IMPLX", startDate, endDate);
			testHelper.CreateCusCodeListAttribute(codeList2.PK, RefCusCodeListAttributeTypes.Codes.Import, "");
			var codeList3 = testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "EXPL1", startDate, endDate);
			testHelper.CreateCusCodeListAttribute(codeList3.PK, RefCusCodeListAttributeTypes.Codes.OnlyForLine1, "");
			testHelper.CreateCusCodeListAttribute(codeList3.PK, RefCusCodeListAttributeTypes.Codes.Export, "");
			var codeList4 = testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "EXPLX", startDate, endDate);
			testHelper.CreateCusCodeListAttribute(codeList4.PK, RefCusCodeListAttributeTypes.Codes.Export, "");
			Factory.Save();
			var list1 = ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, true, true);
			var list2 = ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, true, true);
			AssertEquals("IsCached", true, object.ReferenceEquals(list1, list2));
			Assert("Export Line 1 Only", list1.ContainsCode("EXPL1"));
			AssertEquals("Description", "EXPL1 DESC", list1.GetDescriptionFromCode("EXPL1"));
			Assert("Any Line", list1.ContainsCode("EXPLX"));
			AssertEquals("Description", "EXPLX DESC", list1.GetDescriptionFromCode("EXPLX"));
			var list3 = ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, true, false);
			var list4 = ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, true, false);
			AssertEquals(false, object.ReferenceEquals(list3, list1));
			AssertEquals("IsCached", true, object.ReferenceEquals(list3, list4));
			Assert("Export Line 1 Only", !list3.ContainsCode("EXPL1"));
			AssertNull("Description", list3.GetDescriptionFromCode("EXPL1"));
			Assert("Any Line", list3.ContainsCode("EXPLX"));
			AssertEquals("Description", "EXPLX DESC", list3.GetDescriptionFromCode("EXPLX"));
			var list5 = ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, false, true);
			var list6 = ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, false, true);
			AssertEquals(false, object.ReferenceEquals(list5, list1));
			AssertEquals(false, object.ReferenceEquals(list5, list3));
			AssertEquals("IsCached", true, object.ReferenceEquals(list5, list6));
			Assert("Import Line 1 Only", list5.ContainsCode("IMPL1"));
			AssertEquals("Description", "IMPL1 DESC", list5.GetDescriptionFromCode("IMPL1"));
			Assert("Any Line", list5.ContainsCode("IMPLX"));
			AssertEquals("Description", "IMPLX DESC", list5.GetDescriptionFromCode("IMPLX"));
			var list7 = ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, false, false);
			var list8 = ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, ZDateTime.Today, false, false);
			AssertEquals(false, object.ReferenceEquals(list7, list1));
			AssertEquals(false, object.ReferenceEquals(list7, list3));
			AssertEquals(false, object.ReferenceEquals(list7, list5));
			AssertEquals("IsCached", true, object.ReferenceEquals(list7, list8));
			Assert("Import Line 1 Only", !list7.ContainsCode("IMPL1"));
			AssertNull("Description", list7.GetDescriptionFromCode("IMPL1"));
			Assert("Any Line", list7.ContainsCode("IMPLX"));
			AssertEquals("Description", "IMPLX DESC", list7.GetDescriptionFromCode("IMPLX"));
		}

		public void TestGetAddInWithCusApprovedExporterAttribute()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry("ETA");
			testHelper.CreateAdditionalInformationCusCodeEntry("EUR");
			Factory.Save();
			var list1 = ZARefCusCodeListTypes.GetAddInWithCusApprovedExporterAttribute(Factory, ZDateTime.Today);
			var list2 = ZARefCusCodeListTypes.GetAddInWithCusApprovedExporterAttribute(Factory, ZDateTime.Today);
			AssertEquals("IsCached", true, object.ReferenceEquals(list1, list2));
			AssertEquals("EU Free Trade Agreement", list1.GetDescriptionFromCode("EUR"));
			AssertEquals("Test Count", 2, list1.Count);
		}
	}
}
