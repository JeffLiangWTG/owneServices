using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWRefCusCodeListLoaderTest : TestCaseWithFactory
	{
		public void TestGetCustomsOffice()
		{
			var customsOffice = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice.ZZD_Code = "BB";
			customsOffice.ZZD_Description = "Taipei Office";
			customsOffice.ZZD_StartDate = ZDateTime.MinSmallDateTimeValue;
			customsOffice.ZZD_EndDate = ZDateTime.MaxSmallDateTimeValue;
			Factory.Save();
			var c1 = (ICodeDescription)TWRefCusCodeListLoader.GetCustomsOffice(Factory, "BB", ZDateTime.Today);
			AssertNotNull("Customs Office", c1);
			AssertEquals("BB", c1.Code);
			AssertEquals("Taipei Office", c1.Description);
		}

		public void TestGetLocationOfGoods()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var facilityCodeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var c1 = (ICodeDescription)TWRefCusCodeListLoader.GetLocationOfGoods(Factory, "XXXX0123", ZDateTime.Today);
			AssertNotNull("Facilities", c1);
			AssertEquals("XXXX0123", c1.Code);
			AssertEquals("XXXXXX", c1.Description);
		}

		public void TestGetPackingHouse()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, "Taiwan Packing House");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var c1 = (ICodeDescription)TWRefCusCodeListLoader.GetPackingHouse(Factory, "XXXX0123", ZDateTime.Today);
			AssertNotNull("Taiwan Packing House", c1);
			AssertEquals("XXXX0123", c1.Code);
			AssertEquals("XXXXXX", c1.Description);
		}

		public void TestGetTWCodeByType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var facilityCodeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var facility1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "XXXX0132", "AAAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var facility2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "XXXX0134", "BBBBBB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var c1 = (ICodeDescription)TWRefCusCodeListLoader.GetTWCodeByType(Factory, "XXXX0132", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today);
			var c2 = (ICodeDescription)TWRefCusCodeListLoader.GetTWCodeByType(Factory, "XXXX0134", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today);
			AssertNotNull("XXXX0132", c1);
			AssertNull("XXXX0134", c2);
		}

		public void TestGetICIPlacesByCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "CommodityInspection");
			var placeAA01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "AA01", "Place AA 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(placeAA01.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AA");
			var placeAA02 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "AA02", "Place AA 02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(placeAA02.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AA");
			var placeAB01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "AB01", "Place AB 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(placeAB01.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AB");
			var placeBA01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "BA01", "Place BA 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(placeBA01.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BA");
			Factory.Save();
			var aaList = TWRefCusCodeListLoader.GetICIPlacesByCustomsOffice(Factory, "AA", ZDateTime.Today);
			AssertEquals(2, aaList.Length);
			AssertNotNull("AA01", aaList.FirstOrDefault(refCode => refCode.ZZD_Code == "AA01"));
			AssertNotNull("AA02", aaList.FirstOrDefault(refCode => refCode.ZZD_Code == "AA02"));
			var aList = TWRefCusCodeListLoader.GetICIPlacesByCustomsOffice(Factory, "AC", ZDateTime.Today);
			AssertEquals(3, aList.Length);
			AssertNotNull("AA01", aList.FirstOrDefault(refCode => refCode.ZZD_Code == "AA01"));
			AssertNotNull("AA02", aList.FirstOrDefault(refCode => refCode.ZZD_Code == "AA02"));
			AssertNotNull("AB01", aList.FirstOrDefault(refCode => refCode.ZZD_Code == "AB01"));
			var emptyList = TWRefCusCodeListLoader.GetICIPlacesByCustomsOffice(Factory, "", ZDateTime.Today);
			AssertEquals(0, emptyList.Length);
		}

		public void TestGetSingleLocationOfGoodsCodeOrDefault()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var locationOfGoods640BG340 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "640BG340", "港龍航空有限公司台灣分公司保稅倉庫", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(locationOfGoods640BG340.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "B1");
			var locationOfGoods640B2140 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "640B2140", "高雄空廚股份有限公司保稅倉庫", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(locationOfGoods640B2140.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "B1");
			var locationOfGoods000AZZZZ = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "000AZZZZ", "未經海關登記空貨櫃儲存處", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(locationOfGoods000AZZZZ.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "A1");
			Factory.Save();
			AssertEquals("Should be empty", ZString.Empty, TWRefCusCodeListLoader.GetSingleLocationOfGoodsCodeOrDefault(Factory, "B1", ZDateTime.Today));
			AssertEquals("Should be 000AZZZZ", "000AZZZZ", TWRefCusCodeListLoader.GetSingleLocationOfGoodsCodeOrDefault(Factory, "A1", ZDateTime.Today));
		}

		public void TestGetExportDeclarationType()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForDeclarationType();
			var exportDeclarationType = TWRefCusCodeListLoader.GetExportDeclarationType(Factory, ZDateTime.Today);
			AssertNotNull("ExportDeclarationType", exportDeclarationType);
			AssertEquals(10, exportDeclarationType.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "B1", "B2", "B8", "B9", "D1", "D5", "F4", "F5", "G3", "G5" }, exportDeclarationType.Select(c => c.ZZD_Code));
		}

		public void TestGetImportDeclarationType()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForDeclarationType();
			var importDeclarationType = TWRefCusCodeListLoader.GetImportDeclarationType(Factory, ZDateTime.Today);
			AssertNotNull("importDeclarationType", importDeclarationType);
			AssertEquals(11, importDeclarationType.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "B6", "D2", "D7", "D8", "F1", "F2", "F3", "G1", "G2", "G7", "L1" }, importDeclarationType.Select(c => c.ZZD_Code));
		}

		public void TestGetProcessingUnit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "TWReceivingUnit");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginIssuingUnit, "CertificateOfOriginIssuingUnit");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginIssuingUnit, "OOOO9876", "OOOOOO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var processingUnit1 = (ICodeDescription)TWRefCusCodeListLoader.GetProcessingUnit(Factory, false, "XXXX0123", ZDateTime.Today);
			CombineAssertions(() =>
			{
				AssertEquals("XXXX0123", processingUnit1.Code);
				AssertEquals("XXXXXX", processingUnit1.Description);
			});
			var processingUnit2 = (ICodeDescription)TWRefCusCodeListLoader.GetProcessingUnit(Factory, true, "OOOO9876", ZDateTime.Today);
			CombineAssertions(() =>
			{
				AssertEquals("OOOO9876", processingUnit2.Code);
				AssertEquals("OOOOOO", processingUnit2.Description);
			});
		}

		public void TestLoadTaiwanRefCusCodeListDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECFALoadingPort, "ECFA Loading Port", Core.Constants.CountryCodes.Taiwan);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECFALoadingPort, "TWTPE", "台北", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			AssertEquals("台北", TWRefCusCodeListLoader.LoadTaiwanRefCusCodeListDescription(Factory, "TWTPE", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECFALoadingPort));
		}
	}
}
