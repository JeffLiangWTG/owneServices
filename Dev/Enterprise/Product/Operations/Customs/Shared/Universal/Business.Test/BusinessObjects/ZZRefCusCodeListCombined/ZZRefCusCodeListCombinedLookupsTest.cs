using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using RefCusCodeListTypeCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.Universal.Testing
{
	sealed class ZZRefCusCodeListCombinedLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeTypeList()
		{
			SetupReferenceData();
			AssertCodeTypeList(true);
		}

		public void TestCodeTypeList2()
		{
			SetupReferenceData();
			AssertCodeTypeList(false);
		}

		void SetupReferenceData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = Core.Constants.CountryCodes.Eritrea;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			helper.CreateCusCodeType(RefCusCodeListTypeCodes.Codes.Facilities, "FAC DESC FOR Eritrea", currentCountry);
			helper.CreateCusCodeType(RefCusCodeListTypeCodes.Codes.Port, "Port DESC", Core.Constants.CountryCodes.Australia);
			helper.CreateCusCodeType(RefCusCodeListTypeCodes.Codes.CustomsOffice, "CustomsOffice DESC", currentCountry, isReadonly: true);
			helper.CreateCusCodeType(RefCusCodeListTypeCodes.Codes.Facilities, "FAC DESC FOR Namibia", Core.Constants.CountryCodes.Namibia);
			Factory.Save();
		}

		void AssertCodeTypeList(bool isSystemRecord)
		{
			var currentCountry = Core.Constants.CountryCodes.Eritrea;
			var cusCodeList1 = CreateCodeList(currentCountry, RefCusCodeListTypeCodes.Codes.Facilities, "c1", "c1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, isSystemRecord);
			var cusCodeList2 = CreateCodeList(currentCountry, RefCusCodeListTypeCodes.Codes.Port, "c2", "c2 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, isSystemRecord);
			var cusCodeList3 = CreateCodeList(currentCountry, RefCusCodeListTypeCodes.Codes.CustomsOffice, "c3", "c3 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, isSystemRecord);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertTypeAndDescription(isSystemRecord + "-Should include non-readonly FAC", cusCodeList1, RefCusCodeListTypeCodes.Codes.Facilities, "FAC DESC FOR Eritrea");
				AssertEquals(isSystemRecord + "-Should not include the code type other than current datagrouping", false, cusCodeList2.Lookups.CodeTypeList.ContainsCode(RefCusCodeListTypeCodes.Codes.Port));
				AssertEquals(isSystemRecord + "-Readonly CUSOF", isSystemRecord, cusCodeList3.Lookups.CodeTypeList.ContainsCode(RefCusCodeListTypeCodes.Codes.CustomsOffice));
			});
		}

		void AssertTypeAndDescription(string message, ZZRefCusCodeListCombined cusCodeList, string expectedType, string expectedDescription)
		{
			var codeTypeList = cusCodeList.Lookups.CodeTypeList;
			AssertEquals("Type:" + message, true, codeTypeList.ContainsCode(expectedType));
			AssertEquals("Description:" + message, expectedDescription, codeTypeList.GetDescriptionFromCode(expectedType));
		}

		ZZRefCusCodeListCombined CreateCodeList(ZString dataGroupingCode, ZString codeType, ZString code, ZString description, ZDateTime startDate, ZDateTime endDate, bool isSystem)
		{
			var codeListCombined = Factory.New<ZZRefCusCodeListCombined>();
			codeListCombined.ZZD_StartDate = startDate;
			codeListCombined.ZZD_EndDate = endDate;
			codeListCombined.ZZD_CountryOrGrouping = dataGroupingCode;
			codeListCombined.ZZD_CodeType = codeType;
			codeListCombined.ZZD_Code = code;
			codeListCombined.ZZD_Description = description;
			codeListCombined.ZZD_IsSystem = isSystem;
			return codeListCombined;
		}
	}
}
