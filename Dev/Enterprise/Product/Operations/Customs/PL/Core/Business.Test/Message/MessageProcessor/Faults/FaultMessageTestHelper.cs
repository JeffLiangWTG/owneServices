using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.PL.MessageContracts.DataProviders;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

public static class FaultMessageTestHelper
{
	public record FaultTestInfo
	{
		const string DefaultExpectedFaultCode = "TestCode";
		const string DefaultExpectedFaultDescription = "TestDesc";

		public FaultTestInfo(string caseName, string testFileName, string expectedFaultName, string expectedFaultCode = null, string expectedFaultDescription = null)
		{
			CaseName = caseName;
			TestFileName = testFileName;
			ExpectedFaultName = expectedFaultName;
			ExpectedFaultCode = expectedFaultCode ?? DefaultExpectedFaultCode;
			ExpectedFaultDescription = expectedFaultDescription ?? DefaultExpectedFaultDescription;
		}

		public string CaseName { get; }
		public string TestFileName { get; }
		public string ExpectedFaultName { get; }
		public string ExpectedFaultCode { get; }
		public string ExpectedFaultDescription { get; }

		public override string ToString() => CaseName;
	}

	public static string GetFaultTextFromFile(ZString filename)
		=> typeof(FaultMessageTestHelper).Assembly.GetTestFile(filename);

	public static ICommonFault GetFaultFromFile(ZString filename)
	{
		var textReader = typeof(FaultMessageTestHelper).Assembly.GetTestFileReader(filename);
		var xmlReader = XmlHelper.CreateReaderAndGotoRootNode(textReader);
		if (SoapHelper.IsSoap(xmlReader))
		{
			xmlReader.MoveToSoapBody();
		}
		var dataProviderFactory = new DataProviderFactory(RecognizableMessages.All);
		return dataProviderFactory.NewOrNull<ICommonFault>(xmlReader);
	}

	public static TEDIMessage CreateTransmitMessage<TEDIMessage>(
		BusinessObjectFactory factory,
		ZString applicationCode,
		ZString messageType,
		ZString messageSubType,
		BusinessObject transmitMessageLinkedObject)
		where TEDIMessage : BaseEDIMessage
	{
		var (transmitInterchange, transmitMessage) = factory.CreateInterchangeAndMessageForTest<EDIInterchange, TEDIMessage>();
		transmitMessage.EM_ApplicationCode = applicationCode;
		transmitMessage.EM_MessageType = messageType;
		transmitMessage.EM_MessageSubType = messageSubType;
		transmitMessage.EM_MessageText = "TEST";
		transmitMessage.EM_IsActive = true;
		transmitMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
		transmitMessage.EM_LinkedObject = transmitMessageLinkedObject;

		transmitInterchange.EI_ApplicationCode = applicationCode;
		transmitInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitInterchange.EI_Status = EDIInterchange.Status.Sent;

		return transmitMessage;
	}

	public static TEDIMessage CreateFaultMessage<TEDIMessage>(TEDIMessage transmitMessage, ZString faultMessageXml)
		where TEDIMessage : BaseEDIMessage
	{
		var (faultInterchange, faultMessage) = transmitMessage.Factory.CreateInterchangeAndMessageForTest<EDIInterchange, TEDIMessage>(
			sessionGuid: transmitMessage.Interchange.EI_SessionGUID);
		faultMessage.EM_MessageType = transmitMessage.EM_MessageType;
		faultMessage.EM_MessageSubType = EDIMessageSubType.Fault;
		faultMessage.EM_ApplicationCode = transmitMessage.EM_ApplicationCode;
		faultMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
		faultMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
		faultMessage.EM_IsActive = true;
		faultMessage.EM_LinkedObject = transmitMessage.EM_LinkedObject;
		faultMessage.EM_MessageText = faultMessageXml;
		faultMessage.EM_IsTestMessage = transmitMessage.EM_IsTestMessage;
		faultMessage.EM_MessageNum = "EM_MessageNum";

		faultInterchange.EI_ApplicationCode = faultMessage.EM_ApplicationCode;
		faultInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		faultInterchange.EI_Status = EDIInterchange.Status.Received;

		return faultMessage;
	}
}
