using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.PL.Business.Testing;

abstract class ImpExpMessageProcessorBaseTest<TMessageProcessor, TDataProvider> : BaseMessageProcessorTestCase<TMessageProcessor, TDataProvider>
	where TDataProvider : class, IIncomingMessage
	where TMessageProcessor : BaseMessageProcessor<TDataProvider>
{
	protected const string NoteDescription = "Processing Log";

	public void TestPreProcessMessage()
	{
		const string emptyNumber = "";
		const string mrnWithoutEntryNumber = "MRN_1";
		const string correlationIdentifier = "123456";
		const string correlationIdentifierInvalid = "100000";
		const string mrnForEntryNumberWithoutEntryHeader = "MRN_2";
		const string mrnForEntryNumberWithEntryHeader = "MRN_3";

		var entryHeader = Factory.CreateEntryHeaderWithEntryNumber(mrnForEntryNumberWithEntryHeader);
		Factory.CreateEntryNumber(mrnForEntryNumberWithoutEntryHeader);
		Factory.Save();
		var testEdiMessage = Factory.CreateCoreMessage(messageText: string.Empty, linkedObject: entryHeader);
		testEdiMessage.EM_MessageNum = correlationIdentifier;
		testEdiMessage.EM_Status = EDIMessage.Status.Sent;

		var supportsCorrelationProvider = typeof(ICorrelationProvider).IsAssignableFrom(typeof(TDataProvider));
		var supportsJobIdentification = typeof(IJobIdentification).IsAssignableFrom(typeof(TDataProvider));
		if (!supportsCorrelationProvider && !supportsJobIdentification)
		{
			Assert("Either ICorrelationProvider or IJobIdentification must be supported, otherwise we cannot find a LinkedObject!", condition: false);
		}

		CombineAssertions(() =>
		{
			Mock<ICorrelationProvider> correlationProviderMock = null;
			Mock<IJobIdentification> jobIdentificationMock = null;
			if (supportsCorrelationProvider)
			{
				correlationProviderMock = dataProviderMock.As<ICorrelationProvider>();
				correlationProviderMock.Setup(x => x.CorrelationIdentifier).Returns(emptyNumber);
			}
			if (supportsJobIdentification)
			{
				jobIdentificationMock = dataProviderMock.As<IJobIdentification>();
				jobIdentificationMock.Setup(x => x.LRN).Returns(emptyNumber);
				jobIdentificationMock.Setup(x => x.MRN).Returns(emptyNumber);
			}
			AssertPreProcessMessage("Empty MRN and LRN and CorrelationIdentifier", expectedStatus: EDIMessage.Status.Failed);

			AssertCorrelationProvider();
			AssertJobIdentification();
			return;

			void AssertCorrelationProvider()
			{
				if (!supportsCorrelationProvider || correlationProviderMock == null)
				{
					return;
				}

				correlationProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifierInvalid);
				AssertPreProcessMessage("Invalid CorrelationIdentifier", expectedStatus: EDIMessage.Status.Failed);

				correlationProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
				AssertPreProcessMessage("Valid CorrelationIdentifier - please ensure that EdiMessage is added after Factory.Save() otherwise its gonna fail (unique EM_MessageNum will be generated).",
										expectedStatus: EDIMessage.Status.PreProcessedOK, entryHeader);
			}

			void AssertJobIdentification()
			{
				if (!supportsJobIdentification || jobIdentificationMock == null)
				{
					return;
				}

				if (supportsCorrelationProvider && correlationProviderMock != null)
				{
					correlationProviderMock.Setup(x => x.CorrelationIdentifier).Returns(emptyNumber);
				}

				jobIdentificationMock.Setup(x => x.MRN).Returns(mrnWithoutEntryNumber);
				AssertPreProcessMessage("Missed entry number", expectedStatus: EDIMessage.Status.Failed);

				jobIdentificationMock.Setup(x => x.MRN).Returns(mrnForEntryNumberWithoutEntryHeader);
				AssertPreProcessMessage("Entry Number Without Entry Header", expectedStatus: EDIMessage.Status.Failed);

				jobIdentificationMock.Setup(x => x.MRN).Returns(mrnForEntryNumberWithEntryHeader);
				AssertPreProcessMessage("Valid MRN", expectedStatus: EDIMessage.Status.PreProcessedOK, entryHeader);

				jobIdentificationMock.Setup(x => x.MRN).Returns(emptyNumber);
				jobIdentificationMock.Setup(x => x.LRN).Returns("LRN_1");
				AssertPreProcessMessage("Invalid LRN", expectedStatus: EDIMessage.Status.Failed);

				jobIdentificationMock.Setup(x => x.LRN).Returns(entryHeader.CH_BGMReference);
				AssertPreProcessMessage("Valid LRN", expectedStatus: EDIMessage.Status.PreProcessedOK, entryHeader);
			}
		});

		void AssertPreProcessMessage(string description, string expectedStatus, CusEntryHeader expectedLinkedObject = null)
		{
			var interchange = Factory.New<EDIInterchange>();
			var message = Factory.New<EDIMessage>();
			message.EM_EI = interchange.PK;

			MessageProcessor.PreProcessMessage(message);

			AssertEquals($"{description}: EM_LinkedObject", expectedLinkedObject, message.EM_LinkedObject);
			AssertEquals($"{description}: EM_Status", expectedStatus, message.EM_Status);
			if (message.EM_Status == EDIMessage.Status.Failed)
			{
				var expectedNoteText = "The message pre-process failed because the message could not be linked to a job";
				message.AssertHasLogMessagePart($"{description}: Message has discarded message log", Events.ErrorReport, expectedNoteText);
				serviceLogger.AssertHasLogMessagePart($"{description}: Service has discarded message log", LogType.Error, expectedNoteText);
			}
		}
	}

	protected (CusEntryHeader EntryHeader, EDIMessage TransmitMessage, EDIMessage InboundMessage) PrepareDataForInboundMessageTest(string correlationIdentifier, string mrn)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		if (!string.IsNullOrEmpty(mrn))
		{
			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
			cusEntryNumber.CE_EntryNum = mrn;
			cusEntryNumber.Parent = entryHeader;
		}

		var transmittedMessage = Factory.New<EDIMessage>();
		transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.PLCustoms;
		transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		transmittedMessage.EM_Status = EDIMessage.Status.Sent;
		transmittedMessage.EM_MessageNum = correlationIdentifier;
		transmittedMessage.EM_LinkedObject = entryHeader;

		var transmittedInterchange = Factory.New<EDIInterchange>();
		transmittedInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.PLCustoms;
		transmittedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		transmittedInterchange.EI_From = "TestFrom";
		transmittedInterchange.EI_To = "TestTo";
		transmittedInterchange.EI_Status = EDIInterchange.Status.Sent;
		transmittedInterchange.EI_SessionGUID = ZGuid.NewZGuid();
		transmittedMessage.EM_EI = transmittedInterchange.PK;

		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.PLCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
		incomingMessage.EM_LinkedObject = entryHeader;

		var incomingInterchange = Factory.New<EDIInterchange>();
		incomingInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.PLCustoms;
		incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		incomingInterchange.EI_Status = EDIInterchange.Status.Sent;
		incomingInterchange.EI_From = "TestFrom";
		incomingInterchange.EI_To = "TestTo";
		incomingInterchange.EI_SessionGUID = transmittedInterchange.EI_SessionGUID;
		incomingMessage.EM_EI = incomingInterchange.PK;

		return (entryHeader, transmittedMessage, incomingMessage);
	}

	protected void ProcessMessageAndTestEmail(string expectedSubject, string correlationIdentifier, string entryNumMrn)
	{
		const string emailAddress = "abc@abc.com";
		const string testUserCode = "ABC";

		var user = Factory.New<GlbStaff>();
		user.GS_Code = testUserCode;
		user.GS_EmailAddress = emailAddress;

		var (entryHeader, transmitMessage, inboundMessage) = PrepareDataForInboundMessageTest(correlationIdentifier, entryNumMrn);
		transmitMessage.EM_SystemCreateUser = testUserCode;

		Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		MessageProcessor.ProcessMessage(inboundMessage);
		CombineAssertions(() => AssertEmail(emailAddress, expectedSubject, expectedBody: inboundMessage.EM_MessageInterpretation, relatedJob: entryHeader));
	}
}

sealed class ImpExpMessageProcessorTest : ImpExpMessageProcessorBaseTest<ImpExpMessageProcessorForTest, IAESIncomingMessage>
{
	protected override string ExpectedMessageFriendlyName => "Import/Export Test Message Processor";

	protected override bool ExpectedIsFailureNotification => false;

	public void TestDocumentsAddedToCustomsEntryHeaderJob()
	{
		const string correlationIdentifier = "24IE11100000001";

		var (entryHeader, _, inboundMessage) = PrepareDataForInboundMessageTest(correlationIdentifier, "TEST_MRN");

		inboundMessage.DocManagerInfo.AddFileOrDocument([64, 128, 62], "Document.pdf", Core.Constants.RefDocTypes.TransitAccompanyingDocument);
		inboundMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;

		dataProviderMock.As<ICorrelationProvider>().Setup(p => p.CorrelationIdentifier).Returns(correlationIdentifier);

		var messageProcessor = messageProcessorMock.Object;
		messageProcessor.ProcessMessage(inboundMessage);

		Factory.Save();

		AssertDocumentsAdded(entryHeader, ("Document.pdf", "TAD", 3));
	}
}

public class ImpExpMessageProcessorForTest(LoggingInformation logger) : ImpExpMessageProcessorBase<IAESIncomingMessage>(logger)
{
	protected override string MessageFriendlyNameCore => "Import/Export Test Message Processor";

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IAESIncomingMessage messageDataProvider) => ProcessingResult.Succeed;
}
