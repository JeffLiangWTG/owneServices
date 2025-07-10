using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using RefCusCodeListType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;
using RefDataGroupingCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CusCodesHelperTest : TestCaseWithFactory
{
	public void TestInAU() => TestInCountry(CountryCodes.Australia);

	public void TestInPL() => TestInCountry(CountryCodes.Poland);

	void TestInCountry(string countryCode)
	{
		var branch = Factory.NewCompanyAndBranchWith(companyCode: "ZZ1", branchCode: "ZZ1", countryCode);
		branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, countryCode)).Code;
		Factory.Save();

		using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
		{
			TestGetOfficeCodeWithDescription();
			TestFindCusCodeDescription_Dates();
		}
	}

	public void TestGetOfficeCodeWithDescription()
	{
		var today = ZDateTime.Today;

		Factory.CreateCustomOfficesForTest((code: $"{CountryCodes.Poland}2233", description: "Polish office"));
		Factory.CreateCustomOfficesForTest(
			startDate: today.AddDays(-2),
			endDate: today.AddDays(-1),
			(code: $"{CountryCodes.Germany}2233", description: "German office in past"));
		Factory.CreateCustomOfficesForTest(
			startDate: today.AddDays(1),
			endDate: today.AddDays(2),
			(code: $"{CountryCodes.Denmark}2233", description: "Danish office in future"));
		Factory.Save();

		CombineAssertions(() =>
		{
			var codeWithDescription = Factory.GetOfficeCodeWithDescription("PL2233");
			AssertEquals("Found office", "PL2233 - Polish office", codeWithDescription);

			codeWithDescription = Factory.GetOfficeCodeWithDescription("1");
			AssertEquals("Found office", "1", codeWithDescription);

			codeWithDescription = Factory.GetOfficeCodeWithDescription("DE2233");
			AssertEquals("Not found wrong office", "DE2233", codeWithDescription);

			AssertNoExceptionThrown("Empty code", () => codeWithDescription = Factory.GetOfficeCodeWithDescription(default));
			AssertEquals("Not found empty code", ZString.Empty, codeWithDescription);

			codeWithDescription = Factory.GetOfficeCodeWithDescription("DK2233");
			AssertEquals("Not found date before start date", "DK2233", codeWithDescription);

			codeWithDescription = Factory.GetOfficeCodeWithDescription("DE2233");
			AssertEquals("Not found date after end date", "DE2233", codeWithDescription);
		});
	}

	public void TestGetCodeWithDescription_Countries()
	{
		const string testCusCode = "1";
		const string testCusCodeDescription = "TEST";

		var today = ZDateTime.Today;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);

		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListType.Code_CL560, $"Test code type {RefCusCodeListType.Code_CL560}", CountryCodes.Poland);
		helper.CreateCusCodeList(CountryCodes.Poland, RefCusCodeListType.Code_CL560, testCusCode, testCusCodeDescription, today.AddDays(-1), today.AddDays(1));
		Factory.Save();

		CombineAssertions(() =>
		{
			var codeWithDescription = Factory.GetCodeWithDescription(RefCusCodeListType.Code_CL560, testCusCode, CountryCodes.Poland);
			AssertEquals("Poland", $"{testCusCode} - {testCusCodeDescription}", codeWithDescription);

			codeWithDescription = Factory.GetCodeWithDescription(RefCusCodeListType.Code_CL560, testCusCode, CountryCodes.Germany);
			AssertEquals("Germany", $"{testCusCode}", codeWithDescription);
		});
	}

	public void TestGetCodeWithDescription_Separator()
	{
		const string testCusCode = "1";
		const string testCusCodeDescription = "TEST";

		var today = ZDateTime.Today;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);

		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListType.Code_CL560, $"Test code type {RefCusCodeListType.Code_CL560}", CountryCodes.Poland);
		helper.CreateCusCodeList(CountryCodes.Poland, RefCusCodeListType.Code_CL560, testCusCode, testCusCodeDescription, today.AddDays(-1), today.AddDays(1));
		Factory.Save();

		CombineAssertions(() =>
		{
			var codeWithDescription = Factory.GetCodeWithDescription(RefCusCodeListType.Code_CL560, testCusCode, CountryCodes.Poland);
			AssertEquals("Separator -", $"{testCusCode} - {testCusCodeDescription}", codeWithDescription);

			codeWithDescription = Factory.GetCodeWithDescription(RefCusCodeListType.Code_CL560, testCusCode, CountryCodes.Poland, separator: "=");
			AssertEquals("Separator -", $"{testCusCode} = {testCusCodeDescription}", codeWithDescription);

			codeWithDescription = Factory.GetCodeWithDescription(RefCusCodeListType.Code_CL560, testCusCode, CountryCodes.Poland, separator: "");
			AssertEquals("No any separator", $"{testCusCode} {testCusCodeDescription}", codeWithDescription);
		});
	}

	public void TestGetCodeWithDescription_Dates()
	{
		var today = ZDateTime.Today;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);

		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListType.Code_CL560, $"Test code type {RefCusCodeListType.Code_CL560}", CountryCodes.Poland);
		helper.CreateCusCodeList(CountryCodes.Poland, RefCusCodeListType.Code_CL560, "1", "PL1", today.AddDays(-1), today.AddDays(1));
		Factory.Save();

		CombineAssertions(() =>
		{
			var codeWithDescription = Factory.GetCodeWithDescription(RefCusCodeListType.Code_CL560, "1", CountryCodes.Poland);
			AssertEquals("Default", "1 - PL1", codeWithDescription);

			codeWithDescription = Factory.GetCodeWithDescription(RefCusCodeListType.Code_CL560, "1", CountryCodes.Poland, date: today);
			AssertEquals("Today", "1 - PL1", codeWithDescription);

			codeWithDescription = Factory.GetCodeWithDescription(RefCusCodeListType.Code_CL560, "1", CountryCodes.Poland, date: today.AddDays(-2));
			AssertEquals("-2 days", "1", codeWithDescription);

			codeWithDescription = Factory.GetCodeWithDescription(RefCusCodeListType.Code_CL560, "1", CountryCodes.Poland, date: today.AddDays(2));
			AssertEquals("+2 days", "1", codeWithDescription);
		});
	}

	public void TestGetCodeWithDescription_FallbackParent()
	{
		var today = ZDateTime.Today;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);

		var europeanUnionGrouping = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCode.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, parent: europeanUnionGrouping);
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListType.Code_CL560, $"Test code type {RefCusCodeListType.Code_CL560}", RefDataGroupingCode.EuropeanUnionEUN);
		helper.CreateCusCodeList(RefDataGroupingCode.EuropeanUnionEUN, RefCusCodeListType.Code_CL560, "1", "PL1", today.AddDays(-1), today.AddDays(1));
		Factory.Save();

		var codeWithDescription = Factory.GetCodeWithDescription(RefCusCodeListType.Code_CL560, "1", CountryCodes.Poland);
		AssertEquals("Default", "1 - PL1", codeWithDescription);
	}

	public void TestFindCusCodeDescription_Countries()
	{
		const string testCusCode = "1";
		const string testCusCodeDescription = "TEST";

		var today = ZDateTime.Today;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);

		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListType.Code_CL560, $"Test code type {RefCusCodeListType.Code_CL560}", CountryCodes.Poland);
		helper.CreateCusCodeList(CountryCodes.Poland, RefCusCodeListType.Code_CL560, testCusCode, testCusCodeDescription, today.AddDays(-1), today.AddDays(1));
		Factory.Save();

		CombineAssertions(() =>
		{
			var codeDescription = Factory.FindCusCodeDescription(RefCusCodeListType.Code_CL560, testCusCode, CountryCodes.Poland);
			AssertEquals("Poland", (testCusCode, testCusCodeDescription), (codeDescription.Code, codeDescription.Description));

			codeDescription = Factory.FindCusCodeDescription(RefCusCodeListType.Code_CL560, testCusCode, CountryCodes.Germany);
			AssertNull("Germany", codeDescription);
		});
	}

	public void TestFindCusCodeDescription_Dates()
	{
		var today = ZDateTime.Today;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);

		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListType.Code_CL560, $"Test code type {RefCusCodeListType.Code_CL560}", CountryCodes.Poland);
		helper.CreateCusCodeList(CountryCodes.Poland, RefCusCodeListType.Code_CL560, "1", "PL1", today.AddDays(-1), today.AddDays(1));
		Factory.Save();

		CombineAssertions(() =>
		{
			var codeDescription = Factory.FindCusCodeDescription(RefCusCodeListType.Code_CL560, "1", CountryCodes.Poland);
			AssertEquals("Default", ("1", "PL1"), (codeDescription.Code, codeDescription.Description));

			codeDescription = Factory.FindCusCodeDescription(RefCusCodeListType.Code_CL560, "1", CountryCodes.Poland, date: today);
			AssertEquals("Today", ("1", "PL1"), (codeDescription.Code, codeDescription.Description));

			codeDescription = Factory.FindCusCodeDescription(RefCusCodeListType.Code_CL560, "1", CountryCodes.Poland, date: today.AddDays(-2));
			AssertNull("-2 days", codeDescription);

			codeDescription = Factory.FindCusCodeDescription(RefCusCodeListType.Code_CL560, "1", CountryCodes.Poland, date: today.AddDays(2));
			AssertNull("+2 days", codeDescription);
		});
	}

	public void TestFindCusCodeDescription_FallbackParent()
	{
		var today = ZDateTime.Today;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);

		var europeanUnionGrouping = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCode.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, parent: europeanUnionGrouping);
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListType.Code_CL560, $"Test code type {RefCusCodeListType.Code_CL560}", RefDataGroupingCode.EuropeanUnionEUN);
		helper.CreateCusCodeList(RefDataGroupingCode.EuropeanUnionEUN, RefCusCodeListType.Code_CL560, "1", "PL1", today.AddDays(-1), today.AddDays(1));
		Factory.Save();

		var codeDescription = Factory.FindCusCodeDescription(RefCusCodeListType.Code_CL560, "1", CountryCodes.Poland);
		AssertEquals("Default", ("1", "PL1"), (codeDescription.Code, codeDescription.Description));
	}

	public void TestGetNoReleaseMotivationCodeWithDescription()
		=> TestGetCodeWithDescription(RefCusCodeListType.Code_CL211);

	public void TestGetNoReleaseMotivationCodeWithDescription_NoDescription()
		=> TestCodeWithDescription_NoDescription(RefCusCodeListType.Code_CL211);

	public void TestGetNoReleaseMotivationCodeWithDescription_Fallback()
		=> TestGetCodeWithDescription_Fallback(RefCusCodeListType.Code_CL211);

	public void TestGetIncidentCodeWithDescription()
		=> TestGetCodeWithDescription(RefCusCodeListType.Code_CL019);

	public void TestGetIncidentCodeWithDescription_NoDescription()
		=> TestCodeWithDescription_NoDescription(RefCusCodeListType.Code_CL019);

	public void TestGetIncidentCodeWithDescription_Fallback()
		=> TestGetCodeWithDescription_Fallback(RefCusCodeListType.Code_CL019);

	void TestGetCodeWithDescription(string codeType)
	{
		const string testCode = "BB";
		const string testDescriptionPL = "BB_PL_DESCRIPTION";
		const string testDescriptionEU = "BB_EU_DESCRIPTION";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCode.EuropeanUnionEUN);
		var polandGrouping = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, parent: euDataGrouping);

		var testCodeType = helper.CreateCusCodeType(codeType, "Test Code Type");
		helper.CreateNewOrGetExistingCusCodeList(euDataGrouping.ZZZ_DataGrouping, testCodeType.ZZK_CodeType, testCode, testDescriptionEU, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateNewOrGetExistingCusCodeList(polandGrouping.ZZZ_DataGrouping, testCodeType.ZZK_CodeType, testCode, testDescriptionPL, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		Factory.Save();

		var codeWithDescription = Factory.GetCodeWithDescription(codeType, testCode);
		AssertEquals("Description from PL", $"{testCode} - {testDescriptionPL}", codeWithDescription);
	}

	void TestCodeWithDescription_NoDescription(string codeType)
	{
		const string testCode = "CC";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCode.EuropeanUnionEUN);
		var notPolandGrouping = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Germany, parent: euDataGrouping);

		var testCodeType = helper.CreateCusCodeType(codeType, "Test Code Type");
		helper.CreateNewOrGetExistingCusCodeList(notPolandGrouping.ZZZ_DataGrouping, testCodeType.ZZK_CodeType, testCode, "NOT_PL_DESCRIPTION", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		Factory.Save();

		CombineAssertions(() =>
		{
			var codeWithDescription = Factory.GetCodeWithDescription(codeType, testCode);
			AssertEquals("No description if code is not defined for EU or PL", testCode, codeWithDescription);

			codeWithDescription = Factory.GetCodeWithDescription(codeType, code: "DD");
			AssertEquals("No description if code is not defined", "DD", codeWithDescription);

			codeWithDescription = Factory.GetCodeWithDescription(codeType, code: string.Empty);
			AssertEquals("No description for empty string", string.Empty, codeWithDescription);

			codeWithDescription = Factory.GetCodeWithDescription(codeType, code: null);
			AssertEquals("No description for null", string.Empty, codeWithDescription);
		});
	}

	void TestGetCodeWithDescription_Fallback(string codeType)
	{
		const string testCode = "AA";
		const string testDescription = "AA_EU_DESCRIPTION";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCode.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, parent: euDataGrouping);

		var testCodeType = helper.CreateCusCodeType(codeType, desc: "Test Code Type");
		helper.CreateNewOrGetExistingCusCodeList(euDataGrouping.ZZZ_DataGrouping, testCodeType.ZZK_CodeType, testCode, testDescription, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		Factory.Save();

		var codeWithDescription = Factory.GetCodeWithDescription(codeType, code: testCode);
		AssertEquals("Description from EU if code is not defined for PL", $"{testCode} - {testDescription}", codeWithDescription);
	}
}
