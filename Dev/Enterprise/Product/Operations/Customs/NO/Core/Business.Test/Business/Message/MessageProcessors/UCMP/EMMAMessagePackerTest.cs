using System;
using System.Text;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EMMAMessagePacker))]
sealed class EMMAMessagePackerTest : UniversalCustomsEDIMessagePackerTest<EMMAMessagePacker>
{
	public void TestAllowEmptyMessageBody() => AssertEquals(expected: false, MessagePacker.AllowEmptyMessageBody);

	public void TestPack_ShouldThrowOnInvalidParameters() => CombineAssertions(() =>
	{
		_ = AssertArgumentExceptionThrown<ArgumentNullException>("When message is null", nameof(message), () => MessagePacker.Pack(null, interchange, logger));
		_ = AssertArgumentExceptionThrown<ArgumentNullException>("When interchange is null", nameof(interchange), () => MessagePacker.Pack(message, null, logger));
		_ = AssertArgumentExceptionThrown<ArgumentNullException>("When logger is null", nameof(logger), () => MessagePacker.Pack(message, interchange, null));

		AssertNoExceptionThrown("Happy path", () => MessagePacker.Pack(message, interchange, logger));
	});

	public void TestPack_ForInvalidLinkedObject() => CombineAssertions(() =>
	{
		const string expectedErrorMessage = "Linked object should be a CusEntryHeader with Declaration";

		message.EM_LinkedObject = null;
		AssertEquals("When EM_LinkedObject is null", expectedErrorMessage, MessagePacker.Pack(message, interchange, logger));

		message.EM_LinkedObject = declaration;
		AssertEquals("When EM_LinkedObject is JobDeclaration", expectedErrorMessage, MessagePacker.Pack(message, interchange, logger));

		message.EM_LinkedObject = Factory.New<CusEntryHeader>();
		AssertEquals("When EM_LinkedObject is standalone CusEntryHeader", expectedErrorMessage, MessagePacker.Pack(message, interchange, logger));
	});

	public void TestPack_Logging_WhenLinkedObjectIsNull()
	{
		message.EM_LinkedObject = null;
		var logs = logger.CaptureLogMessages(() => MessagePacker.Pack(message, interchange, logger));
		AssertContainsExactElementsInExactOrder([
			(LogType.Information, $"Started NO EMMA Message Packer for EDIMessage with PK: [{message.PK}]"),
			(LogType.Error, $"NO EMMA Message Packer Linked object is not a type of CusEntryHeader for EDIMessage with PK: [{message.PK}]"),
			(LogType.Information, $"Finished NO EMMA Message Packer for EDIMessage with PK: [{message.PK}]"),
		], logs);
	}

	public void TestPack_Logging_WhenLinkedObjectIsNotCusEntryHeader()
	{
		message.EM_LinkedObject = declaration;
		var logs = logger.CaptureLogMessages(() => MessagePacker.Pack(message, interchange, logger));
		AssertContainsExactElementsInExactOrder([
			(LogType.Information, $"Started NO EMMA Message Packer for EDIMessage with PK: [{message.PK}]"),
			(LogType.Error, $"NO EMMA Message Packer Linked object is not a type of CusEntryHeader for EDIMessage with PK: [{message.PK}]"),
			(LogType.Information, $"Finished NO EMMA Message Packer for EDIMessage with PK: [{message.PK}]"),
		], logs);
	}

	public void TestPack_Logging_WhenLinkedObjectIsStandaloneCusEntryHeader()
	{
		var standaloneEntryHeader = Factory.New<CusEntryHeader>();
		message.EM_LinkedObject = standaloneEntryHeader;
		var logs = logger.CaptureLogMessages(() => MessagePacker.Pack(message, interchange, logger));
		AssertContainsExactElementsInExactOrder([
			(LogType.Information, $"Started NO EMMA Message Packer for EDIMessage with PK: [{message.PK}]"),
			(LogType.Error, $"NO EMMA Message Packer Linked CusEntryHeader: [{standaloneEntryHeader.PK}] is not linked to a JobDeclaration for EDIMessage with PK: [{message.PK}]"),
			(LogType.Information, $"Finished NO EMMA Message Packer for EDIMessage with PK: [{message.PK}]"),
		], logs);
	}

	public void TestPack_Logging_WhenDeclarantCodeIsEmpty()
	{
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		var logs = logger.CaptureLogMessages(() => MessagePacker.Pack(message, interchange, logger));
		AssertContainsExactElementsInExactOrder([
			(LogType.Information, $"Started NO EMMA Message Packer for EDIMessage with PK: [{message.PK}]"),
			(LogType.Error, $"NO EMMA Message Packer found empty DeclarantCode for EDIMessage with PK: [{message.PK}]"),
			(LogType.Information, $"Finished NO EMMA Message Packer for EDIMessage with PK: [{message.PK}]"),
		], logs);
	}

	public void TestPack_Logging_WhenHappyPath() => CombineAssertions(() =>
	{
		entryHeader.SetEntryReleaseNumber("44556677");
		var arrangeEDocs = ArrangeEDocs();
		var logs = logger.CaptureLogMessages(() => MessagePacker.Pack(message, interchange, logger));
		AssertContainsExactElementsInExactOrder([
			(LogType.Information, $"Started NO EMMA Message Packer for EDIMessage with PK: [{message.PK}]"),
			(LogType.Debug, $"NO EMMA Message Packer adding eDoc attachment with FileName: [{arrangeEDocs[0].DocType}-44556677-{arrangeEDocs[0].FileName}] to EDIMessage with PK: [{message.PK}]"),
			(LogType.Debug, $"NO EMMA Message Packer adding eDoc attachment with FileName: [{arrangeEDocs[1].DocType}-44556677-{arrangeEDocs[1].FileName}] to EDIMessage with PK: [{message.PK}]"),
			(LogType.Debug, $"NO EMMA Message Packer adding eDoc attachment with FileName: [{arrangeEDocs[2].DocType}-44556677-{arrangeEDocs[2].FileName}] to EDIMessage with PK: [{message.PK}]"),
			(LogType.Debug, $"NO EMMA Message Packer adding headers to EDIInterchange with PK: [{interchange.PK}]"),
			(LogType.Debug, $"NO EMMA Message Packer adding attributes to EDIInterchange with PK: [{interchange.PK}]"),
			(LogType.Debug, $"NO EMMA Message Packer adding body to EDIInterchange with PK: [{interchange.PK}]"),
			(LogType.Information, $"NO EMMA Message Packer successfully for EDIMessage with PK: [{message.PK}]"),
			(LogType.Information, $"Finished NO EMMA Message Packer for EDIMessage with PK: [{message.PK}]"),
		], logs);
	});

	public void TestPack_ShouldAttachInvoiceEDocsAndMostRecentSadhEDoc() => CombineAssertions(() =>
	{
		var expectedEDocs = ArrangeEDocs();
		var errors = MessagePacker.Pack(message, interchange, logger);
		AssertEquals("Returned errors", ZString.Empty, errors);
		const string expectedHeader = """{"custom.NO.AttachmentFilenames":"EPR-sadh-file-new.pdf, INV-inv-file-1.txt, INV-inv-file-2.txt","custom.NO.XmlFileName":"Decl.xml"}""";
		AssertEquals("EI_HeaderText", expectedHeader, interchange.EI_HeaderText);
		AssertContainsExactElementsInAnyOrder("MessageAttachments", expectedEDocs, message.MessageAttachments.Select(x => x.GetAttachment()));
	});

	public void TestPack_ForNewMessage()
	{
		var result = MessagePacker.Pack(message, interchange, logger);

		const string expectedInterchangeText =
			"{\"custom.NO.XmlFileName\":\"Decl.xml\"}" +
			"MIME-Version: 1.0\n" +
			"Content-Type: Multipart/Related; boundary=\"MIME_boundary\"\n" +
			"\n" +
			"--MIME_boundary\n" +
			"Content-Type: Application/xml\n" +
			"Content-ID: <xml_manifest>\n" +
			"Content-Disposition: inline; filename=Decl.xml\n" +
			"\n" +
			"<<MSGNO PLACEHOLDER>>\n" +
			"\n" +
			"--MIME_boundary--\n";

		CombineAssertions(() =>
		{
			AssertNullOrEmpty("Output String", result);
			AssertEquals(
				"EI_InterchangeText",
				NormalizeLineEndings(expectedInterchangeText),
				NormalizeLineEndings(interchange.EI_InterchangeText.ToString())
			);
		});
	}

	static string NormalizeLineEndings(string text) => text?.Replace("\r\n", "\n").Replace("\r", "\n");

	public void TestPack_InterchangeText_withAttachments()
	{
		string xml = @"<?xml version=""1.0"" encoding=""iso-8859-1"" standalone=""yes""?>
<EmmaSystemsFortolling>
    <Fortolling>
        <Tollnummer>0175402500000183</Tollnummer>
        <MRN>0175402500000183</MRN>
    </Fortolling>
</EmmaSystemsFortolling>";

		message.EM_MessageText = xml;

		var arrangedEDocs = ArrangeEDocs();
		var errors = MessagePacker.Pack(message, interchange, logger);
		AssertEquals("Returned errors", ZString.Empty, errors);
		const string expectedHeader = """{"custom.NO.AttachmentFilenames":"EPR-sadh-file-new.pdf, INV-inv-file-1.txt, INV-inv-file-2.txt","custom.NO.XmlFileName":"Decl.xml"}""";
		AssertEquals("EI_HeaderText", expectedHeader, interchange.EI_HeaderText);

		var id1 = arrangedEDocs[0].UniqueKey.ToString();
		var id2 = arrangedEDocs[1].UniqueKey.ToString();
		var id3 = arrangedEDocs[2].UniqueKey.ToString();

		var expectedInterchangeText =
			"{\"custom.NO.AttachmentFilenames\":\"EPR-sadh-file-new.pdf, INV-inv-file-1.txt, INV-inv-file-2.txt\",\"custom.NO.XmlFileName\":\"Decl.xml\"}" +
			"MIME-Version: 1.0\n" +
			"Content-Type: Multipart/Related; boundary=\"MIME_boundary\"\n" +
			"\n" +
			"--MIME_boundary\n" +
			"Content-Type: Application/xml\n" +
			"Content-ID: <xml_manifest>\n" +
			"Content-Disposition: inline; filename=Decl.xml\n" +
			"\n" +
			"<?xml version=\"1.0\" encoding=\"iso-8859-1\" standalone=\"yes\"?>\n" +
			"<EmmaSystemsFortolling>\n" +
			"    <Fortolling>\n" +
			"        <Tollnummer>0175402500000183</Tollnummer>\n" +
			"        <MRN>0175402500000183</MRN>\n" +
			"    </Fortolling>\n" +
			"</EmmaSystemsFortolling>\n" +
			"\n" +
			"--MIME_boundary\n" +
			"Content-Type: application/pdf\n" +
			$"Content-ID: <{id1}>\n" +
			"Content-Disposition: attachment; filename=EPR-sadh-file-new.pdf \n" +
			"Content-Transfer-Encoding: base64\n" +
			"\n" +
			"c3R1YiBTQURIIE5PIGRvY3VtZW50IDI=\n" +
			"\n" +
			"--MIME_boundary\n" +
			"Content-Type: application/pdf\n" +
			$"Content-ID: <{id2}>\n" +
			"Content-Disposition: attachment; filename=INV-inv-file-1.txt \n" +
			"Content-Transfer-Encoding: base64\n" +
			"\n" +
			"c3R1YiBJbnZvaWNlIGRvY3VtZW50IDE=\n" +
			"\n" +
			"--MIME_boundary\n" +
			"Content-Type: application/pdf\n" +
			$"Content-ID: <{id3}>\n" +
			"Content-Disposition: attachment; filename=INV-inv-file-2.txt \n" +
			"Content-Transfer-Encoding: base64\n" +
			"\n" +
			"c3R1YiBJbnZvaWNlIGRvY3VtZW50IDI=\n" +
			"\n" +
			"--MIME_boundary--\n";

		AssertEquals(
			"EI_InterchangeText",
			NormalizeLineEndings(expectedInterchangeText),
			NormalizeLineEndings(interchange.EI_InterchangeText.ToString())
			);
	}

	public void TestPack_ShouldAddInterchangeHeaders() => CombineAssertions(() =>
	{
		var result = MessagePacker.Pack(message, interchange, logger);
		AssertNullOrEmpty("Error String", result);
		AssertEquals("EI_ApplicationCode", message.EM_ApplicationCode, interchange.EI_ApplicationCode);
		AssertEquals("EI_InterchangeType", message.EM_MessageType, interchange.EI_InterchangeType);
		AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
		AssertEquals("EI_To", "NOCEmma", interchange.EI_To);
		AssertEquals("EI_GB", message.EM_GB, interchange.EI_GB);
		AssertEquals("EI_GP", message.EM_GP, interchange.EI_GP);
		AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
		AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
		AssertEquals("EI_IsActive", expected: true, interchange.EI_IsActive);
		AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
		AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
	});

	IeDoc[] ArrangeEDocs()
	{
		var docManager = entryHeader.DocManagerInfo();
		_ = docManager.AddFileOrDocument(Encoding.ASCII.GetBytes("stub SADH NO document 1"), "sadh-file-old.pdf", Core.Constants.RefDocTypes.EntryPrint);
		var sadh = docManager.AddFileOrDocument(Encoding.ASCII.GetBytes("stub SADH NO document 2"), "sadh-file-new.pdf", Core.Constants.RefDocTypes.EntryPrint);

		var declarationDocumentManager = declaration.DocManagerInfo();
		var inv1 = declarationDocumentManager.AddFileOrDocument(Encoding.ASCII.GetBytes("stub Invoice document 1"), "inv-file-1.txt", Core.Constants.RefDocTypes.Invoice);
		var inv2 = declarationDocumentManager.AddFileOrDocument(Encoding.ASCII.GetBytes("stub Invoice document 2"), "inv-file-2.txt", Core.Constants.RefDocTypes.Invoice);
		_ = declarationDocumentManager.AddFileOrDocument(Encoding.ASCII.GetBytes("stub Misc document"), "misc-file.png", Core.Constants.RefDocTypes.MiscellaneousDocument);
		return [sadh, inv1, inv2];
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		message = Factory.NewWithValidTestData<EDIMessage>();
		message.EM_ApplicationCode = "NOE";
		message.EM_MessageType = "EMM";
		interchange = Factory.New<EDIInterchange>();
		logger = new ();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();

		declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "Decl";
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

		message.EM_LinkedObject = entryHeader;
	}

	EDIMessage message;
	EDIInterchange interchange;
	LoggingInformation logger;

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	OrgHeader declarant;

	IUniversalCustomsEDIMessagePacker MessagePacker => messagePacker ??= new EMMAMessagePacker();
	IUniversalCustomsEDIMessagePacker messagePacker;

	protected override string ApplicationCode => ApplicationCodeList.Codes.NOCustomsEmma;
}
