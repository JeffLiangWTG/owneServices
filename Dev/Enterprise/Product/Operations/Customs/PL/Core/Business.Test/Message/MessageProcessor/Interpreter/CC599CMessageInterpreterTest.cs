using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC599CMessageInterpreter))]
sealed class CC599CMessageInterpreterTest : MessageInterpreterTest<ICC599C>
{
	public void TestInterpret()
	{
		const string expectedInterpretationEmptyExitControlResult = GlobalHtmlStyle +
																	"<table>" +
																	"<caption><h3>CC599C</h3></caption>" +
																	"<tbody>" +
																	"<tr><th>LRN</th><td></td></tr>" +
																	"<tr><th>MRN</th><td></td></tr>" +
																	"</tbody>" +
																	"</table>" +
																	"<p>Status is set to </p>";

		const string expectedInterpretationNotEmptyExitControlResult = GlobalHtmlStyle +
																		"<table>" +
																		"<caption><h3>CC599C</h3></caption>" +
																		"<tbody>" +
																		"<tr><th>LRN</th><td></td></tr>" +
																		"<tr><th>MRN</th><td></td></tr>" +
																		"</tbody>" +
																		"</table>" +
																		"<p>Declaration has exited the EU on  according to customs office .</p>" +
																		"<p>Control result code is  </p>" +
																		"<p>State of seals is stateOfSeals Not Ok</p>" +
																		"<p>Status is set to </p>";

		var interpreter = new CC599CMessageInterpreter(entryHeader);
		CombineAssertions(() =>
		{
			dataProviderMock.Setup(x => x.ExitControlResult).Returns((IExitControlResult)null);
			var interpretation = interpreter.Interpret(dataProviderMock.Object);
			AssertEquals("Empty ExitControlResult", expectedInterpretationEmptyExitControlResult, interpretation);

			dataProviderMock.Setup(x => x.ExitControlResult).Returns(exitControlResult.Object);
			interpretation = interpreter.Interpret(dataProviderMock.Object);
			AssertEquals("Not empty ExitControlResult", expectedInterpretationNotEmptyExitControlResult, interpretation);
		});
	}

	public void TestDescription()
	{
		const string expectedDescription = "CC599C";
		var interpreter = new CC599CMessageInterpreter(entryHeader);
		AssertLineExists(interpreter, dataProviderMock.Object, expectedDescription, description: "Description");
	}

	public void TestLineLRN()
	{
		const string testLrn = "TST_LRN";
		const string expectedMrn = $"<tr><th>LRN</th><td>{testLrn}</td></tr>";
		dataProviderMock.Setup(m => m.LRN).Returns(testLrn);
		var interpreter = new CC599CMessageInterpreter(entryHeader);
		AssertLineExists(interpreter, dataProviderMock.Object, expectedMrn, description: "LRN");
	}

	public void TestLineMRN()
	{
		const string testMrn = "TST_MRN";
		const string expectedMrn = $"<tr><th>MRN</th><td>{testMrn}</td></tr>";
		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		var interpreter = new CC599CMessageInterpreter(entryHeader);
		AssertLineExists(interpreter, dataProviderMock.Object, expectedMrn, description: "MRN");
	}

	public void TestExitStoppedDate()
	{
		var testExitStoppedDate = new DateTime(2024, 01, 01);
		var testExitDate = new DateTime(2024, 01, 29);
		var testCustomsOffice = "A1234";
		CombineAssertions(() =>
		{
			var interpreter = new CC599CMessageInterpreter(entryHeader);
			AssertLineExists(interpreter, dataProviderMock.Object, "<p>Declaration has exited the EU on  according to customs office .</p>", description: "Empty data present");

			dataProviderMock.Setup(m => m.CustomsOfficeOfExitActualReferenceNumber).Returns(testCustomsOffice);
			AssertLineExists(interpreter, dataProviderMock.Object, $"<p>Declaration has exited the EU on  according to customs office {testCustomsOffice}.</p>", description: "Empty ExitDate");

			exitControlResult.Setup(m => m.ExitDate).Returns(testExitDate);
			AssertLineExists(interpreter, dataProviderMock.Object, $"<p>Declaration has exited the EU on {testExitDate} according to customs office {testCustomsOffice}.</p>", description: "Empty ExitStoppedDate");

			exitControlResult.Setup(m => m.ExitStoppedDate).Returns(testExitStoppedDate);
			AssertLineExists(interpreter, dataProviderMock.Object, $"<p>Declaration has not exited the EU. The exit was stopped on {testExitStoppedDate} according to customs office {testCustomsOffice}.</p>", description: "ExitStoppedDate is not empty");
		});
	}

	public void TestCode()
	{
		var codesToTest = new[] { null, string.Empty, "00", AESExitControlResultCodes.Codes.A1, AESExitControlResultCodes.Codes.A2, AESExitControlResultCodes.Codes.A4 };
		CombineAssertions(() =>
		{
			var interpreter = new CC599CMessageInterpreter(entryHeader);
			foreach (var code in codesToTest)
			{
				exitControlResult.Setup(m => m.Code).Returns(code);
				AssertLineExists(interpreter, dataProviderMock.Object, $"<p>Control result code is {code} {GetExpectedExitControlResultCodeDescription(code)}</p>", description: $"Code : '{code}'");
			}
		});

		string GetExpectedExitControlResultCodeDescription(string code) => code switch
		{
			AESExitControlResultCodes.Codes.A1 => AESExitControlResultCodes.Descriptions.A1,
			AESExitControlResultCodes.Codes.A2 => AESExitControlResultCodes.Descriptions.A2,
			AESExitControlResultCodes.Codes.A4 => AESExitControlResultCodes.Descriptions.A4,
			AESExitControlResultCodes.Codes.B1 => AESExitControlResultCodes.Descriptions.B1,
			_ => string.Empty
		};
	}

	public void TestStateOfSeals()
	{
		CombineAssertions(() =>
		{
			var interpreter = new CC599CMessageInterpreter(entryHeader);
			AssertLineExists(interpreter, dataProviderMock.Object, "<p>State of seals is stateOfSeals Not Ok</p>", description: "StateOfSeals is null");

			exitControlResult.Setup(m => m.StateOfSeals).Returns(0);
			AssertLineExists(interpreter, dataProviderMock.Object, "<p>State of seals is stateOfSeals Not Ok</p>", description: "StateOfSeals is 0");

			exitControlResult.Setup(m => m.StateOfSeals).Returns(1);
			AssertLineExists(interpreter, dataProviderMock.Object, "<p>State of seals is stateOfSeals Ok</p>", description: "StateOfSeals is 1");

			exitControlResult.Setup(m => m.StateOfSeals).Returns(2);
			AssertLineExists(interpreter, dataProviderMock.Object, "<p>State of seals is stateOfSeals Not Ok</p>", description: "StateOfSeals is not 1 or 0");
		});
	}

	public void TestLineStatus()
	{
		const string testStatus = AESEntryStatusList.Codes.MrnAllocated;
		var expectedStatus = $"<p>Status is set to {testStatus}</p>";
		entryHeader.CH_EntryStatus = testStatus;
		var interpreter = new CC599CMessageInterpreter(entryHeader);
		AssertLineExists(interpreter, dataProviderMock.Object, expectedStatus, description: "Entry Status");
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryHeader = Factory.New<CusEntryHeader>();
		exitControlResult = new Mock<IExitControlResult>();
		dataProviderMock.Setup(x => x.ExitControlResult).Returns(exitControlResult.Object);
	}

	CusEntryHeader entryHeader;
	Mock<IExitControlResult> exitControlResult;
}
