using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC057CMessageInterpreterTest : MessageInterpreterTest<IIE057>
{
	public void TestInterpret()
	{
		const string testMrn = "TEST_MRN";
		const string testTrader = "TEST_TRADER";
		const string testRejectionDate = "2024-04-24";
		const string testRejectionReason = "TEST_REJECTION_REASON";
		const int testErrorCode = 12;
		const string testErrorCodeDescription = "Codelist violation";
		const string testErrorReason = "TEST_ERROR_REASON_1";
		const string testErrorPointer = "TEST_ERROR_POINTER_1";
		const string testAttribute = "TEST_ORIGINAL_ATTRIBUTE_VALUE_1";

		const string expectedInterpretation =
			ExtendedGlobalHtmlStyle
			+ "<h2>IE057 - Functional Error</h2><hr />"
			+ "<table class=\"no-border bold-font\">"
				+ "<tbody>"
					+ $"<tr><th>MRN</th><td>: {testMrn}</td></tr>"
					+ "<tr><th>Business Rejection Type</th><td>: </td></tr>"
					+ $"<tr><th>Rejection Date &amp; Time</th><td>: {testRejectionDate}</td></tr>"
					+ "<tr><th>Rejection Code</th><td>: </td></tr>"
					+ $"<tr><th>Rejection Reason</th><td>: {testRejectionReason}</td></tr>"
					+ "<tr><th>Customs Office Of Destination</th><td>: </td></tr>"
				+ "</tbody>"
			+ "</table><hr />"
			+ "<table>"
				+ "<caption><h3>Functional Error</h3></caption>"
				+ "<tbody>"
					+ "<tr><th>Code</th><th>Reason</th><th>Error Pointer</th><th>Error Reference Field</th></tr>"
					+ $"<tr><td>12</td><td>{testErrorReason}<br />{testErrorCodeDescription}</td><td>{testErrorPointer}</td><td>{testAttribute}</td></tr>"
				+ "</tbody>"
			+ "</table>"
			+ "<table class=\"no-border bold-font\">"
				+ "<caption><h3>Trader At Destination</h3></caption>"
				+ "<tbody>"
					+ $"<tr><th>EORI</th><td>: {testTrader}</td></tr>"
				+ "</tbody>"
			+ "</table>";

		FunctionalErrorInterpreterTest.CreateCodeWithDescriptionForCL180(Factory, testErrorCode, testErrorCodeDescription);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var interpreter = new CC057CMessageInterpreter(nctsHeader.ArrivalMovementHeader);
		var dataProvider = Mock.Of<IIE057>(x =>
			x.MRN == testMrn &&
			x.TraderAtDestination == testTrader &&
			x.FunctionalErrors == new[]
			{
				Mock.Of<IFunctionalError>(e =>
					e.ErrorCode == testErrorCode &&
					e.ErrorReason == testErrorReason &&
					e.ErrorPointer == testErrorPointer &&
					e.OriginalAttributeValue == testAttribute),
			} &&
			x.TransitOperation == Mock.Of<ICC057CTransitOperation>(t =>
				t.RejectionDateAndTime == testRejectionDate &&
				t.RejectionReason == testRejectionReason));
		var interpretation = interpreter.Interpret(dataProvider).ToString();
		AssertEquals(expected: expectedInterpretation, actual: interpretation);
	}

	public void TestBusinessRejectionType() => CombineAssertions(() =>
	{
		const string testBusinessRejectionType = "007";
		const string testBusinessRejectionTypeDescription = "Arrival notification rejection";
		const string testInvalidBusinessRejectionType = "000";

		CreateCodeWithDescriptionForCodeList(testBusinessRejectionType, testBusinessRejectionTypeDescription, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL570);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var interpreter = new CC057CMessageInterpreter(nctsHeader.ArrivalMovementHeader);
		var transitOperationMock = new Mock<ICC057CTransitOperation>();
		var dataProvider = Mock.Of<IIE057>(x => x.TransitOperation == transitOperationMock.Object);

		transitOperationMock.Setup(x => x.BusinessRejectionType).Returns(testBusinessRejectionType);
		var expectedLine = $"</tr><tr><th>Business Rejection Type</th><td>: {testBusinessRejectionType} - {testBusinessRejectionTypeDescription}</td></tr>";
		AssertLineExists(interpreter, dataProvider, expectedLine, description: "Description is defined for specified code: ");

		transitOperationMock.Setup(x => x.BusinessRejectionType).Returns(testInvalidBusinessRejectionType);
		expectedLine = $"</tr><tr><th>Business Rejection Type</th><td>: {testInvalidBusinessRejectionType}</td></tr>";
		AssertLineExists(interpreter, dataProvider, expectedLine, description: "Description is not defined for specified code: ");
	});

	public void TestRejectionCode() => CombineAssertions(() =>
	{
		const string testRejectionCode = "4";
		const string testRejectionCodeDescription = "Other reasons";
		const string testInvalidRejectionCode = "444";

		CreateCodeWithDescriptionForCodeList(testRejectionCode, testRejectionCodeDescription, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL227);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var interpreter = new CC057CMessageInterpreter(nctsHeader.ArrivalMovementHeader);
		var transitOperationMock = new Mock<ICC057CTransitOperation>();
		var dataProvider = Mock.Of<IIE057>(x => x.TransitOperation == transitOperationMock.Object);

		transitOperationMock.Setup(t => t.RejectionCode).Returns(testRejectionCode);
		var expectedLine = $"</tr><tr><th>Rejection Code</th><td>: {testRejectionCode} - {testRejectionCodeDescription}</td></tr>";
		AssertLineExists(interpreter, dataProvider, expectedLine, description: "Description is defined for specified code: ");

		transitOperationMock.Setup(t => t.RejectionCode).Returns(testInvalidRejectionCode);
		expectedLine = $"</tr><tr><th>Rejection Code</th><td>: {testInvalidRejectionCode}</td></tr>";
		AssertLineExists(interpreter, dataProvider, expectedLine, description: "Description is not defined for specified code: ");
	});

	public void TestCustomsOfficeOfDestination()
	{
		const string testOfficeCode = $"{CountryCodes.Poland}1234";
		const string testOfficeDescription = "Test Office Of Destination";

		Factory.CreateCustomOfficesForTest((code: testOfficeCode, description: testOfficeDescription));
		Factory.Save();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var interpreter = new CC057CMessageInterpreter(nctsHeader.ArrivalMovementHeader);
		var dataProvider = Mock.Of<IIE057>(x => x.CustomsOfficeOfDestinationActual == testOfficeCode);

		AssertLineExists(interpreter, dataProvider, $"<tr><th>Customs Office Of Destination</th><td>: {testOfficeCode} - {testOfficeDescription}</td></tr>");
	}

	void CreateCodeWithDescriptionForCodeList(string code, string description, string codeList)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		var plGrouping = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, parent: eunGrouping);
		_ = helper.CreateNewOrGetExistingCusCodeType(codeList, $"Test CusCodeType {codeList}", eunGrouping.ZZZ_DataGrouping);
		_ = helper.CreateCusCodeList(plGrouping.ZZZ_DataGrouping, codeList, code, description, DateTime.Now.AddMonths(-2), DateTime.Now.AddMonths(2));
		Factory.Save();
	}
}
