using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CC521CMessageInterpreter))]
sealed class CC521CMessageInterpreterTest : MessageInterpreterTest<ICC521C>
{
	public void TestInterpret()
	{
		const string expectedInterpretation = GlobalHtmlStyle +
		"<h2>CC521C - Diversion rejection notification.</h2>" +
		"<hr /><table><tbody>" +
		"<tr><th>Customs Office of Exit</th><td>OfficeOfExit_TST</td></tr>" +
		"<tr><th>MRN</th><td>MRN_TST</td></tr>" +
		"<tr><th>Date of diversion rejection at Exit</th><td>1/02/2023</td></tr>" +
		"<tr><th>Diversion Rejection Reason Code</th><td>11</td></tr>" +
		"<tr><th>Details about the reason code</th><td>Test rejection details</td></tr>" +
		"</tbody></table>";

		dataProviderMock.Setup(x => x.CustomsOfficeOfExitReferenceNumber).Returns("OfficeOfExit_TST");
		dataProviderMock.Setup(x => x.MRN).Returns("MRN_TST");
		dataProviderMock.Setup(x => x.PreparationDateAndTime).Returns(new DateTime(2023, 02, 01));

		var exportOperationMock = new Mock<ICC521CExportOperation>();
		exportOperationMock.Setup(x => x.DiversionRejectionText).Returns("Test rejection details");
		exportOperationMock.Setup(x => x.DiversionRejectionReasonCode).Returns("11");
		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperationMock.Object);

		var exitReport = Factory.New<CusExitReport>();
		var interpreter = new CC521CMessageInterpreter(exitReport);
		var interpretation = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals(expectedInterpretation, interpretation);
	}

	public void TestDiversionRejectionReasonCodeDescription()
	{
		InitializeCodeAndDescriptionForCodeType("12", "MRN unknown", "CL046");

		var exitReport = Factory.New<CusExitReport>();
		var interpreter = new CC521CMessageInterpreter(exitReport);
		var exportOperationMock = new Mock<ICC521CExportOperation>();
		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperationMock.Object);

		CombineAssertions(() =>
		{
			exportOperationMock.Setup(x => x.DiversionRejectionReasonCode).Returns((string)null);
			var actual = InterpretAndGetCodeWithDescription();
			AssertEquals("Code is null", string.Empty, actual);

			exportOperationMock.Setup(x => x.DiversionRejectionReasonCode).Returns("12");
			actual = InterpretAndGetCodeWithDescription();
			AssertEquals("Code is in the list", "12 - MRN unknown", actual);

			exportOperationMock.Setup(x => x.DiversionRejectionReasonCode).Returns("14");
			actual = InterpretAndGetCodeWithDescription();
			AssertEquals("Code is not in the list", "14", actual);
		});

		string InterpretAndGetCodeWithDescription() => interpreter.Interpret(dataProviderMock.Object).ToString()
			.GetTextBetween("<tr><th>Diversion Rejection Reason Code</th><td>", "</td></tr>");
	}

	void InitializeCodeAndDescriptionForCodeType(string code, string codeDescription, string codeType)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		var plGrouping = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, parent: eunGrouping);
		helper.CreateNewOrGetExistingCusCodeType(codeType, $"Test code type {codeType}", eunGrouping.ZZZ_DataGrouping);
		helper.CreateNewOrGetExistingCusCodeList(plGrouping.ZZZ_DataGrouping, codeType, code, codeDescription, new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
		Factory.Save();
	}
}
