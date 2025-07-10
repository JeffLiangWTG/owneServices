using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(UPOMessageProcessor))]
sealed class UPOMessageProcessorTest : ImpExpMessageProcessorBaseTest<UPOMessageProcessor, IUpo>
{
	protected override string ExpectedMessageFriendlyName => $"{ApplicationCodes.PLCustoms}/{PUESC.SystemMessages.UPO}";

	protected override Type ExpectedMessageInterpreterType => typeof(UPOMessageInterpreter);

	protected override bool ExpectedIsFailureNotification => false;

	public void TestProcessMessage()
	{
		var testCusEntryHeader = Factory.New<CusEntryHeader>();
		var testUpoMessage = Factory.New<EDIMessage>();
		testUpoMessage.EM_LinkedObject = testCusEntryHeader;
		testUpoMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
		testCusEntryHeader.CH_EntryStatus = LogicalStatusList.Codes.Sent;
		MessageProcessor.ProcessMessage(testUpoMessage);

		CombineAssertions(() => {
			AssertEquals("Message EM_Status", expected: "PRS", testUpoMessage.EM_Status);
			AssertEquals("Entry Header EM_Status", expected: "ACC", testCusEntryHeader.CH_EntryStatus);
		});
	}

	public void TestProcessMessage_NotSent()
	{
		var testCusEntryHeader = Factory.New<CusEntryHeader>();
		var testUpoMessage = Factory.New<EDIMessage>();
		testUpoMessage.EM_LinkedObject = testCusEntryHeader;
		testUpoMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
		testCusEntryHeader.CH_EntryStatus = LogicalStatusList.Codes.Failed;
		MessageProcessor.ProcessMessage(testUpoMessage);

		CombineAssertions(() => {
			AssertEquals("Message EM_Status", expected: "PRS", testUpoMessage.EM_Status);
			AssertEquals("Entry Header EM_Status", expected: LogicalStatusList.Codes.Failed, testCusEntryHeader.CH_EntryStatus);
		});
	}

	public void TestEmail()
	{
		const string correlationIdentifier = "cor123";
		const string transmitDocumentType = "DocType";
		const string expectedSubject = "UPO - Official Confirmation of Receipt (DocType) cor123. Response";

		dataProviderMock.As<ICorrelationProvider>().Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.TransmitDocumentType).Returns(transmitDocumentType);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: null);
	}
}
