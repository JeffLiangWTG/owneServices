using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	public class RefTransportModesHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsTransportModeApplied_NoExceptionWhenTransportModeIsInvalid()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, "Manifest Validation Rule");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, "Person", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();
			var zzd = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "Person", Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, ZDateTime.Today);
			Assert(!zzd.IsTransportModeApplied("AES"));
		}

		public void TestGetList()
		{
			var list1 = RefTransportModesHelper.GetList(Factory);
			var list2 = RefTransportModesHelper.GetList(Factory);
			AssertEquals("UniveralRefTransportModeList should be cached", list1, list2);
		}

		public void TestExistsTransportModesForThatCodeTypeAndCountryOrGroupInZZDatabase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("TYPE1", "TYPE1 DESC");
			helper.CreateNewOrGetExistingCusCodeType("TYPE2", "TYPE2 DESC");
			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, "TYPE1", "AA", "AA THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TYPE1", "BB", "BB THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, "TYPE2", "CC", "CC THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTransportModeForCusCodeList(codeList1.PK, "ROA");
			Factory.Save();
			Assert("TYPE1/ER", RefTransportModesHelper.ExistsTransportModesForThatCodeTypeAndCountryOrGroupInZZDatabase(Factory, "TYPE1", Core.Constants.CountryCodes.Eritrea));
			Assert("TYPE1/CN", !RefTransportModesHelper.ExistsTransportModesForThatCodeTypeAndCountryOrGroupInZZDatabase(Factory, "TYPE1", Core.Constants.CountryCodes.China));
			Assert("TYPE2/ER", !RefTransportModesHelper.ExistsTransportModesForThatCodeTypeAndCountryOrGroupInZZDatabase(Factory, "TYPE2", Core.Constants.CountryCodes.Eritrea));
		}

		public void TestExistsSpecificTransportModeForThatCodeTypeAndCountryOrGroupInZZDatabase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("TYPE1", "TYPE1 DESC");
			helper.CreateNewOrGetExistingCusCodeType("TYPE2", "TYPE2 DESC");
			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, "TYPE1", "AA", "AA THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TYPE1", "BB", "BB THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, "TYPE2", "CC", "CC THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTransportModeForCusCodeList(codeList1.PK, "ROA");
			Factory.Save();
			Assert("TYPE1/ER", RefTransportModesHelper.ExistsSpecificTransportModeForThatCodeTypeAndCountryOrGroupInZZDatabase(Factory, "TYPE1", Core.Constants.CountryCodes.Eritrea, "ROA"));
			Assert("TYPE1/CN", !RefTransportModesHelper.ExistsSpecificTransportModeForThatCodeTypeAndCountryOrGroupInZZDatabase(Factory, "TYPE1", Core.Constants.CountryCodes.China, "ROA"));
			Assert("TYPE2/ER", !RefTransportModesHelper.ExistsSpecificTransportModeForThatCodeTypeAndCountryOrGroupInZZDatabase(Factory, "TYPE2", Core.Constants.CountryCodes.Eritrea, "ROA"));
		}

		public void TestExistsTransportModesForThatAttributeNameInZZDatabase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateNewOrGetExistingCusCodeType("TYPE1", "TYPE1 DESC");
			var codeType2 = helper.CreateNewOrGetExistingCusCodeType("TYPE2", "TYPE2 DESC");
			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType1.ZZK_CodeType, "AA", "AA THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, codeType1.ZZK_CodeType, "BB", "BB THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType2.ZZK_CodeType, "CC", "CC THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATTR1", "Desc.", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, codeType1.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATTR2", "Desc.", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, codeType1.ZZK_CodeType);
			var attribute1 = helper.CreateCusCodeListAttribute(codeList1.PK, "ATTR1", "Attribute 11");
			var attribute2 = helper.CreateCusCodeListAttribute(codeList1.PK, "ATTR2", "Attribute 12");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATTR1", "Desc.", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.China, codeType1.ZZK_CodeType);
			var attribute3 = helper.CreateCusCodeListAttribute(codeList2.PK, "ATTR1", "Attribute 21");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATTR1", "Desc.", codeType2.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, codeType2.ZZK_CodeType);
			var attribute4 = helper.CreateCusCodeListAttribute(codeList3.PK, "ATTR1", "Attribute 31");
			helper.CreateTransportModeForCusCodeAttribute(attribute1.PK, "ROA");
			Factory.Save();
			Assert("ATTR1/TYPE1/ER", RefTransportModesHelper.ExistsTransportModesForThatAttributeNameInZZDatabase(Factory, "TYPE1", Core.Constants.CountryCodes.Eritrea, "ATTR1"));
			Assert("ATTR1/TYPE1/CN", !RefTransportModesHelper.ExistsTransportModesForThatAttributeNameInZZDatabase(Factory, "TYPE1", Core.Constants.CountryCodes.China, "ATTR1"));
			Assert("ATTR1/TYPE2/ER", !RefTransportModesHelper.ExistsTransportModesForThatAttributeNameInZZDatabase(Factory, "TYPE2", Core.Constants.CountryCodes.Eritrea, "ATTR1"));
			Assert("ATTR2/TYPE1/ER", !RefTransportModesHelper.ExistsTransportModesForThatAttributeNameInZZDatabase(Factory, "TYPE1", Core.Constants.CountryCodes.Eritrea, "ATTR2"));
		}
	}
}
