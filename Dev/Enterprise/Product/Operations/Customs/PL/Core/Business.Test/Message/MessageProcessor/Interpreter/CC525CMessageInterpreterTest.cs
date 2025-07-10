using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC525CMessageInterpreter))]
sealed class CC525CMessageInterpreterTest : MessageInterpreterTest<ICC525C>
{
	public void TestInterpret()
	{
		const string expectedInterpretation = GlobalHtmlStyle +
"<table>" +
	"<caption><h3>CC525C</h3></caption>" +
	"<tbody>" +
		"<tr><th>MRN</th><td></td></tr>" +
		"<tr><th>Declaration has been released for exit on</th><td>1/01/0001</td></tr>" +
		"<tr><th>Customs Office Of Exit - Actual</th><td></td></tr>" +
		"<tr><th>Storing Flag</th><td>0</td></tr>" +
		"<tr><th>Status is set to</th><td></td></tr>" +
	"</tbody>" +
"</table>";

		var interpreter = new CC525CMessageInterpreter(entryHeader);
		var interpretation = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals(expectedInterpretation, interpretation);
	}

	public void TestDescription()
	{
		const string expectedDescription = "CC525C";
		var interpreter = new CC525CMessageInterpreter(entryHeader);
		AssertLineExists(interpreter, dataProviderMock.Object, expectedDescription, description: "Description");
	}

	public void TestLineMRN()
	{
		const string testMrn = "TST_MRN";
		const string expectedMrn = $"<tr><th>MRN</th><td>{testMrn}</td></tr>";
		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		var interpreter = new CC525CMessageInterpreter(entryHeader);
		AssertLineExists(interpreter, dataProviderMock.Object, expectedMrn, description: "MRN");
	}

	public void TestLineReleaseDate()
	{
		var testReleaseDate = new DateTime(2000, 11, 22);
		var expectedReleaseDateLine = $"<tr><th>Declaration has been released for exit on</th><td>{testReleaseDate.ToShortDateString()}</td></tr>";
		exportOperationMock.Setup(x => x.ReleaseDate).Returns(testReleaseDate);
		var interpreter = new CC525CMessageInterpreter(entryHeader);
		AssertLineExists(interpreter, dataProviderMock.Object, expectedReleaseDateLine, description: "Message Sent On");
	}

	public void TestLineCustomsOfficeOfExit()
	{
		const string testOffice = "Test Office Of Exit";
		const string expectedOfficeLine = $"<tr><th>Customs Office Of Exit - Actual</th><td>{testOffice}</td></tr>";
		dataProviderMock.Setup(m => m.CustomsOfficeOfExitReferenceNumber).Returns(testOffice);
		var interpreter = new CC525CMessageInterpreter(entryHeader);
		AssertLineExists(interpreter, dataProviderMock.Object, expectedOfficeLine, description: "Customs Office Of Exit");
	}

	public void TestLineStoringFlag()
	{
		const int testStoringFlag = 1;
		var expectedStoringFlag = $"<tr><th>Storing Flag</th><td>{testStoringFlag}</td></tr>";
		exportOperationMock.Setup(m => m.StoringFlag).Returns(1);
		var interpreter = new CC525CMessageInterpreter(entryHeader);
		AssertLineExists(interpreter, dataProviderMock.Object, expectedStoringFlag, description: "Storing Flag");
	}

	public void TestLineStatus()
	{
		const string testStatus = AESEntryStatusList.Codes.MrnAllocated;
		var expectedStatus = $"<tr><th>Status is set to</th><td>{testStatus}</td></tr>";
		entryHeader.CH_EntryStatus = testStatus;
		var interpreter = new CC525CMessageInterpreter(entryHeader);
		AssertLineExists(interpreter, dataProviderMock.Object, expectedStatus, description: "Entry Status");
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryHeader = Factory.New<CusEntryHeader>();
		exportOperationMock = new Mock<ICC525CExportOperation>();
		dataProviderMock.Setup(m => m.ExportOperation).Returns(exportOperationMock.Object);
	}

	CusEntryHeader entryHeader;
	Mock<ICC525CExportOperation> exportOperationMock;
}
