using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	sealed class ZZRefCusCodeListAttributeCombinedValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll_CheckBothOrNonDatesArePopulated()
		{
			const string errorMessage = "Both or none of the dates (Start Date, End Date) must be populated.";

			var codeType = helper.CreateCusCodeType("TYPE1", "DESC");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "DESC ER");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATTR1", "DESC", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, isDateRangeUsed: true);
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType.ZZK_CodeType, "CODE1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeListAttribute(codeList.PK, "ATTR1", ZString.Empty, false);
			Factory.Save();

			var codeListCombined = Factory.New<ZZRefCusCodeListCombined>();
			codeListCombined.ZZD_CodeType = codeType.ZZK_CodeType;
			codeListCombined.ZZD_Code = "CODE1";
			codeListCombined.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			var attributeCombined = codeListCombined.Attributes.AddNew();
			attributeCombined.ZZE_ZXE_NKName = "ATTR1";
			attributeCombined.ZZE_Value = YesNoList.Codes.Yes;

			var validation = attributeCombined.Validation;
			CombineAssertions(() =>
			{
				validation.ValidateAll();
				AssertNoRowError("None of the dates populated", attributeCombined, errorMessage);

				attributeCombined.ZZE_StartDate = ZDateTime.Today;
				validation.ValidateAll();
				AssertHasRowError("StartDate populated", attributeCombined, errorMessage);

				attributeCombined.ZZE_StartDate = ZDateTime.Empty;
				attributeCombined.ZZE_EndDate = ZDateTime.Today.AddDays(1);
				validation.ValidateAll();
				AssertHasRowError("EndDate populated", attributeCombined, errorMessage);

				attributeCombined.ZZE_StartDate = ZDateTime.Today;
				validation.ValidateAll();
				AssertNoRowError("Both dates populated", attributeCombined, errorMessage);
			});
		}

		public void TestValidateAll_CheckBothOrNonDatesArePopulated_HasMultipleIdenticalAttributeValueRecordsHaveStartAndEndDateRowError()
		{
			const string errorMessage = "Both or none of the dates (Start Date, End Date) must be populated.";

			var codeType = helper.CreateCusCodeType("TYPE1", "DESC");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "DESC ER");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATTR1", "DESC", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, isDateRangeUsed: true);
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType.ZZK_CodeType, "CODE1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeListAttribute(codeList.PK, "ATTR1", ZString.Empty, false);
			Factory.Save();

			var codeListCombined = Factory.New<ZZRefCusCodeListCombined>();
			codeListCombined.ZZD_CodeType = codeType.ZZK_CodeType;
			codeListCombined.ZZD_Code = "CODE1";
			codeListCombined.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			var attributeCombined = codeListCombined.Attributes.AddNew();
			attributeCombined.ZZE_ZXE_NKName = "ATTR1";
			attributeCombined.ZZE_Value = YesNoList.Codes.Yes;

			var validation = attributeCombined.Validation;
			CombineAssertions(() =>
			{
				attributeCombined.ZZE_StartDate = ZDateTime.Empty;
				attributeCombined.ZZE_EndDate = ZDateTime.Today.AddDays(1);
				validation.ValidateAll();
				AssertHasRowError("StartDate not populated", attributeCombined, errorMessage);

				attributeCombined.AddRowError(new RefCusCodeListMultipleIdenticalAttributeValueRecordsValidator(codeListCombined).MultipleIdenticalAttributeValueRecordsHaveStartAndEndDateRowErrorMessage);
				validation.ValidateAll();
				AssertNoRowError("StartDate not populated but HasMultipleIdenticalAttributeValueRecordsHaveStartAndEndDateRowError", attributeCombined, errorMessage);
			});
		}

		public void TestCheckZZE_StartDate_StartDateHasToBeEarlierThanEndDate()
		{
			const string errorMessage = "Start Date cannot be after End Date.";

			var codeType = helper.CreateCusCodeType("TYPE1", "DESC");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "DESC ER");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATTR1", "DESC", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, isDateRangeUsed: true);
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType.ZZK_CodeType, "CODE1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeListAttribute(codeList.PK, "ATTR1", ZString.Empty, false);
			Factory.Save();

			var codeListCombined = Factory.New<ZZRefCusCodeListCombined>();
			codeListCombined.ZZD_CodeType = codeType.ZZK_CodeType;
			codeListCombined.ZZD_Code = "CODE1";
			codeListCombined.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			var attribute = codeListCombined.Attributes.AddNew();
			attribute.ZZE_ZXE_NKName = "ATTR1";
			attribute.ZZE_Value = YesNoList.Codes.Yes;
			var propertyInfo = attribute.ZZE_StartDateInfo;

			CombineAssertions(() =>
			{
				attribute.Validation.ValidateZZE_StartDate();
				AssertNoError("No dates", propertyInfo, errorMessage);

				attribute.ZZE_StartDate = ZDateTime.Today;
				AssertNoError("StartDate: Today", propertyInfo, errorMessage);

				attribute.ZZE_EndDate = ZDateTime.Today;
				attribute.ZZE_StartDate = ZDateTime.Empty;
				AssertNoError("EndDate: Today", propertyInfo, errorMessage);

				attribute.ZZE_StartDate = ZDateTime.Today;
				AssertNoError("Both Dates: Today", propertyInfo, errorMessage);

				attribute.ZZE_EndDate = ZDateTime.Today.AddDays(-1);
				attribute.Validation.ValidateZZE_StartDate();
				AssertHasError("StartDate > EndDate", propertyInfo, errorMessage);

				attribute.ZZE_EndDate = ZDateTime.Today.AddDays(1);
				attribute.Validation.ValidateZZE_StartDate();
				AssertNoError("StartDate < EndDate", propertyInfo, errorMessage);
			});
		}

		public void TestCheckTransportModes()
		{
			helper.CreateOrGetLanguage("FR", "French");
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
			helper.CreateNewOrGetExistingCusCodeType("TYPE1", "TYPE1 DESC");
			helper.CreateNewOrGetExistingCusCodeType("TYPE2", "TYPE2 DESC");
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATTR2", "Desc.", "TYPE1", Core.Constants.CountryCodes.Eritrea, "Test");
			var refCusCodeListAttributeNameLanguage = Factory.New<RefCusCodeListAttributeNameLanguage>();
			refCusCodeListAttributeNameLanguage.ZXH_ZXE_CodeListAttributeName = attributeName.PK;
			refCusCodeListAttributeNameLanguage.ZXH_ZX6_NKLanguage = "FR";
			refCusCodeListAttributeNameLanguage.ZXH_Name = "A2FR";
			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, "TYPE1", "AA", "AA THE BUILDER", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var attribute1 = helper.CreateCusCodeListAttribute(codeList1.PK, "ATTR1", "Attribute 11");
			var attribute2 = helper.CreateCusCodeListAttribute(codeList1.PK, "ATTR2", "Attribute 12");
			helper.CreateTransportModeForCusCodeAttribute(attribute1.PK, "ROA");
			Factory.Save();
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CodeType = "TYPE1";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			var attribute = cusCodeList.Attributes.AddNew();
			attribute.ZZE_ZXE_NKName = "ATTR2";
			var propertyInfos = RefTransportModesHelper.GetList(Factory).Cast<CodeDescriptionPair>().Select(x => attribute.GetZPropertyInfoForTransportMode(x.Code));
			foreach (var propertyInfo in propertyInfos)
			{
				propertyInfo.Value = ZBool.False;
				AssertNoErrorContaining(propertyInfo, "does not support Transport Modes");
			}

			foreach (var propertyInfo in propertyInfos)
			{
				propertyInfo.Value = ZBool.True;
				AssertHasErrorContaining(propertyInfo, "does not support Transport Modes");
				AssertHasError(propertyInfo, ZZRefCusCodeListAttributeCombinedValidation.DoesNotSupportTransportModes("TYPE1", Core.Constants.CountryCodes.Eritrea, "A2FR"));
			}

			attribute.ZZE_ZXE_NKName = "ATTR1";
			foreach (var propertyInfo in propertyInfos)
			{
				AssertNoErrorContaining(propertyInfo, "does not support Transport Modes");
			}
		}

		public void TestDupplicateAttributeName()
		{
			var codeType = helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "DESC FAC");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ethiopia, "DESC ET");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, codeType.ZZK_CodeType, "abc", DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1));
			helper.CreateCusCodeListAttribute(codeList.PK, "attr1", ZString.Empty);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("abc", "DESC abc", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Ethiopia, codeType.ZZK_CodeType, allowDuplicates: true);
			Factory.Save();
			var errorMessage = "Duplicate attribute name 'abc' is not allowed.";
			var attr1 = codeList.Attributes.AddNew("abc", ZString.Empty);
			var listAttributeCombined = Factory.Load<ZZRefCusCodeListAttributeCombined>(new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, codeList.PK)).FirstOrDefault();
			listAttributeCombined.ZZE_ZXE_NKName = "abc";
			AssertNoError(listAttributeCombined.CodeListAttributeNamePKInfo, errorMessage);
			attributeName.ZXE_AllowDuplicates = false;
			Factory.Save();
			listAttributeCombined.ZZE_ZXE_NKName = "abc";
			AssertHasError(listAttributeCombined.CodeListAttributeNamePKInfo, errorMessage);
		}

		public void TestMandatoryValue()
		{
			var codeType = helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "DESC FAC");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ethiopia, "DESC ET");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, codeType.ZZK_CodeType, "abc", DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1));
			helper.CreateCusCodeListAttribute(codeList.PK, "attr1", ZString.Empty);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("abc", "DESC abc", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Ethiopia, codeType.ZZK_CodeType);
			Factory.Save();
			var errorMessage = "Value is mandatory for attribute abc.";
			var listAttributeCombined = Factory.Load<ZZRefCusCodeListAttributeCombined>(new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, codeList.PK)).FirstOrDefault();
			listAttributeCombined.ZZE_ZXE_NKName = "abc";
			listAttributeCombined.ZZE_Value = ZString.Empty;
			AssertNoError(listAttributeCombined.ZZE_ValueInfo, errorMessage);
			attributeName.ZXE_IsValueMandatory = true;
			Factory.Save();
			listAttributeCombined.ZZE_ZXE_NKName = "abc";
			listAttributeCombined.ZZE_Value = ZString.Empty;
			AssertHasError(listAttributeCombined.ZZE_ValueInfo, errorMessage);
			attributeName.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.String;
			listAttributeCombined.Validation.ValidateZZE_Value();
			AssertHasError(listAttributeCombined.ZZE_ValueInfo, errorMessage);
			attributeName.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean;
			listAttributeCombined.Validation.ValidateZZE_Value();
			AssertNoError(listAttributeCombined.ZZE_ValueInfo, errorMessage);
			attributeName.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer;
			listAttributeCombined.Validation.ValidateZZE_Value();
			AssertNoError(listAttributeCombined.ZZE_ValueInfo, errorMessage);
			attributeName.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal;
			listAttributeCombined.Validation.ValidateZZE_Value();
			AssertNoError(listAttributeCombined.ZZE_ValueInfo, errorMessage);
		}

		public void TestValueRange()
		{
			helper.CreateCusCodeType("Test", "Desc", Core.Constants.CountryCodes.Eritrea);
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice", Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "DESC ER");
			var cusCodeList1 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList1.ZZD_CodeType = C.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList1.ZZD_Code = "B0B";
			cusCodeList1.ZZD_Description = "BOB THE BUILDER";
			cusCodeList1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			cusCodeList1.ZZD_StartDate = ZDateTime.Today.AddYears(-1);
			cusCodeList1.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			var cusCodeListAttributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("T1", "Desc", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea, "Test");
			cusCodeListAttributeName1.ZXE_MinLengthOrValue = 2;
			Factory.Save();
			var attribute1 = cusCodeList1.Attributes.AddNew("T1", "A");
			attribute1.Validation.ValidateZZE_Value();
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueLengthIsLessThanMinLength(2));
			cusCodeListAttributeName1.ZXE_MaxLengthOrValue = 4;
			Factory.Save();
			attribute1.Validation.ValidateZZE_Value();
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueLengthRangeIsInvalid(2, 4));
			attribute1.ZZE_Value = "AAAAA";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueLengthRangeIsInvalid(2, 4));
			attribute1.ZZE_Value = "AAAA";
			AssertNoErrors(attribute1.ZZE_ValueInfo);
			cusCodeListAttributeName1.ZXE_MinLengthOrValue = 0;
			Factory.Save();
			attribute1.ZZE_Value = "AAAAA";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueLengthRangeIsInvalid(0, 4));
			cusCodeListAttributeName1.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.String;
			cusCodeListAttributeName1.ZXE_MinLengthOrValue = 2;
			cusCodeListAttributeName1.ZXE_MaxLengthOrValue = 4;
			Factory.Save();
			attribute1.ZZE_Value = "A";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueLengthRangeIsInvalid(2, 4));
			attribute1.ZZE_Value = "AAAAA";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueLengthRangeIsInvalid(2, 4));
			attribute1.ZZE_Value = "AAAA";
			AssertNoErrors(attribute1.ZZE_ValueInfo);
			cusCodeListAttributeName1.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer;
			cusCodeListAttributeName1.ZXE_MaxLengthOrValue = 0;
			Factory.Save();
			attribute1.ZZE_Value = "1";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueIsLessThanMinValue(2, 0));
			cusCodeListAttributeName1.ZXE_MaxLengthOrValue = 10000;
			Factory.Save();
			attribute1.Validation.ValidateZZE_Value();
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueRangeIsInvalid(2, (ZInt)10000, 0));
			attribute1.ZZE_Value = "0";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueRangeIsInvalid(2, (ZInt)10000, 0));
			attribute1.ZZE_Value = "11111";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueRangeIsInvalid(2, (ZInt)10000, 0));
			attribute1.ZZE_Value = "1111";
			AssertNoErrors(attribute1.ZZE_ValueInfo);
			cusCodeListAttributeName1.ZXE_MinLengthOrValue = 0;
			Factory.Save();
			attribute1.ZZE_Value = "11111";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueRangeIsInvalid(0, (ZInt)10000, 0));
			cusCodeListAttributeName1.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal;
			cusCodeListAttributeName1.ZXE_MinLengthOrValue = 2;
			cusCodeListAttributeName1.ZXE_MaxLengthOrValue = 0;
			cusCodeListAttributeName1.ZXE_DecimalPlaces = 2;
			Factory.Save();
			attribute1.ZZE_Value = "1";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueIsLessThanMinValue(2, 2));
			cusCodeListAttributeName1.ZXE_MaxLengthOrValue = 10000.80000M;
			Factory.Save();
			attribute1.Validation.ValidateZZE_Value();
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueRangeIsInvalid(2, (ZDecimal)10000.80M, 2));
			attribute1.ZZE_Value = "0.00";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueRangeIsInvalid(2, (ZDecimal)10000.80M, 2));
			attribute1.ZZE_Value = "11111";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueRangeIsInvalid(2, (ZDecimal)10000.80M, 2));
			attribute1.ZZE_Value = "1111.77";
			AssertNoErrors(attribute1.ZZE_ValueInfo);
			cusCodeListAttributeName1.ZXE_MinLengthOrValue = 0;
			Factory.Save();
			attribute1.ZZE_Value = "11111";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ZZRefCusCodeListAttributeCombinedValidation.ValueRangeIsInvalid(0, (ZDecimal)10000.80M, 2));
			cusCodeListAttributeName1.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean;
			cusCodeListAttributeName1.ZXE_MinLengthOrValue = 0;
			cusCodeListAttributeName1.ZXE_MaxLengthOrValue = 0;
			cusCodeListAttributeName1.ZXE_DecimalPlaces = 0;
			Factory.Save();
			attribute1.ZZE_Value = "A";
			AssertHasErrorContaining(attribute1.ZZE_ValueInfo, ListValidation.InvalidCodeError);
			attribute1.ZZE_Value = "Y";
			AssertNoErrorContaining(attribute1.ZZE_ValueInfo, ListValidation.InvalidCodeError);
			attribute1.ZZE_Value = "";
			AssertNoErrorContaining(attribute1.ZZE_ValueInfo, ListValidation.InvalidCodeError);
		}

		protected override void SetUp()
		{
			helper = new UniversalReferenceTestDataHelper(Factory);
			base.SetUp();
		}
		UniversalReferenceTestDataHelper helper;
	}
}
