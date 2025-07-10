using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Integration;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(NPPMessageProcessor))]
sealed class NPPMessageProcessorTest : BaseMessageProcessorTestCase<NPPMessageProcessor, IConfirmation>
{
	protected override string ExpectedMessageFriendlyName => $"{ApplicationCodes.PLCustoms}/{PUESC.SystemMessages.NPP}";

	protected override bool ExpectedIsFailureNotification => true;

	protected override Type ExpectedMessageInterpreterType => typeof(NPPMessageInterpreter<CusEntryHeader>);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendExportMessageErrors;

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
			AssertEquals("Entry Header EM_Status", expected: "ERR", testCusEntryHeader.CH_EntryStatus);
		});
	}
}
