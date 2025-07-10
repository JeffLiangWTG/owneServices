using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;

namespace Enterprise.Customs.Universal.Testing;

sealed class RefCusCodeListMultipleIdenticalAttributeValueRecordsValidatorTest : TestCaseWithFactory
{
	public void TestConstructor_Null() => AssertArgumentExceptionThrown("codeListCombined", () => GetValidator(null));

	public void TestCheckMultipleIdenticalAttributeValueRecordsHaveStartAndEndDate()
	{
		const string errorMessage = "For Attributes which have the same name and value it's mandatory to provide a Start Date and End Date.";

		var codeType = helper.CreateCusCodeType("TYPE1", "DESC");
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
		var nonMatchingAttributeCombined = codeListCombined.Attributes.AddNew();
		nonMatchingAttributeCombined.ZZE_ZXE_NKName = "ATTR1";
		nonMatchingAttributeCombined.ZZE_Value = YesNoList.Codes.No;

		var validator = GetValidator(codeListCombined);
		CombineAssertions(() =>
		{
			validator.CheckMultipleIdenticalAttributeValueRecords();
			AssertHasRowError("matchingAttributeCombined: no dates", matchingAttributeCombined, errorMessage);
			AssertHasRowError("matchingAttributeCombined2: no dates", matchingAttributeCombined2, errorMessage);
			AssertNoRowError("nonMatchingAttributeCombined: no dates", nonMatchingAttributeCombined, errorMessage);

			matchingAttributeCombined.ZZE_StartDate = ZDateTime.Today;
			matchingAttributeCombined2.ZZE_EndDate = ZDateTime.Today.AddDays(10);
			validator.CheckMultipleIdenticalAttributeValueRecords();
			AssertHasRowError("matchingAttributeCombined: has StartDate", matchingAttributeCombined, errorMessage);
			AssertHasRowError("matchingAttributeCombined2: has EndDate", matchingAttributeCombined2, errorMessage);

			matchingAttributeCombined.ZZE_EndDate = ZDateTime.Today.AddDays(1);
			validator.CheckMultipleIdenticalAttributeValueRecords();
			AssertNoRowError("matchingAttributeCombined: has both Dates", matchingAttributeCombined, errorMessage);
			AssertHasRowError("matchingAttributeCombined2: has EndDate", matchingAttributeCombined2, errorMessage);

			matchingAttributeCombined2.ZZE_StartDate = ZDateTime.Today.AddDays(9);
			validator.CheckMultipleIdenticalAttributeValueRecords();
			AssertNoRowError("matchingAttributeCombined: has both Dates", matchingAttributeCombined, errorMessage);
			AssertNoRowError("matchingAttributeCombined2: has both Dates", matchingAttributeCombined2, errorMessage);
		});
	}

	public void TestCheckMultipleIdenticalAttributeValueRecordsHaveNoOverlappingValidityPeriod()
	{
		const string errorMessage = "Attributes which have the same name and value must not have overlapping validity periods. Please adjust Start Dates and End Dates accordingly.";

		var codeType = helper.CreateCusCodeType("TYPE1", "DESC");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "DESC ER");
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATTR1", "DESC", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, allowDuplicates: true, isDateRangeUsed: true);
		var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType.ZZK_CodeType, "CODE1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		helper.CreateCusCodeListAttribute(codeList.PK, "ATTR1", ZString.Empty, false);
		Factory.Save();

		var codeListCombined = Factory.New<ZZRefCusCodeListCombined>();
		codeListCombined.ZZD_CodeType = codeType.ZZK_CodeType;
		var matchingAttributeCombined = codeListCombined.Attributes.AddNew();
		matchingAttributeCombined.ZZE_ZXE_NKName = "ATTR1";
		matchingAttributeCombined.ZZE_Value = YesNoList.Codes.Yes;
		var matchingAttributeCombined2 = codeListCombined.Attributes.AddNew();
		matchingAttributeCombined2.ZZE_ZXE_NKName = "ATTR1";
		matchingAttributeCombined2.ZZE_Value = YesNoList.Codes.Yes;
		var matchingAttributeCombined3 = codeListCombined.Attributes.AddNew();
		matchingAttributeCombined3.ZZE_ZXE_NKName = "ATTR1";
		matchingAttributeCombined3.ZZE_Value = YesNoList.Codes.Yes;
		var nonMatchingAttributeCombined = codeListCombined.Attributes.AddNew();
		nonMatchingAttributeCombined.ZZE_ZXE_NKName = "ATTR1";
		nonMatchingAttributeCombined.ZZE_Value = YesNoList.Codes.No;

		var validator = GetValidator(codeListCombined);
		CombineAssertions(() =>
		{
			matchingAttributeCombined.ZZE_StartDate = new ZDateTime(2024, 10, 1);
			matchingAttributeCombined.ZZE_EndDate = new ZDateTime(2024, 10, 31);
			matchingAttributeCombined2.ZZE_StartDate = new ZDateTime(2024, 10, 31);
			matchingAttributeCombined2.ZZE_EndDate = new ZDateTime(2024, 11, 30);
			matchingAttributeCombined3.ZZE_StartDate = new ZDateTime(2024, 12, 1);
			matchingAttributeCombined3.ZZE_EndDate = new ZDateTime(2024, 12, 31);
			nonMatchingAttributeCombined.ZZE_StartDate = new ZDateTime(2024, 10, 1);
			nonMatchingAttributeCombined.ZZE_EndDate = new ZDateTime(2024, 10, 31);
			validator.CheckMultipleIdenticalAttributeValueRecords();
			AssertHasRowError("matchingAttributeCombined: overlapps with matchingAttributeCombined2", matchingAttributeCombined, errorMessage);
			AssertHasRowError("matchingAttributeCombined2: overlapps with matchingAttributeCombined", matchingAttributeCombined2, errorMessage);
			AssertHasRowError("matchingAttributeCombined3: no overlapping", matchingAttributeCombined3, errorMessage);
			AssertNoRowError("nonMatchingAttributeCombined: no overlapping as single record", nonMatchingAttributeCombined, errorMessage);

			matchingAttributeCombined2.ZZE_StartDate = new ZDateTime(2024, 11, 1);
			matchingAttributeCombined2.ZZE_EndDate = new ZDateTime(2024, 11, 30);
			validator.CheckMultipleIdenticalAttributeValueRecords();
			AssertNoRowError("matchingAttribute: no overlapping", matchingAttributeCombined, errorMessage);
			AssertNoRowError("matchingAttribute2: no overlapping", matchingAttributeCombined2, errorMessage);
			AssertNoRowError("matchingAttribute3: still no overlapping", matchingAttributeCombined3, errorMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		helper = new UniversalReferenceTestDataHelper(Factory);
	}
	UniversalReferenceTestDataHelper helper;

	RefCusCodeListMultipleIdenticalAttributeValueRecordsValidator GetValidator(ZZRefCusCodeListCombined codeListCombined) => new(codeListCombined);
}
