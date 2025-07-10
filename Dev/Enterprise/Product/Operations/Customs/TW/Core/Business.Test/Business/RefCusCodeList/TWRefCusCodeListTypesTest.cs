using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWRefCusCodeListTypesTest : TestCaseWithFactory
	{
		public void TestGetMethodOfCalculationList()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateOrGetLanguage("ZHT", "繁體中文");
			helper.CreateCusCodeType("TWPUM", "Taiwan Packing Units of Measurement");
			var cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWPUM", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(cusCodeList, "ZHT", "安培");
			cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWPUM", "YDS", "Yards", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(cusCodeList, "ZHT", "碼");
			cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWPUM", "DOZ", "Dozen", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(cusCodeList, "ZHT", "一打");
			cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWPUM", "PCS", "Pieces", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(cusCodeList, "ZHT", "件");
			newFactory.Save();
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddRange(TWRefCusCodeListTypes.GetCustomsPackUnitsList(Factory));
			expectedList.AddPair("%");
			var actualList = TWRefCusCodeListTypes.GetMethodOfCalculationList(Factory);
			CombineAssertions(() =>
			{
				AssertEquals(expectedList, actualList);
				AssertEquals("%, AMP, DOZ, PCS, YDS", actualList.CodesAsString);
			});
		}

		public void TestGetCategoryCodesOfCAAAircraftParts()
		{
			var list1 = TWRefCusCodeListTypes.GetCategoryCodesOfCAAAircraftParts(Factory);
			var list2 = TWRefCusCodeListTypes.GetCategoryCodesOfCAAAircraftParts(Factory);
			AssertEquals("IsCached", list1, list2);
			var collection = new TWRefCusCodeListCollection(Factory, Codes.TaiwanAircraftPartCAACodeCategory, ZDateTime.Today);
			collection.Load();
			var count = collection.Count;
			AssertEquals("PreCondition", count, list1.Count);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.TaiwanAircraftPartCAACodeCategory, "TaiwanAircraftPartCAACodeCategory");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACodeCategory, "6", "第六類：航空器化學及油漆材料", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			list1 = TWRefCusCodeListTypes.GetCategoryCodesOfCAAAircraftParts(Factory);
			AssertEquals("IsCached", list1, list2);

			list1 = TWRefCusCodeListTypes.GetCategoryCodesOfCAAAircraftParts(new BusinessObjectFactory());
			AssertNotEquals("CacheRefreshed", list1, list2);
			AssertEquals("第六類：航空器化學及油漆材料", list1.GetDescriptionFromCode("6"));
		}

		public void TestGetCAAAircraftPartsCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.TaiwanAircraftPartCAACode, "TaiwanAircraftPartCAACode");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACode, "1.1", "Air Seal Guard", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACode, "1.10", "Decal-Marking", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACode, "1.13", "Fabric-Glass", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACode, "1.2", "Barrier Material", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACode, "1.20", "Paper-Laminated", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACode, "1.21", "Rod (Tube)-Lamicold, Phenolic resin linen base", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACode, "3.1", "Aircraft Communication and Navigation use Radio Equipment : such as Transmitters, Receivers, Transceiver and Accessories.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACode, "3.3a", "a Amplifier", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACode, "3.2", "Aircraft Antenna Equipment & Parts", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACode, "3.3b", "b Battery-Dry cell or Storage(with Battery Terminal)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var list1 = TWRefCusCodeListTypes.GetCAAAircraftPartsCodes(Factory, "1");
			var list2 = TWRefCusCodeListTypes.GetCAAAircraftPartsCodes(Factory, "2");
			var list3 = TWRefCusCodeListTypes.GetCAAAircraftPartsCodes(Factory, "3");
			CombineAssertions(() =>
			{
				AssertEquals(6, list1.Count);
				AssertEquals("1", list1[0].Code);
				AssertEquals("Air Seal Guard", list1[0].Description);
				AssertEquals("2", list1[1].Code);
				AssertEquals("Barrier Material", list1[1].Description);
				AssertEquals("10", list1[2].Code);
				AssertEquals("Decal-Marking", list1[2].Description);
				AssertEquals("13", list1[3].Code);
				AssertEquals("Fabric-Glass", list1[3].Description);
				AssertEquals("20", list1[4].Code);
				AssertEquals("Paper-Laminated", list1[4].Description);
				AssertEquals("21", list1[5].Code);
				AssertEquals("Rod (Tube)-Lamicold, Phenolic resin linen base", list1[5].Description);

				AssertEquals(0, list2.Count);

				AssertEquals(4, list3.Count);
				AssertEquals("1", list3[0].Code);
				AssertEquals("Aircraft Communication and Navigation use Radio Equipment : such as Transmitters, Receivers, Transceiver and Accessories.", list3[0].Description);
				AssertEquals("2", list3[1].Code);
				AssertEquals("Aircraft Antenna Equipment & Parts", list3[1].Description);
				AssertEquals("3a", list3[2].Code);
				AssertEquals("a Amplifier", list3[2].Description);
				AssertEquals("3b", list3[3].Code);
				AssertEquals("b Battery-Dry cell or Storage(with Battery Terminal)", list3[3].Description);
			});

			var list4 = TWRefCusCodeListTypes.GetCAAAircraftPartsCodes(Factory, "1");
			AssertEquals("IsCached", list1, list4);
		}

		public void TestGetCustomsOfficeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var list1 = TWRefCusCodeListTypes.GetCustomsOfficeList(Factory, declaration);
			var list2 = TWRefCusCodeListTypes.GetCustomsOfficeList(Factory, declaration);
			AssertEquals("IsCached", list1, list2);
			var listType = Codes.CustomsOffice;
			var collection = new TWRefCusCodeListCollection(Factory, listType, ZDateTime.Today);
			collection.Load();
			var count = collection.Count;
			AssertEquals("PreCondition", count, list1.Count);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(listType, "CustomsOffice");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, listType, "CC", "BABA THE BUILDER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList.PK, TransportTypeList.Codes.Sea);
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, listType, "DD", "DD Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList2.PK, TransportTypeList.Codes.Air);
			Factory.Save();
			list1 = TWRefCusCodeListTypes.GetCustomsOfficeList(Factory, declaration);
			AssertEquals("IsCached", list1, list2);
			list1 = TWRefCusCodeListTypes.GetCustomsOfficeList(new BusinessObjectFactory(), declaration);
			AssertNotEquals("CacheRefreshed", list1, list2);
			AssertEquals(1, list1.Count);
			AssertEquals("BABA THE BUILDER", list1.GetDescriptionFromCode("CC"));
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			list1 = TWRefCusCodeListTypes.GetCustomsOfficeList(Factory, declaration);
			AssertEquals(1, list1.Count);
			AssertEquals("DD Desc", list1.GetDescriptionFromCode("DD"));
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			list1 = TWRefCusCodeListTypes.GetCustomsOfficeList(Factory, cusInBondHeader);
			AssertEquals(1, list1.Count);
			AssertEquals("DD Desc", list1.GetDescriptionFromCode("DD"));
			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			list1 = TWRefCusCodeListTypes.GetCustomsOfficeList(new BusinessObjectFactory(), cusInBondHeader);
			AssertEquals(1, list1.Count);
			AssertEquals("BABA THE BUILDER", list1.GetDescriptionFromCode("CC"));
		}

		public void TestGetCustomsOfficeListNoDeclaration()
		{
			var list1 = TWRefCusCodeListTypes.GetCustomsOfficeList(Factory);
			var list2 = TWRefCusCodeListTypes.GetCustomsOfficeList(Factory);
			AssertEquals("IsCached", list1, list2);
			var collection = new TWRefCusCodeListCollection(Factory, Codes.CustomsOffice, ZDateTime.Today);
			collection.Load();
			var count = collection.Count;
			AssertEquals("PreCondition", count, list1.Count);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.CustomsOffice, "CustomsOffice");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.CustomsOffice, "CC", "BABA THE BUILDER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList.PK, TransportTypeList.Codes.Sea);
			Factory.Save();
			list1 = TWRefCusCodeListTypes.GetCustomsOfficeList(Factory);
			AssertEquals("IsCached", list1, list2);
			list1 = TWRefCusCodeListTypes.GetCustomsOfficeList(new BusinessObjectFactory());
			AssertNotEquals("CacheRefreshed", list1, list2);
			AssertEquals("BABA THE BUILDER", list1.GetDescriptionFromCode("CC"));
		}

		public void TestGetGoodsLocationCollection()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var list1 = TWRefCusCodeListTypes.GetGoodsLocationCollection(Factory, cusEntryInstruction.CEI_CustomsOffice);
			var list2 = TWRefCusCodeListTypes.GetGoodsLocationCollection(Factory, cusEntryInstruction.CEI_CustomsOffice);
			AssertEquals("IsCached", list1, list2);
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var facilityCodeType = helper.CreateNewOrGetExistingCusCodeType(Codes.Facilities, "Facilities");
			var facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.Facilities, "ANP0060D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", Codes.Facilities, Core.Constants.CountryCodes.Taiwan);
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BA");
			newFactory.Save();
			cusEntryInstruction.CEI_CustomsOffice = "##";
			list2 = TWRefCusCodeListTypes.GetGoodsLocationCollection(newFactory, cusEntryInstruction.CEI_CustomsOffice);
			var testList = (ZZRefCusCodeListCombinedCollection)list2;
			testList.Load();
			Assert(!testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.Attributes.HasAttribute(RefCusCodeListAttributeTypes.Codes.CustomsOffice, cusEntryInstruction.CEI_CustomsOffice)));
			cusEntryInstruction.CEI_CustomsOffice = "BA";
			list2 = TWRefCusCodeListTypes.GetGoodsLocationCollection(newFactory, cusEntryInstruction.CEI_CustomsOffice);
			testList = (ZZRefCusCodeListCombinedCollection)list2;
			testList.Load();
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.Attributes.HasAttribute(RefCusCodeListAttributeTypes.Codes.CustomsOffice, cusEntryInstruction.CEI_CustomsOffice)));
		}

		public void TestGetPackingHouseList()
		{
			CombineAssertions(() =>
			{
				var coll = (ZZRefCusCodeListCombinedCollection)TWRefCusCodeListTypes.GetPackingHouseList(Factory, "Country", "AU", "Tariff", "07061000005");
				AssertEquals("AU", coll.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().First(x => x.FilterName == "Country").Value);
				AssertEquals("07061000005", coll.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().First(x => x.FilterName == "Tariff").Value);
			});
		}

		public void TestGetPackingHouseTariffs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, "Taiwan Packing House");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.Tariff, "Tariff", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, Core.Constants.CountryCodes.Taiwan);
			var code1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			code1.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Tariff, "07061000005");

			var code2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, "AU0003", "BW GRIGGS & SONS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			code2.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Tariff, "08092100008");

			Factory.Save();
			var coll = TWRefCusCodeListTypes.GetPackingHouseTariffs(Factory);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "07061000005", "08092100008" }, coll);
		}

		public void TestGetControllingMessageMessageTypeRefCusCodeListCombined()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.TWControllingMessageMessageType, "Taiwan Controlling Message Message Type");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ControlAgency, "Taiwan Controlling Agency", Codes.TWControllingMessageMessageType, Core.Constants.CountryCodes.Taiwan);
			var codeListX101 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWControllingMessageMessageType, "X101", "產地證明申辦訊息", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeListX101.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "FT");
			var codeListNX301 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWControllingMessageMessageType, "NX301", "報驗申辦訊息", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeListNX301.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "CI");
			codeListNX301.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "20");
			codeListNX301.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "30");
			newFactory.Save();
			AssertNotNull(TWRefCusCodeListTypes.GetControllingMessageMessageTypeRefCusCodeListCombined(newFactory, "X101"));
			AssertNotNull(TWRefCusCodeListTypes.GetControllingMessageMessageTypeRefCusCodeListCombined(newFactory, "NX301"));
			AssertNull(TWRefCusCodeListTypes.GetControllingMessageMessageTypeRefCusCodeListCombined(newFactory, "XX"));
		}

		public void TestGetControllingAgencyCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.TWControllingAgency, "TWControllingAgency");
			helper.CreateNewOrGetExistingCusCodeType(Codes.TWControllingMessageMessageType, "Taiwan Controlling Message Message Type");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ControlAgency, "Taiwan Controlling Agency", Codes.TWControllingMessageMessageType, Core.Constants.CountryCodes.Taiwan);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWControllingAgency, "CI", "CI DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWControllingAgency, "20", "20 DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWControllingAgency, "FT", "FT DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListX101 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWControllingMessageMessageType, "X101", "產地證明申辦訊息", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeListX101.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "FT");
			var codeListNX301 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWControllingMessageMessageType, "NX301", "報驗申辦訊息", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeListNX301.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "CI");
			codeListNX301.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "20");
			codeListNX301.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "30");
			newFactory.Save();
			var testList = TWRefCusCodeListTypes.GetControllingAgencyCollection(newFactory, "X101");
			AssertEquals(1, testList.Count());
			Assert(testList.Any(code => code.ZZD_Code == "FT" && code.ZZD_Description == "FT DES."));
			testList = TWRefCusCodeListTypes.GetControllingAgencyCollection(newFactory, "NX301");
			AssertEquals(2, testList.Count());
			Assert(testList.Any(code => code.ZZD_Code == "CI" && code.ZZD_Description == "CI DES."));
			Assert(testList.Any(code => code.ZZD_Code == "20" && code.ZZD_Description == "20 DES."));
		}

		public void TestGetFacilityCollection()
		{
			var list1 = TWRefCusCodeListTypes.GetGoodsLocationCollection(Factory, null);
			var list2 = TWRefCusCodeListTypes.GetGoodsLocationCollection(Factory, null);
			AssertEquals("IsCached", list1, list2);
			var collection = new TWRefCusCodeListCollection(Factory, Codes.Facilities, ZDate.Today);
			collection.Load();
			var count = collection.Count;
			AssertEquals("PreCondition", count, list1.Count);
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.Facilities, "Facilities");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.Facilities, "AG888", "BABA THE XXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();
			list1 = TWRefCusCodeListTypes.GetGoodsLocationCollection(newFactory, null);
			((ZZRefCusCodeListCombinedCollection)list1).Load();
			AssertEquals(1, list1.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "AG888" }, list1.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestGetContactOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.TWReceivingUnit, "TWReceivingUnit");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWReceivingUnit, "PROU", "TW Receiving Unit", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWReceivingUnit, "PROU1", "Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Codes.Facilities, "Facilities");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.Facilities, "AG888", "BABA THE XXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var list = TWRefCusCodeListTypes.GetContactOfficeList(Factory);
			AssertEquals(2, list.Count);
		}

		public void TestGetControllingAgencyList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.TWControllingAgency, "TWControllingAgency");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWControllingAgency, "96", "交通部航政司", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var list1 = TWRefCusCodeListTypes.GetControllingAgencyList(Factory, true);
			var list2 = TWRefCusCodeListTypes.GetControllingAgencyList(Factory, true);
			Assert("Should have cached.", list1.Equals(list2));
			AssertEquals(11, list1.Count);
			AssertEquals("CI, 20, 2Q, VP, DN, CD, IF, DH, FT, AX, AG", list1.CodesAsString);
			var list3 = TWRefCusCodeListTypes.GetControllingAgencyList(Factory);
			var list4 = TWRefCusCodeListTypes.GetControllingAgencyList(Factory);
			Assert("Should have cached.", list3.Equals(list4));
			AssertEquals(1, list3.Count);
			AssertEquals("96", list3.CodesAsString);
		}

		public void TestGetProcessingUnitList()
		{
			var today = ZDateTime.Today;
			var controllingMessageHeader = Factory.New<CusTWControllingMessageHeader>();
			var list1 = TWRefCusCodeListTypes.GetProcessingUnitList(Factory, controllingMessageHeader.TW1_ControllingAgency, today);
			var list2 = TWRefCusCodeListTypes.GetProcessingUnitList(Factory, controllingMessageHeader.TW1_ControllingAgency, today);
			AssertSame("IsCached", list1, list2);

			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.TWReceivingUnit, "Receiving Unit");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ControlAgency, "Desc.", Codes.TWReceivingUnit, Core.Constants.CountryCodes.Taiwan);

			var prou = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWReceivingUnit, "1TT", "花蓮分局", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			prou.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "BA");

			prou = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TWReceivingUnit, "2TP", "基隆分局臺北港辦事處", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			prou.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "BB");
			newFactory.Save();

			controllingMessageHeader.TW1_ControllingAgency = "##";
			var list = TWRefCusCodeListTypes.GetProcessingUnitList(newFactory, controllingMessageHeader.TW1_ControllingAgency, today);
			AssertEquals(0, list.Count);

			controllingMessageHeader.TW1_ControllingAgency = "BA";
			list = TWRefCusCodeListTypes.GetProcessingUnitList(newFactory, controllingMessageHeader.TW1_ControllingAgency, today);
			AssertEquals("1TT", list.CodesAsString);

			controllingMessageHeader.TW1_ControllingAgency = "BB";
			list = TWRefCusCodeListTypes.GetProcessingUnitList(newFactory, controllingMessageHeader.TW1_ControllingAgency, today);
			AssertEquals("2TP", list.CodesAsString);
		}

		public void TestGetProcessingUnitListWhenMessageTypeIsNX101()
		{
			var today = ZDateTime.Today;
			var controllingMessageHeader = Factory.New<CusTWControllingMessageHeader>();
			var list1 = TWRefCusCodeListTypes.GetProcessingUnitList(Factory, controllingMessageHeader.TW1_CertificateType, today, true);
			var list2 = TWRefCusCodeListTypes.GetProcessingUnitList(Factory, controllingMessageHeader.TW1_CertificateType, today, true);
			AssertSame("IsCached", list1, list2);

			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.TaiwanCertificateOfOriginIssuingUnit, "Certificate of Origin Issuing Unit");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Type, "Desc.", Codes.TaiwanCertificateOfOriginIssuingUnit, Core.Constants.CountryCodes.Taiwan);

			var prou = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanCertificateOfOriginIssuingUnit, "AA", "台灣區電機電子工業同業公會", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			prou.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Type, "01");

			prou = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanCertificateOfOriginIssuingUnit, "AB", "臺灣機械工業同業公會", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			prou.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Type, "02");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.TaiwanCertificateOfOriginIssuingUnit, "BA", "台灣花卉輸出業同業公會", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			controllingMessageHeader.TW1_CertificateType = "##";
			var list = TWRefCusCodeListTypes.GetProcessingUnitList(newFactory, controllingMessageHeader.TW1_CertificateType, today, true);
			AssertEquals(0, list.Count);

			controllingMessageHeader.TW1_CertificateType = "01";
			list = TWRefCusCodeListTypes.GetProcessingUnitList(newFactory, controllingMessageHeader.TW1_CertificateType, today, true);
			AssertEquals("AA", list.CodesAsString);

			controllingMessageHeader.TW1_CertificateType = "02";
			list = TWRefCusCodeListTypes.GetProcessingUnitList(newFactory, controllingMessageHeader.TW1_CertificateType, today, true);
			AssertEquals("AB", list.CodesAsString);
		}

		public void TestGetCustomsPackUnitsList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ZHT", "繁體中文");
			helper.CreateCusCodeType("TWPUM", "Taiwan Packing Units of Measurement");
			var cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWPUM", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(cusCodeList, "ZHT", "安培");
			cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWPUM", "YDS", "Yards", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(cusCodeList, "ZHT", "碼");
			cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWPUM", "DOZ", "Dozen", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(cusCodeList, "ZHT", "一打");
			cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWPUM", "PCS", "Pieces", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(cusCodeList, "ZHT", "件");
			Factory.Save();
			var list = TWRefCusCodeListTypes.GetCustomsPackUnitsList(Factory);
			CombineAssertions(() =>
			{
				AssertEquals(4, list.Count);
				AssertEquals("AMP", list[0].Code);
				AssertEquals("Ampere", list[0].Description);
			});

			var newList = TWRefCusCodeListTypes.GetCustomsPackUnitsList(Factory);
			AssertSame("Should have cached", list, newList);
			MasterFiles.Business.GlbStaff.CurrentUser[ZArchitecture.Schema.GlbStaffSchema.GS_WorkingLanguage] = Core.SharedConstants.Languages.ChineseTraditional;
			list = TWRefCusCodeListTypes.GetCustomsPackUnitsList(Factory);
			CombineAssertions(() =>
			{
				AssertEquals(4, list.Count);
				AssertEquals("AMP", list[0].Code);
				AssertEquals("安培", list[0].Description);
			});

			list = TWRefCusCodeListTypes.GetCustomsPackUnitsList(Factory, Core.SharedConstants.Languages.English);
			CombineAssertions(() =>
			{
				AssertEquals(4, list.Count);
				AssertEquals("AMP", list[0].Code);
				AssertEquals("Ampere", list[0].Description);
			});
		}

		public void TestGetCommercialPackUnitsList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWCIU", "Taiwan Packing Units of Measurement");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWCIU", "CTN", "Carton", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWCIU", "YDS", "Yards", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWCIU", "DOZ", "Dozen", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWCIU", "PCS", "Pieces", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var list = TWRefCusCodeListTypes.GetCommercialPackUnitsList(Factory);
			CombineAssertions(() =>
			{
				AssertEquals(4, list.Count);
				AssertEquals("CTN, DOZ, PCS, YDS", list.CodesAsString);
			});
		}

		public void TestControllingMessageTypeList()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForControllingMessageType();
			var list = TWRefCusCodeListTypes.GetControllingMessageTypeList(Factory);
			CombineAssertions(() =>
			{
				AssertEquals(7, list.Count);
				AssertEquals("NX101, NX301, NX301_DN, NX401, NX601, NX603, X101", list.CodesAsString);
			});
		}

		public void TestGetCustomsOfficeCollection()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusGoodsLocation();
			var lookupsList = TWRefCusCodeListTypes.GetCustomsOfficeCollection(Factory) as ZZRefCusCodeListCombinedCollection;
			lookupsList.Load();
			AssertEquals(2, lookupsList.Count);
			Assert(lookupsList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "CC"));
			Assert(lookupsList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "DD"));
		}

		public void TestGetBankAccountNoByCustomsOfficeCode()
		{
			var listType = Codes.CustomsOffice;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(listType, "CustomsOffice");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, listType, "CA", "臺北關業務一組", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, listType, "AA", "基隆關", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.BankAccountNo, "Bank Account Number", listType, Core.Constants.CountryCodes.Taiwan);
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.BankAccountNo, "00000000000150");
			codeList2.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.BankAccountNo, "00000000000110");
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("CA Bank Account Number", "00000000000150", TWRefCusCodeListTypes.GetBankAccountNoByCustomsOfficeCode(Factory, "CA"));
				AssertEquals("AA Bank Account Number", "00000000000110", TWRefCusCodeListTypes.GetBankAccountNoByCustomsOfficeCode(Factory, "AA"));
				AssertEquals("FC Bank Account Number", ZString.Empty, TWRefCusCodeListTypes.GetBankAccountNoByCustomsOfficeCode(Factory, "FC"));
			});
		}
	}
}
