using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.PL.Business.Constants;
using static Enterprise.Customs.PL.Business.Testing.FaultMessageTestHelper;
using static Enterprise.Messaging.Integration.EDIMessageStatusList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

public abstract class FaultMessageProcessorTestBase<TFaultMessageProcessor, TEDIMessage> : BaseMessageProcessorTestCase<TFaultMessageProcessor, ICommonFault>
	where TFaultMessageProcessor : FaultMessageProcessor
	where TEDIMessage : BaseEDIMessage
{
	protected override bool ExpectedIsFailureNotification => true;

	protected FaultTestInfo[] FaultsForTest { get; } = [
		new (caseName: "Universal Event Rejection",	expectedFaultName: "Universal Event Rejection",	testFileName: "Enterprise.Customs.PL.Business.Testing.Message.TestFiles.UniversalEventRejection.xml", expectedFaultCode: "Universal Event Rejection", expectedFaultDescription: "Error Message  Contract: xt-contract:/Customs/PL/PL Contract  Reference Object: xt-httpclientaddress:{0a725522-7303-473f-8963-cf3b612c6d81}  Reference Object: xt-node:{8fbf34ed-88b2-4929-aa57-fdc9fabecf39}    Error description: Message transmission to https://te-ws.puesc.gov.pl/seap_wsChannel/DocumentHandlingPort rejected by peer: xT certificate validation failed  Reference Object: xt-httpclientaddress:{0a725522-7303-473f-8963-cf3b612c6d81}  Reference Object: xt-node:{8fbf34ed-88b2-4929-aa57-fdc9fabecf39}    Error description: Message transmission to https://te-ws.puesc.gov.pl/seap_wsChannel/DocumentHandlingPort rejected by peer: xT certificate validation failed  Reference Object: xt-httpclientaddress:{0a725522-7303-473f-8963-cf3b612c6d81}  Reference Object: xt-node:{8fbf34ed-88b2-4929-aa57-fdc9fabecf39}    Error description: Message transmission to https://te-ws.puesc.gov.pl/seap_wsChannel/DocumentHandlingPort rejected by peer: xT certificate validation failed  Reference Object: xt-httpclientaddress:{0a725522-7303-473f-8963-cf3b612c6d81}  Reference Object: xt-node:{8fbf34ed-88b2-4929-aa57-fdc9fabecf39}    Error description: Message transmission to https://te-ws.puesc.gov.pl/seap_wsChannel/DocumentHandlingPort rejected by peer: xT certificate validation failed  Reference Object: xt-httpclientaddress:{0a725522-7303-473f-8963-cf3b612c6d81}  Reference Object: xt-node:{8fbf34ed-88b2-4929-aa57-fdc9fabecf39}    Error description: Message transmission to https://te-ws.puesc.gov.pl/seap_wsChannel/DocumentHandlingPort rejected by peer: xT certificate validation failed  Reference Object: xt-httpclientaddress:{0a725522-7303-473f-8963-cf3b612c6d81}  Reference Object: xt-node:{8fbf34ed-88b2-4929-aa57-fdc9fabecf39}    Error description: Message transmission to https://te-ws.puesc.gov.pl/seap_wsChannel/DocumentHandlingPort rejected by peer: xT certificate validation failed  "),
		new (caseName: "Soap 1.1 fault",	expectedFaultName: "SOAP fault",	testFileName: "Enterprise.Customs.PL.Business.Testing.Message.TestFiles.SoapFault1.1.xml"),
		new (caseName: "Soap 1.2 fault",	expectedFaultName: "SOAP fault",	testFileName: "Enterprise.Customs.PL.Business.Testing.Message.TestFiles.SoapFault1.2.xml"),
		new (caseName: "BusinessError",		expectedFaultName: "BusinessError", testFileName: "Enterprise.Customs.PL.Business.Testing.Message.InterchangeUnpacking.DocumentHandlingPort.TestFiles.BusinessErrorFault.xml"),
		new (caseName: "TechError",			expectedFaultName: "TechError",		testFileName: "Enterprise.Customs.PL.Business.Testing.Message.InterchangeUnpacking.DocumentHandlingPort.TestFiles.TechErrorFault.xml"),
	];

	public void TestDeserializeFaultMessage() => CombineAssertions(() =>
	{
		FaultsForTest.ForEach(AssertDeserializeFaultMessage);

		void AssertDeserializeFaultMessage(FaultTestInfo faultTestInfo)
		{
			var fault = GetFaultFromFile(faultTestInfo.TestFileName);

			AssertNotNull($"{faultTestInfo}: Fault must be read from soap!", fault);
			AssertEquals($"{faultTestInfo}: FaultName", faultTestInfo.ExpectedFaultName, fault.FaultName);
			AssertEquals($"{faultTestInfo}: ErrorCode", faultTestInfo.ExpectedFaultCode, fault.ErrorCode);
			AssertEquals($"{faultTestInfo}: ErrorDescription", faultTestInfo.ExpectedFaultDescription, fault.Description);
		}
	});

	protected (bool Processed, TEDIMessage TransmitMessage, TEDIMessage FaultMessage) AssertCommonProcessingResults(
		FaultTestInfo faultTestInfo,
		ZString applicationCode,
		ZString messageType,
		ZString messageSubType,
		BusinessObject transmitMessageLinkedObject,
		bool connectObjectInsteadOfTransmitMessage = false)
	{
		var transmitMessage = CreateTransmitMessage<TEDIMessage>(Factory, applicationCode, messageType, messageSubType, transmitMessageLinkedObject);
		var faultMessageText = GetFaultTextFromFile(faultTestInfo.TestFileName);
		var faultMessage = CreateFaultMessage(transmitMessage, faultMessageText);
		if (connectObjectInsteadOfTransmitMessage)
		{
			faultMessage.EM_LinkedObject = transmitMessage.EM_LinkedObject;
		}
		var result = (Processed: false, transmitMessage, faultMessage);

		MessageProcessor.PreProcessMessage(faultMessage);
		AssertEquals($"{faultTestInfo}: Fault message status must be {PreProcessedOK}", PreProcessedOK, faultMessage.EM_Status);
		if (faultMessage.EM_Status != PreProcessedOK)
		{
			return result;
		}

		MessageProcessor.ProcessMessage(faultMessage);
		AssertEquals($"{faultTestInfo}: Fault message status must be {ProcessedOK}", ProcessedOK, faultMessage.EM_Status);
		if (faultMessage.EM_Status != ProcessedOK)
		{
			return result;
		}

		if (!connectObjectInsteadOfTransmitMessage)
		{
			AssertEquals($"{faultTestInfo}: Validating message status", Failed, transmitMessage.EM_Status);

			const string transmitMessageStatusUpdatedNote = "Updated corresponding transmit message status to [FAL].";
			transmitMessage.AssertHasLogMessagePart($"{faultTestInfo}: Transmit message has updated", Events.MessageRejected, transmitMessageStatusUpdatedNote);
			faultMessage.AssertHasLogMessagePart($"{faultTestInfo}: Fault message has updated", Events.MessageRejected, transmitMessageStatusUpdatedNote);
		}

		AssertEquals($"{faultTestInfo}: Fault message linked object", transmitMessageLinkedObject, faultMessage.EM_LinkedObject);

		return (Processed: true, transmitMessage, faultMessage);
	}

	protected override string ExpectedMessageFriendlyName => EDIMessageSubType.Fault;

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendExportMessageErrors;
}
