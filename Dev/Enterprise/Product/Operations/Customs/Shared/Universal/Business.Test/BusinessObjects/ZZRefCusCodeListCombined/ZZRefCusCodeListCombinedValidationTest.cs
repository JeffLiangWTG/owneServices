using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	sealed class ZZRefCusCodeListCombinedValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll_CheckMultipleIdenticalAttributeValueRecords()
		{
			const string errorMessage = "For Attributes which have the same name and value it's mandatory to provide a Start Date and End Date.";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType("TYPE1", "DESC");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "DESC ER");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATTR1", "DESC", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, allowDuplicates: true, isDateRangeUsed: true);
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType.ZZK_CodeType, "CODE1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeListAttribute(codeList.PK, "ATTR1", ZString.Empty, false);
			Factory.Save();

			var codeListCombined = Factory.New<ZZRefCusCodeListCombined>();
			codeListCombined.ZZD_CodeType = codeType.ZZK_CodeType;
			codeListCombined.ZZD_Code = "CODE1";
			codeListCombined.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			var matchingAttributeCombined = codeListCombined.Attributes.AddNew();
			matchingAttributeCombined.ZZE_ZXE_NKName = "ATTR1";
			matchingAttributeCombined.ZZE_Value = YesNoList.Codes.Yes;
			var matchingAttributeCombined2 = codeListCombined.Attributes.AddNew();
			matchingAttributeCombined2.ZZE_ZXE_NKName = "ATTR1";
			matchingAttributeCombined2.ZZE_Value = YesNoList.Codes.Yes;

			CombineAssertions(() =>
			{
				codeListCombined.Validation.ValidateAll();
				AssertHasRowError("matchingAttributeCombined: no dates", matchingAttributeCombined, errorMessage);
				AssertHasRowError("matchingAttributeCombined2: no dates", matchingAttributeCombined2, errorMessage);

				matchingAttributeCombined.ZZE_StartDate = ZDateTime.Today;
				matchingAttributeCombined.ZZE_EndDate = ZDateTime.Today.AddDays(1);
				matchingAttributeCombined2.ZZE_StartDate = ZDateTime.Today.AddDays(2);
				matchingAttributeCombined2.ZZE_EndDate = ZDateTime.Today.AddDays(3);
				codeListCombined.Validation.ValidateAll();
				AssertNoRowError("matchingAttributeCombined: has both Dates", matchingAttributeCombined, errorMessage);
				AssertNoRowError("matchingAttributeCombined2: has both Dates", matchingAttributeCombined2, errorMessage);
			});
		}

		public void TestCheckZZD_CodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC DESC", Core.Constants.CountryCodes.Ethiopia);
			var codeType2 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "EPC DESC", Core.Constants.CountryCodes.Ethiopia);
			codeType2.ZZK_IsReadonly = false;
			var codeType3 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT DESC", Core.Constants.CountryCodes.Ethiopia);
			var codeType4 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TRNAT DESC", Core.Constants.CountryCodes.Ethiopia);
			codeType4.ZZK_IsReadonly = true;
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ethiopia, "ET DESC");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, codeType2.ZZK_CodeType, "AAA", "AAA DESC", new ZDateTime("2019-07-22"), new ZDateTime("2020-07-22"));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, codeType3.ZZK_CodeType, "BBB", "BBB DESC", new ZDateTime("2019-07-22"), new ZDateTime("2020-07-22"));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, codeType4.ZZK_CodeType, "CCC", "CCC DESC", new ZDateTime("2019-07-22"), new ZDateTime("2020-07-22"));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("123", "123 DESC", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.Ethiopia, codeType1.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ABC", "ABC DESC", codeType2.ZZK_CodeType, Core.Constants.CountryCodes.Ethiopia, codeType2.ZZK_CodeType, isMandatory: true);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DEF", "DEF DESC", codeType2.ZZK_CodeType, Core.Constants.CountryCodes.Ethiopia, codeType3.ZZK_CodeType, isMandatory: true);
			Factory.Save();
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CodeType = C.RefCusCodeListTypes.Codes.CustomsOffice;
			AssertNoErrorContaining(cusCodeList.ZZD_CodeTypeInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_CodeType = ZString.Empty;
			AssertHasErrorContaining(cusCodeList.ZZD_CodeTypeInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_IsSystem = true;
			AssertNoErrorContaining(cusCodeList.ZZD_CodeTypeInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_IsSystem = false;
			AssertHasErrorContaining(cusCodeList.ZZD_CodeTypeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(cusCodeList.ZZD_CodeTypeInfo, ListValidation.InvalidCodeError);
			cusCodeList.ZZD_CodeType = "!@";
			AssertHasErrorContaining(cusCodeList.ZZD_CodeTypeInfo, ListValidation.InvalidCodeError);
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Ethiopia;
			cusCodeList.ZZD_CodeType = codeType2.ZZK_CodeType;
			AssertHasErrorContaining(cusCodeList.ZZD_CodeTypeInfo, "Mandatory attribute name 'ABC DESC', 'DEF DESC' is required.");
			cusCodeList.ZZD_CodeType = codeType4.ZZK_CodeType;
			cusCodeList.ZZD_IsSystem = false;
			AssertHasErrorContaining(cusCodeList.ZZD_CodeTypeInfo, "This List Type 'TRNAT' is read only.");
			cusCodeList.Attributes.AddNew("abc", "11");
			cusCodeList.Attributes.AddNew("DEF", "22");
			cusCodeList.ZZD_IsSystem = true;
			cusCodeList.Validation.ValidateZZD_CodeType();
			AssertNoErrors(cusCodeList.ZZD_CodeTypeInfo);
		}

		public void TestCheckZZD_CountryOrGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType("Z#@", "DESC");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea);
			Factory.Save();
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.NewZealand;
			AssertNoErrorContaining(cusCodeList.ZZD_CountryOrGroupingInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_CountryOrGrouping = ZString.Empty;
			AssertHasErrorContaining(cusCodeList.ZZD_CountryOrGroupingInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_IsSystem = true;
			AssertNoErrorContaining(cusCodeList.ZZD_CountryOrGroupingInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_IsSystem = false;
			AssertHasErrorContaining(cusCodeList.ZZD_CountryOrGroupingInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(cusCodeList.ZZD_CountryOrGroupingInfo, ListValidation.InvalidCodeError);
			cusCodeList.ZZD_CountryOrGrouping = "!@";
			AssertHasErrorContaining(cusCodeList.ZZD_CountryOrGroupingInfo, ListValidation.InvalidCodeError);
			var codeList = Factory.New<RefCusCodeList>();
			codeList.ZZD_ZZK_NKCodeType = C.RefCusCodeListTypes.Codes.CustomsOffice;
			codeList.ZZD_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			Assert(!codeList.ZZD_ZZZ_NKDataGroupingInfo.Notifications.Any());
		}

		public void TestCheckZZD_Code()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			var info = cusCodeList.ZZD_CodeInfo;
			cusCodeList.ZZD_Code = "BOB";
			AssertNoErrorContaining(info, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_Code = ZString.Empty;
			AssertHasErrorContaining(info, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_IsSystem = true;
			AssertNoErrorContaining(info, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_IsSystem = false;
			AssertHasErrorContaining(info, MandatoryValidation.MustBeEntered);
			cusCodeList.Delete();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusCodeType = helper.CreateNewOrGetExistingCusCodeType("SHAPE", "Shapes...", Core.Constants.CountryCodes.Brazil, 10);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil, "Brazil");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Brazil, "SHAPE", "SQUARE", "Square Description...", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var odysseyZZRefCusCodeList = Factory.New<Internal.ZZRefCusCodeList>();
			odysseyZZRefCusCodeList.ZZD_CodeType = "COLOR";
			odysseyZZRefCusCodeList.ZZD_Code = "RED";
			odysseyZZRefCusCodeList.ZZD_Description = "Red Description...";
			odysseyZZRefCusCodeList.ZZD_StartDate = ZDateTime.MinSmallDateTimeValue.Date;
			odysseyZZRefCusCodeList.ZZD_EndDate = ZDateTime.MaxSmallDateTimeValue.Date;
			odysseyZZRefCusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			info = cusCodeList.ZZD_CodeInfo;
			cusCodeList.ZZD_CodeType = "SHAPE";
			cusCodeList.ZZD_Code = "SQUARE";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Brazil;
			AssertNoErrorContaining("Shouldn't conflict with ZZ ref database's RefCusCodeList, only the Odyssey database's ZZRefCusCodeList", info, "already exists.");
			cusCodeList.ZZD_CodeType = "COLOR";
			cusCodeList.ZZD_Code = "RED";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Australia;
			AssertHasErrorContaining(info, "already exists.");
			cusCodeList.ZZD_CodeType = "COLOR";
			cusCodeList.ZZD_Code = "BLUE";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Australia;
			AssertNoErrorContaining("Shouldn't conflict, as they have different codes.", info, "already exists.");
			cusCodeList.ZZD_CodeType = "OTHER";
			cusCodeList.ZZD_Code = "RED";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Australia;
			AssertNoErrorContaining("Shouldn't conflict, as they have different codeTypes.", info, "already exists.");
			cusCodeList.ZZD_CodeType = "COLOR";
			cusCodeList.ZZD_Code = "RED";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Singapore;
			AssertNoErrorContaining("Shouldn't conflict, as they have different countries.", info, "already exists.");
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var loadedCusCodeList = newFactory.Load<ZZRefCusCodeListCombined>(cusCodeList.PK);
			loadedCusCodeList.Validation.ValidateZZD_Code();
			AssertNoErrorContaining("Shouldn't conflict with itself.", loadedCusCodeList.ZZD_CodeInfo, "already exists.");
			cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			info = cusCodeList.ZZD_CodeInfo;
			cusCodeList.ZZD_CodeType = "SHAPE";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Brazil;
			cusCodeList.ZZD_Code = "SQUARE SQUARE";
			AssertHasErrorContaining(info, "exceeds the max length 10");
			cusCodeList.ZZD_Code = "SQUARE";
			AssertNoErrorContaining(info, "exceeds the max length 10");
		}

		public void TestCheckZZD_Description()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_Description = "BOB";
			AssertNoErrorContaining(cusCodeList.ZZD_DescriptionInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_Description = ZString.Empty;
			AssertHasErrorContaining(cusCodeList.ZZD_DescriptionInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_IsSystem = true;
			AssertNoErrorContaining(cusCodeList.ZZD_DescriptionInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_IsSystem = false;
			AssertHasErrorContaining(cusCodeList.ZZD_DescriptionInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckZZD_StartDate()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_StartDate = ZDateTime.BrettsBirthday;
			AssertNoErrorContaining(cusCodeList.ZZD_StartDateInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_StartDate = ZDateTime.Empty;
			AssertHasErrorContaining(cusCodeList.ZZD_StartDateInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_IsSystem = true;
			AssertNoErrorContaining(cusCodeList.ZZD_StartDateInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_IsSystem = false;
			AssertHasErrorContaining(cusCodeList.ZZD_StartDateInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_EndDate = new ZDateTime(2016, 4, 1);
			cusCodeList.ZZD_StartDate = new ZDateTime(2016, 4, 2);
			AssertNoErrorContaining(cusCodeList.ZZD_StartDateInfo, MandatoryValidation.MustBeEntered);
			AssertHasError(cusCodeList.ZZD_StartDateInfo, ZZRefCusCodeListCombinedValidation.StartDateCannotBeAfterEndDate);
			cusCodeList.ZZD_EndDate = new ZDateTime(2016, 5, 1);
			AssertNoError(cusCodeList.ZZD_StartDateInfo, ZZRefCusCodeListCombinedValidation.StartDateCannotBeAfterEndDate);
		}

		public void TestCheckZZD_EndDate()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_EndDate = ZDateTime.BrettsBirthday;
			AssertNoErrorContaining(cusCodeList.ZZD_EndDateInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_EndDate = ZDateTime.Empty;
			AssertHasErrorContaining(cusCodeList.ZZD_EndDateInfo, MandatoryValidation.MustBeEntered);
			cusCodeList.ZZD_IsSystem = true;
			AssertNoErrorContaining(cusCodeList.ZZD_EndDateInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckTransportModes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("TYPE1", "TYPE1 DESC");
			helper.CreateNewOrGetExistingCusCodeType("TYPE2", "TYPE2 DESC");
			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, "TYPE1", "AA", "AA THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.China, "TYPE1", "BB", "BB THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, "TYPE2", "CC", "CC THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTransportModeForCusCodeList(codeList1.PK, "ROA");
			Factory.Save();
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CodeType = "TYPE1";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.China;
			var propertyInfos = RefTransportModesHelper.GetList(Factory).Cast<CodeDescriptionPair>().Select(x => cusCodeList.GetZPropertyInfoForTransportMode(x.Code));
			foreach (var propertyInfo in propertyInfos)
			{
				propertyInfo.Value = ZBool.False;
				AssertNoErrorContaining(propertyInfo, "does not support Transport Modes");
			}

			foreach (var propertyInfo in propertyInfos)
			{
				propertyInfo.Value = ZBool.True;
				AssertHasErrorContaining(propertyInfo, "does not support Transport Modes");
				AssertHasError(propertyInfo, ZZRefCusCodeListCombinedValidation.DoesNotSupportTransportModes("TYPE1", Core.Constants.CountryCodes.China));
			}

			cusCodeList.ZZD_CodeType = "TYPE2";
			foreach (var propertyInfo in propertyInfos)
			{
				AssertHasErrorContaining(propertyInfo, "does not support Transport Modes");
				AssertHasError(propertyInfo, ZZRefCusCodeListCombinedValidation.DoesNotSupportTransportModes("TYPE2", Core.Constants.CountryCodes.China));
			}

			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			foreach (var propertyInfo in propertyInfos)
			{
				AssertHasErrorContaining(propertyInfo, "does not support Transport Modes");
				AssertHasError(propertyInfo, ZZRefCusCodeListCombinedValidation.DoesNotSupportTransportModes("TYPE2", Core.Constants.CountryCodes.Eritrea));
			}

			cusCodeList.ZZD_CodeType = "TYPE1";
			foreach (var propertyInfo in propertyInfos)
			{
				AssertNoErrorContaining(propertyInfo, "does not support Transport Modes");
			}
		}
	}
}
