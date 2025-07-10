using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(BaseNctsMessageProcessor<>))]
public abstract class BaseNctsMessageProcessorTestCase<TMessageProcessor, T> : BaseMessageProcessorTestCase<TMessageProcessor, T>
	where T : class
	where TMessageProcessor : BaseNctsMessageProcessor<T>
{
	public override void TestApplicationCode() => AssertEquals(ApplicationCodeList.Codes.PLCustomsNCTS, MessageProcessor.ApplicationCode);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendNctsAcknowledgements;

	public virtual void TestPreProcessMessage_CorrelationIdentifier()
	{
		const string messageIdentified = MessageNameList.Codes.IE015;
		const string invalidCorrelationIdentifier = "ABC12345";
		var year = "24";
		var fullCorrelationIdentifier = $"{year}{messageIdentified}4345ABC";
		var messageNum = $"{year}{messageIdentified}4345ABC";
		var fullCorrelationIdentifierArrival = $"{year}{messageIdentified}123456";
		var ediCorrelationIdentifierArrival = $"{year}{messageIdentified}123456";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var testEdiMessage = Factory.CreateNCTSMessage(messageText: string.Empty, linkedObject: nctsHeader.MovementHeader);
		testEdiMessage.EM_MessageNum = messageNum;
		testEdiMessage.EM_MessageSubType = "015";

		var nctsHeaderArrival = Factory.New<NctsHeader>();
		nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
		var testEdiMessageArrival = Factory.CreateNCTSMessage(messageText: string.Empty, linkedObject: nctsHeader.MovementHeader);
		testEdiMessageArrival.EM_MessageNum = ediCorrelationIdentifierArrival;
		testEdiMessageArrival.EM_MessageSubType = "015";

		CombineAssertions(() =>
		{
			AssertPreProcessMessageForCorrelationIdentifier("Valid CorrelationIdentifier", fullCorrelationIdentifier,
				expectedStatus: Status.PreProcessedOK, expectedLinkedObject: nctsHeader.MovementHeader);

			testEdiMessage.EM_ApplicationCode = ApplicationCodes.PLCustoms;
			testEdiMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			AssertPreProcessMessageForCorrelationIdentifier("Not Ncts Message", fullCorrelationIdentifier,
				expectedStatus: Status.Failed, expectedLinkedObject: null);

			testEdiMessage.EM_ApplicationCode = ApplicationCodes.PLCustomsNCTS;
			testEdiMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			AssertPreProcessMessageForCorrelationIdentifier("Invalid Receive Transmit", fullCorrelationIdentifier,
				expectedStatus: Status.Failed, expectedLinkedObject: null);

			testEdiMessage.EM_LinkedObject = nctsHeader.MovementHeader;
			AssertPreProcessMessageForCorrelationIdentifier("Invalid CorrelationIdentifier", invalidCorrelationIdentifier,
				expectedStatus: Status.Failed, expectedLinkedObject: null);
			AssertPreProcessMessageForCorrelationIdentifier("Empty CorrelationIdentifier", string.Empty,
				expectedStatus: Status.Failed, expectedLinkedObject: null);

			testEdiMessageArrival.EM_LinkedObject = nctsHeaderArrival;
			AssertPreProcessMessageForCorrelationIdentifier("Valid Arrival CorrelationIdentifier", fullCorrelationIdentifierArrival,
				expectedStatus: Status.PreProcessedOK, expectedLinkedObject: nctsHeaderArrival);
		});

		void AssertPreProcessMessageForCorrelationIdentifier(string description, string correlationIdentifier, string expectedStatus, BusinessObject expectedLinkedObject)
		{
			dataProviderMock.As<ICorrelationProvider>().Setup(m => m.CorrelationIdentifier).Returns(correlationIdentifier);
			AssertPreProcessMessage(description, expectedStatus, expectedLinkedObject);
		}
	}

	protected (NctsHeader NctsHeader, EDIMessage TransmitMessage, EDIMessage InboundMessage) PrepareDataForInboundMessageTest
		(string movementType, string transitMessageNum, string mrn, string lrn, string transmittedMessageSubType = null, string inboundMessageSubType = null)
	{
		const string testJobReference = "MOVEMENT_NUM";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(movementType);
		nctsHeader.CommonMovementHeader.BM_PaperlessInbondNum = lrn;
		nctsHeader.BH_JobReference = testJobReference;

		if (!string.IsNullOrEmpty(mrn))
		{
			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
			cusEntryNumber.CE_EntryNum = mrn;
			cusEntryNumber.Parent = nctsHeader;
		}

		var linkedObject = movementType switch
		{
			NctsMovementType.Codes.Departure => (BusinessObject)nctsHeader.MovementHeader,
			NctsMovementType.Codes.Arrival => nctsHeader,
			_ => throw new NotImplementedException($"{new NctsMovementType().GetDescriptionFromCode(movementType)} is unsupported in {GetType()}.{nameof(ProcessMessageAndTestEmail)}!"),
		};

		var transmittedMessage = Factory.New<EDIMessage>();
		transmittedMessage.EM_ApplicationCode = ApplicationCodes.PLCustomsNCTS;
		transmittedMessage.EM_ReceiveTransmit = Direction.Transmit;
		transmittedMessage.EM_Status = Status.Sent;
		transmittedMessage.EM_MessageNum = transitMessageNum;
		transmittedMessage.EM_LinkedObject = linkedObject;
		transmittedMessage.EM_MessageSubType = transmittedMessageSubType ?? "001";

		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodes.PLCustomsNCTS;
		incomingMessage.EM_ReceiveTransmit = Direction.Receive;
		incomingMessage.EM_Status = Status.PreProcessedOK;
		incomingMessage.EM_LinkedObject = linkedObject;
		incomingMessage.EM_MessageSubType = inboundMessageSubType ?? "001";

		return (nctsHeader, transmittedMessage, incomingMessage);
	}

	protected (string ExpectedEmailAddress, string ExpectedSubject, EDIMessage InboundMessage) CreateTestDataForEmailTest
		(string movementType, string expectedSubjectPrefix, string transmitMessageNum, string mrn, string lrn, bool isFailure)
	{
		const string emailAddress = "abc@abc.com";
		const string testUserCode = "ABC";

		var user = Factory.New<GlbStaff>();
		user.GS_Code = testUserCode;
		user.GS_EmailAddress = emailAddress;

		var (nctsHeader, transmitMessage, inboundMessage) = PrepareDataForInboundMessageTest(movementType, transmitMessageNum, mrn, lrn);
		transmitMessage.EM_SystemCreateUser = testUserCode;

		var status = isFailure ? "(Failure) " : string.Empty;
		var expectedSubject = $"{expectedSubjectPrefix} Response {status}for {nctsHeader.JobNumber}";
		return (emailAddress, expectedSubject, inboundMessage);
	}

	protected void ProcessMessageAndTestEmail(string movementType, string expectedSubjectPrefix, string transmitMessageNum, string mrn, string lrn, bool isFailure = false)
	{
		var (expectedEmailAddress, expectedSubject, inboundMessage) = CreateTestDataForEmailTest(movementType, expectedSubjectPrefix, transmitMessageNum, mrn, lrn, isFailure);

		MessageProcessor.ProcessMessage(inboundMessage);

		var relatedJob = inboundMessage.GetRelatedNctsHeader();
		CombineAssertions(() => AssertEmail(expectedEmailAddress, expectedSubject, expectedBody: inboundMessage.EM_MessageInterpretation, relatedJob));
	}

	public virtual void TestPreProcessMessage_MRN()
	{
		const string testMrn = "7567456A";
		const string testMrnArrival = "12345ABC";
		const string invalidNumber = "ABC12345";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = testMrn;

		var testNctsHeaderArrival = Factory.New<NctsHeader>();
		testNctsHeaderArrival.BH_HeaderType = NctsMovementType.Codes.Arrival;
		testNctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		testNctsHeaderArrival.MovementReferenceEntryNumber.CE_EntryNum = testMrnArrival;

		CombineAssertions(() =>
		{
			AssertPreProcessMessageMRN("Valid MRN", mrn: testMrn, expectedStatus: Status.PreProcessedOK, expectedLinkedObject: testNctsHeader.MovementHeader);
			AssertPreProcessMessageMRN("Valid MRN Arrival", mrn: testMrnArrival, expectedStatus: Status.PreProcessedOK, expectedLinkedObject: testNctsHeaderArrival);
			AssertPreProcessMessageMRN("Invalid MRN", mrn: invalidNumber, expectedStatus: Status.Failed, expectedLinkedObject: null);
		});

		void AssertPreProcessMessageMRN(string description, string mrn, string expectedStatus, BusinessObject expectedLinkedObject)
		{
			dataProviderMock.As<IJobIdentification>().Setup(m => m.MRN).Returns(mrn);
			AssertPreProcessMessage(description, expectedStatus, expectedLinkedObject);
		}
	}

	public virtual void TestPreProcessMessage_LRN()
	{
		const string testLrn = "12345ABC";
		const string testLrnArrival = "POI1234";
		const string invalidNumber = "ABC12345";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		testNctsHeader.MovementHeader.BM_PaperlessInbondNum = testLrn;

		var testNctsHeaderArrival = Factory.New<NctsHeader>();
		testNctsHeaderArrival.BH_HeaderType = NctsMovementType.Codes.Arrival;
		testNctsHeaderArrival.ArrivalMovementHeader.BM_PaperlessInbondNum = testLrnArrival;
		testNctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		CombineAssertions(() =>
		{
			AssertPreProcessMessageLRN("Valid LRN", lrn: testLrn, expectedStatus: Status.PreProcessedOK, expectedLinkedObject: testNctsHeader.MovementHeader);
			AssertPreProcessMessageLRN("Valid LRN Arrival", lrn: testLrnArrival, expectedStatus: Status.PreProcessedOK, expectedLinkedObject: testNctsHeaderArrival);
			AssertPreProcessMessageLRN("Invalid LRN", lrn: invalidNumber, expectedStatus: Status.Failed, expectedLinkedObject: null);
		});

		void AssertPreProcessMessageLRN(string description, string lrn, string expectedStatus, BusinessObject expectedLinkedObject)
		{
			dataProviderMock.As<IJobIdentification>().Setup(m => m.LRN).Returns(lrn);
			AssertPreProcessMessage(description, expectedStatus, expectedLinkedObject);
		}
	}

	void AssertPreProcessMessage(string description, string expectedStatus, BusinessObject expectedLinkedObject)
	{
		var message = Factory.New<EDIMessage>();
		MessageProcessor.PreProcessMessage(message);
		AssertEquals($"{description}: EM_LinkedObject", expectedLinkedObject, message.EM_LinkedObject);
		AssertEquals($"{description}: EM_Status", expectedStatus, message.EM_Status);
	}
}

sealed class BaseNctsMessageProcessorTest : BaseNctsMessageProcessorTestCase<BaseNctsMessageProcessorForTest, IIncomingIE>
{
	protected override string ExpectedMessageFriendlyName => "Test PL NCTS Message Processor";

	protected override bool ExpectedIsFailureNotification => false;

	public override void TestPreProcessMessage_LRN() => Assert("Tested by TestPreProcessMessage_CorrelationIdentifier", true);

	public override void TestPreProcessMessage_MRN() => Assert("Tested by TestPreProcessMessage_CorrelationIdentifier", true);

	public void TestDocumentsAddedToDepartureNctsHeaderJob()
	{
		const string correlationIdentifier = "24IE01500000001";

		var (nctsHeader, _, inboundMessage) = PrepareDataForInboundMessageTest(NctsMovementType.Codes.Departure, correlationIdentifier, "TEST_MRN", lrn: null, transmittedMessageSubType: "015");

		inboundMessage.DocManagerInfo.AddFileOrDocument([64, 128, 62], "Document.pdf", Core.Constants.RefDocTypes.TransitAccompanyingDocument);
		inboundMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;

		dataProviderMock.Setup(p => p.CorrelationIdentifier).Returns(correlationIdentifier);

		var messageProcessor = messageProcessorMock.Object;
		messageProcessor.ProcessMessage(inboundMessage);

		Factory.Save();

		AssertDocumentsAdded(nctsHeader, ("Document.pdf", "TAD", 3));
	}

	public void TestDocumentsAddedToArrivalNctsHeaderJob()
	{
		const string correlationIdentifier = "24IE00700000001";

		var (nctsHeader, _, inboundMessage) = PrepareDataForInboundMessageTest(NctsMovementType.Codes.Arrival, correlationIdentifier, "TEST_MRN", lrn: null, transmittedMessageSubType: "007");

		inboundMessage.DocManagerInfo.AddFileOrDocument([64, 128, 62], "Document.pdf", Core.Constants.RefDocTypes.TransitAccompanyingDocument);
		inboundMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;

		dataProviderMock.Setup(p => p.CorrelationIdentifier).Returns(correlationIdentifier);

		var messageProcessor = messageProcessorMock.Object;
		messageProcessor.ProcessMessage(inboundMessage);

		Factory.Save();

		AssertDocumentsAdded(nctsHeader, ("Document.pdf", "TAD", 3));
	}
}

public class BaseNctsMessageProcessorForTest : BaseNctsMessageProcessor<IIncomingIE>
{
	public BaseNctsMessageProcessorForTest(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => "Test PL NCTS Message Processor";

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIncomingIE messageDataProvider) => ProcessingResult.Succeed;
}
