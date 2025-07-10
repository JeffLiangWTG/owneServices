using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(NPPMessageInterpreter<CusEntryHeader>))]
public class NPPMessageInterpreterTest : MessageInterpreterTest<IConfirmation>
{
	public void TestInterpret()
	{
		const string externalSystemId = "Test ExternalSystemID";
		const string sepaDocumentId = "Test SeapDocumentID";
		const string errorCause = "Test Error Cause";

		dataProviderMock.Setup(x => x.ExternalSystemID).Returns(externalSystemId);
		dataProviderMock.Setup(x => x.ReferenceToExternalSystemID).Returns(sepaDocumentId);
		dataProviderMock.Setup(x => x.ErrorCause).Returns(errorCause);

		var expectedInterpretation = $"""
			<table border="0">
				<tr><th align="left" width="800">Komunikat NPP (Poświadczenie Nieprzedłożenia Dokumentu)</th></tr>
			</table>
			<hr /><br />
			<table border="0">
				<tr><th align="left" width="300">IdentyfikatorPoswiadczenia</th><td align="left" width="500"> : {externalSystemId}</td></tr>
				<tr><th align="left" width="300">idDokumentuSEAP</th><td align="left" width="500"> : {sepaDocumentId}</td></tr>
				<tr><th align="left" width="300">Przyczyna błędu</th></tr>
			</table>
			<table border="1" cellpadding="1" cellspacing="0" class="table">
				<tr><td align="left" width="800">{errorCause}</td></tr>
			</table><br /><hr />
			""".ToSingleLineHtml();

		var interpretation = interpreter.Interpret(dataProviderMock.Object).ToString().ToSingleLineHtml();
		AssertEquals(expectedInterpretation, interpretation);
	}

	public void TestExternalSystemID() => CombineAssertions(() =>
	{
		const string testSystemId = "TestSystemId";
		const string rowCaption = "IdentyfikatorPoswiadczenia";
		dataProviderMock.Setup(x => x.ExternalSystemID).Returns((string)null);
		var interpretation = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals("ExternalSystemID is not defined: row is not added.", false, interpretation.Contains(rowCaption));
		AssertEquals("ExternalSystemID is not defined: value is not added.", false, interpretation.Contains(testSystemId));

		dataProviderMock.Setup(x => x.ExternalSystemID).Returns(testSystemId);
		interpretation = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals("ExternalSystemID is defined: row is added.", true, interpretation.Contains(rowCaption));
		AssertEquals("ExternalSystemID is defined: value is added.", true, interpretation.Contains(testSystemId));
	});

	public void TestSeapDocumentID() => CombineAssertions(() =>
	{
		const string testSeapDocumentId = "TestSeapId";
		const string rowCaption = "idDokumentuSEAP";
		dataProviderMock.Setup(x => x.ReferenceToExternalSystemID).Returns((string)null);
		var interpretation = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals("SeapDocumentID is not defined: row is not added.", false, interpretation.Contains(rowCaption));
		AssertEquals("SeapDocumentID is not defined: value is not added.", false, interpretation.Contains(testSeapDocumentId));

		dataProviderMock.Setup(x => x.ReferenceToExternalSystemID).Returns(testSeapDocumentId);
		interpretation = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals("SeapDocumentID is defined: row is added.", true, interpretation.Contains(rowCaption));
		AssertEquals("SeapDocumentID is defined: value is added.", true, interpretation.Contains(testSeapDocumentId));
	});

	public void TestErrorCause() => CombineAssertions(() =>
	{
		const string testErrorCause = "Test Error Cause";
		const string rowCaption = "Przyczyna błędu";
		dataProviderMock.Setup(x => x.ErrorCause).Returns((string)null);
		var interpretation = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals("ErrorCause is not defined: row is added.", true, interpretation.Contains(rowCaption));
		AssertEquals("ErrorCause is not defined: value is not added.", false, interpretation.Contains(testErrorCause));

		dataProviderMock.Setup(x => x.ErrorCause).Returns(testErrorCause);
		interpretation = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals("ErrorCause is defined: row is added.", true, interpretation.Contains(rowCaption));
		AssertEquals("ErrorCause is defined: value is added.", true, interpretation.Contains(testErrorCause));
	});

	protected override void SetUp()
	{
		base.SetUp();

		interpreter = CreateMessageInterpreter();
	}

	protected virtual IMessageInterpreter<IConfirmation> CreateMessageInterpreter()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		return new NPPMessageInterpreter<CusEntryHeader>(entryHeader);
	}

	IMessageInterpreter<IConfirmation> interpreter;
}
