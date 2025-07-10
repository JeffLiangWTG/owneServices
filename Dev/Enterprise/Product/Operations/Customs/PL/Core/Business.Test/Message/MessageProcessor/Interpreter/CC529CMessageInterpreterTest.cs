using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC529CMessageInterpreter))]
sealed class CC529CMessageInterpreterTest : MessageInterpreterTest<ICC529C>
{
	public void TestArgument() => AssertNoExceptionThrown("Argument is not null", () => new CC529CMessageInterpreter(entryHeader));

	public void TestInterpret()
	{
		const string correlationIdentifier = "CorrelationIdentifier";
		var preparationDateAndTime = new DateTime(2024, 01, 01);
		const string mrn = "MRN";
		const string lrn = "LRN";
		var releaseDate = new DateTime(2024, 02, 28);
		const string customsOfficeOfExportReferenceNumber = "CustomsOfficeOfExportReferenceNumber";
		var controlResultDate = new DateTime(2024, 01, 29);
		var declarationAcceptanceDate = new DateTime(2024, 02, 28);

		const string expectedInterpretation = GlobalHtmlStyle +
"<h2>CC529C - Release for export</h2>" +
"<hr />" +
"<table>" +
	"<tbody>" +
		$"<tr><th>Linked by message Identification</th><td>{correlationIdentifier}</td></tr>" +
		"<tr><th>Message Sent On</th><td>01-Jan-24 00:00:00</td></tr>" +
		$"<tr><th>LRN</th><td>{lrn}</td></tr>" +
		$"<tr><th>MRN</th><td>{mrn}</td></tr>" +
		"<tr><th>Declaration is released on</th><td>28-Feb-24 00:00:00</td></tr>" +
		$"<tr><th>Customs Office of Export</th><td>{customsOfficeOfExportReferenceNumber}</td></tr>" +
		"<tr><th>Control Result</th><td>29-Jan-24 00:00:00</td></tr>" +
		"<tr><th>Declaration is accepted on</th><td>28-Feb-24 00:00:00</td></tr>" +
	"</tbody>" +
"</table>";

		var interpreter = new CC529CMessageInterpreter(entryHeader);

		var exportOperation = new Mock<ICC529CExportOperation>();
		var customsOfficeOfExport = new Mock<ICustomsOffice>();
		exportOperation.Setup(x => x.ReleaseDate).Returns(releaseDate);
		exportOperation.Setup(x => x.DeclarationAcceptanceDate).Returns(declarationAcceptanceDate);
		customsOfficeOfExport.Setup(x => x.ReferenceNumber).Returns(customsOfficeOfExportReferenceNumber);

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.PreparationDateAndTime).Returns(preparationDateAndTime);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);
		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperation.Object);
		dataProviderMock.Setup(x => x.ControlResultDate).Returns(controlResultDate);
		dataProviderMock.Setup(x => x.CustomsOfficeOfExport).Returns(customsOfficeOfExport.Object);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedInterpretation);
	}

	public void TestDeclarationAcceptanceDate()
	{
		var declarationAcceptanceDate = new DateTime(2024, 02, 28);
		const string declarationAcceptanceDateWithoutValue = "<tr><th>Declaration is accepted on</th><td></td></tr>";
		const string declarationAcceptanceDateWithValue = "<tr><th>Declaration is accepted on</th><td>28-Feb-24 00:00:00</td></tr>";

		var interpreter = new CC529CMessageInterpreter(entryHeader);
		var exportOperation = new Mock<ICC529CExportOperation>();
		exportOperation.Setup(x => x.DeclarationAcceptanceDate).Returns(declarationAcceptanceDate);
		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperation.Object);

		CombineAssertions(() =>
		{
			AssertLineExists(interpreter, dataProviderMock.Object, declarationAcceptanceDateWithValue, isExisted: true, description: "declaration Acceptance Date");

			entryHeader.EntryNumber = "MRNOnly";
			interpreter = new CC529CMessageInterpreter(entryHeader);
			AssertLineExists(interpreter, dataProviderMock.Object, declarationAcceptanceDateWithoutValue, isExisted: false, description: "No declaration Acceptance Date");
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}
	JobDeclaration declaration;
	CusEntryHeader entryHeader;
}
