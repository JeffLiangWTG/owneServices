using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC504CMessageInterpreter))]
sealed class CC504CMessageInterpreterTest : MessageInterpreterTest<ICC504C>
{
	public void TestInterpret_Empty()
	{
		const string expectedInterpretation = GlobalHtmlStyle +
"<h2>CC504C - Export Declaration Amendment Acceptance</h2>" +
"<hr />" +
"<table>" +
	"<tbody>" +
		"<tr><th>Customs Office of Export</th><td></td></tr>" +
		"<tr><th>LRN</th><td></td></tr>" +
		"<tr><th>MRN</th><td></td></tr>" +
		"<tr><th>Declaration amendment date</th><td></td></tr>" +
		"<tr><th>Declaration amendment acceptance date</th><td></td></tr>" +
	"</tbody>" +
"</table>";
		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC504CMessageInterpreter(entryHeader);
		var interpretation = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals(expectedInterpretation, interpretation);
	}

	public void TestInterpret_NotEmpty()
	{
		var dateAndTime = new DateTime(2024, 01, 01, 02, 03, 04).ToString();
		const string mrn = "MRN";
		const string lrn = "LRN";
		const string customsOfficeOfExportReferenceNumber = "CustomsOfficeOfExportReferenceNumber";
		var expectedInterpretation = GlobalHtmlStyle +
"<h2>CC504C - Export Declaration Amendment Acceptance</h2>" +
"<hr />" +
"<table>" +
	"<tbody>" +
		@$"<tr><th>Customs Office of Export</th><td>{customsOfficeOfExportReferenceNumber}</td></tr>" +
		@$"<tr><th>LRN</th><td>{lrn}</td></tr>" +
		@$"<tr><th>MRN</th><td>{mrn}</td></tr>" +
		@$"<tr><th>Declaration amendment date</th><td>{dateAndTime}</td></tr>" +
		@$"<tr><th>Declaration amendment acceptance date</th><td>{dateAndTime}</td></tr>" +
	"</tbody>" +
"</table>";

		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC504CMessageInterpreter(entryHeader);

		var exportOperation = new Mock<ICC504CExportOperation>();
		exportOperation.Setup(x => x.AmendmentAcceptanceDateAndTime).Returns(dateAndTime);
		exportOperation.Setup(x => x.AmendmentDateAndTime).Returns(dateAndTime);

		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);
		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperation.Object);
		dataProviderMock.Setup(x => x.CustomsOfficeOfExportReferenceNumber).Returns(customsOfficeOfExportReferenceNumber);

		var interpretation = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals(expectedInterpretation, interpretation);
	}
}
