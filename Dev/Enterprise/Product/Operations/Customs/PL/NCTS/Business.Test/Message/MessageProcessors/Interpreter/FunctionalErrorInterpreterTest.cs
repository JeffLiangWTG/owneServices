using System;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.PL.Business;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class FunctionalErrorInterpreterTest : TestCaseWithFactory
{
	public void TestGetColumnTitles()
	{
		var expectedTitles = new ZString[]
		{
			"Code",
			"Reason",
			"Error Pointer",
			"Error Reference Field",
		};
		var actualTitles = new FunctionalErrorInterpreter(Factory).GetColumnTitles();

		AssertArrayEqualsByElements(actualTitles, expectedTitles);
	}

	public void TestGetRows() => CombineAssertions(() =>
	{
		const int testErrorCode1 = 12;
		const int testErrorCode2 = 14;
		const string testErrorCodeDescription1 = "Code list violation";
		const string testErrorCodeDescription2 = "Rule violation";
		const string testErrorReason1 = "TEST_ERROR_REASON_1";
		const string testErrorReason2 = "TEST_ERROR_REASON_2";
		const string testErrorPointer1 = "TEST_ERROR_POINTER_1";
		const string testErrorPointer2 = "TEST_ERROR_POINTER_2";
		const string testAttribute1 = "TEST_ORIGINAL_ATTRIBUTE_VALUE_1";
		const string testAttribute2 = "TEST_ORIGINAL_ATTRIBUTE_VALUE_2";

		CreateCodeWithDescriptionForCL180(Factory, testErrorCode1, testErrorCodeDescription1);
		CreateCodeWithDescriptionForCL180(Factory, testErrorCode2, testErrorCodeDescription2);

		var functionalErrors = new[]
		{
			Mock.Of<IFunctionalError>(x =>
				x.ErrorCode == testErrorCode1 &&
				x.ErrorReason == testErrorReason1 &&
				x.ErrorPointer == testErrorPointer1 &&
				x.OriginalAttributeValue == testAttribute1),
			Mock.Of<IFunctionalError>(x =>
				x.ErrorCode == testErrorCode2 &&
				x.ErrorReason == testErrorReason2 &&
				x.ErrorPointer == testErrorPointer2 &&
				x.OriginalAttributeValue == testAttribute2),
		};

		var actualRows = new FunctionalErrorInterpreter(Factory).GetRows(functionalErrors);

		AssertEquals("Number of rows", 2, actualRows.Count());

		var expectedRow = actualRows.Cast<ParamValues>().First();
		AssertEquals("Row1: index", "0", expectedRow.Name);
		AssertEquals("Row1: error code", testErrorCode1.ToString(), expectedRow.StringValues[0]);
		AssertEquals("Row1: error reason", new ZString($"{testErrorReason1}\n{testErrorCodeDescription1}"), expectedRow.StringValues[1]);
		AssertEquals("Row1: error pointer", testErrorPointer1, expectedRow.StringValues[2]);
		AssertEquals("Row1: error original attribute value", testAttribute1, expectedRow.StringValues[3]);

		expectedRow = actualRows.Cast<ParamValues>().Last();
		AssertEquals("Row2: index", "1", expectedRow.Name);
		AssertEquals("Row2: error code", testErrorCode2.ToString(), expectedRow.StringValues[0]);
		AssertEquals("Row2: error reason", new ZString($"{testErrorReason2}\n{testErrorCodeDescription2}"), expectedRow.StringValues[1]);
		AssertEquals("Row2: error pointer", testErrorPointer2, expectedRow.StringValues[2]);
		AssertEquals("Row2: error original attribute value", testAttribute2, expectedRow.StringValues[3]);
	});

	public static void CreateCodeWithDescriptionForCL180(BusinessObjectFactory factory, int code, string description)
	{
		const string cl180 = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL180;
		var helper = new UniversalReferenceTestDataHelper(factory);
		var eunGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		var plGrouping = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, parent: eunGrouping);
		_ = helper.CreateNewOrGetExistingCusCodeType(cl180, $"Test CusCodeType {cl180}", eunGrouping.ZZZ_DataGrouping);
		_ = helper.CreateCusCodeList(plGrouping.ZZZ_DataGrouping, cl180, code.ToString(), description, DateTime.Now.AddMonths(-2), DateTime.Now.AddMonths(2));
		factory.Save();
	}
}
