using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;
using EDIMessageStatus = Enterprise.Messaging.Business.EDIMessage.Status;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC525CMessageProcessor))]
sealed class CC525CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC525CMessageProcessor, ICC525C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC525;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC525CMessageInterpreter);

	public void TestProcessMessage_Discarded()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessageStatus.PreProcessedOK;
		var testEntryHeader = Factory.New<CusEntryHeader>();
		testEntryHeader.CH_EntryStatus = AESEntryStatusList.Codes.ReleasedForExit;
		message.EM_LinkedObject = testEntryHeader;
		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;

		MessageProcessor.ProcessMessage(message);

		const string expectedMessageStatus = EDIMessageStatus.Discarded;
		var expectedNoteText = $"The message with interchange {interchange.EI_InterchangeNum} is discarded, because the present status is not MRN.";
		CombineAssertions(() =>
		{
			AssertEquals("If entry header is already released for exit", expectedMessageStatus, message.EM_Status);
			message.AssertHasExactLogMessage("Discarded message note text:", Events.ErrorReport, expectedNoteText);
		});
	}

	public void TestProcessMessage()
	{
		var testDateTime = new DateTime(2000, 10, 30);
		exportOperationMock.Setup(x => x.ReleaseDate).Returns(testDateTime);
		var testEntryHeader = Factory.New<CusEntryHeader>();
		testEntryHeader.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessageStatus.PreProcessedOK;
		message.EM_LinkedObject = testEntryHeader;

		MessageProcessor.ProcessMessage(message);

		const string expectedMessageStatus = EDIMessageStatus.ProcessedOK;
		const string expectedEntryStatus = AESEntryStatusList.Codes.ReleasedForExit;
		var expectedEntryReleaseDate = new ZDateTime(testDateTime);
		var expectedInterpretation =
"<style>" +
	"table, th, td { " +
		"border: 1px solid black; " +
		"border-collapse: collapse; " +
	"} " +
	"th, td { " +
		"padding: 5px; " +
		"text-align: left; " +
	"}" +
"</style>" +
"<table>" +
	"<caption><h3>CC525C</h3></caption>" +
	"<tbody>" +
		"<tr><th>MRN</th><td></td></tr>" +
		$"<tr><th>Declaration has been released for exit on</th><td>{testDateTime.ToShortDateString()}</td></tr>" +
		"<tr><th>Customs Office Of Exit - Actual</th><td></td></tr>" +
		"<tr><th>Storing Flag</th><td>0</td></tr>" +
		$"<tr><th>Status is set to</th><td>{expectedEntryStatus}</td></tr>" +
	"</tbody>" +
"</table>";
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", expectedMessageStatus, message.EM_Status);
			AssertEquals("CH_EntryStatus", expectedEntryStatus, testEntryHeader.CH_EntryStatus);
			AssertEquals("CH_EntryReleaseDate", expectedEntryReleaseDate, testEntryHeader.CH_EntryReleaseDate);
			AssertEquals("EM_MessageInterpretation", expectedInterpretation, message.EM_MessageInterpretation);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		exportOperationMock = new Mock<ICC525CExportOperation>();
		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperationMock.Object);
	}

	Mock<ICC525CExportOperation> exportOperationMock;
}
