using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using CargoWise.Application;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using ProcessingStatus = Enterprise.Messaging.Business.EDIMessage.Status;

namespace Enterprise.Customs.PL.Business.Testing;

[TestsSubclassesOf(typeof(BaseMessageProcessor<>))]
public abstract class BaseMessageProcessorTestCase<TMessageProcessor, TDataProvider> : TestCaseWithFactory
	where TDataProvider : class
	where TMessageProcessor : BaseMessageProcessor<TDataProvider>
{
	public virtual void TestApplicationCode()
	{
		AssertEquals(ApplicationCodeList.Codes.PLCustoms, MessageProcessor.ApplicationCode);
	}

	public void TestMessageFriendlyName()
	{
		AssertEquals(ExpectedMessageFriendlyName, MessageProcessor.MessageFriendlyName);
	}

	protected virtual BaseEDIMessage CreateTestMessage() => Factory.New<EDIMessage>();

	public void TestPreProcessMessage_Error()
	{
		const string expectedErrorLogNoteRegEx = "^Unable to create data message provider: .*?";

		var message = CreateTestMessage();
		messageProcessorMock.Setup(m => m.GetMessageDataProvider(It.IsAny<BaseEDIMessage>())).Throws<Exception>();

		CombineAssertions(() =>
		{
			AssertExceptionThrown<Exception>(() => messageProcessor.PreProcessMessage(message));
			AssertWasLogError(message, expectedErrorLogNoteRegEx);
			AssertEquals("EM_Status if message format is wrong", ProcessingStatus.Error, message.EM_Status);
		});
	}

	public void TestPreProcessMessage_NotPreprocessed()
	{
		const string expectedErrorLogNoteRegEx = $"^Message status must be {ProcessingStatus.PreProcessedOK} for processing";

		var message = CreateTestMessage();
		message.EM_Status = ProcessingStatus.Queued;
		messageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertWasLogError(message, expectedErrorLogNoteRegEx);
			AssertEquals("EM_Status if message format is wrong", ProcessingStatus.Error, message.EM_Status);
		});
	}

	public void TestPreProcessMessage_MessageDataProviderNotGenerated()
	{
		const string expectedErrorLogNote = "Unable to create data message provider";

		var message = CreateTestMessage();
		dataProviderMock = new Mock<TDataProvider> { CallBase = true };
		messageProcessorMock = new Mock<TMessageProcessor>(serviceLogger) { CallBase = true };
		var messageInfo = @$"\. \(Interchange Number:{message.EM_InterchangeNumber}, Number:{message.EM_MessageNum}, Type:{message.EM_MessageType}\); message status set to {ProcessingStatus.Error}\.$";

		CombineAssertions(() =>
		{
			AssertExceptionThrown<XmlException>("XML is not correct", () => messageProcessorMock.Object.PreProcessMessage(message));
			message.AssertHasLogMessagePart("Assert message log has error", Events.ErrorReport, expectedErrorLogNote);
			serviceLogger.AssertHasLogMessagePart("Assert service log has error", LogType.Error, expectedErrorLogNote);
			AssertEquals("EM_Status if message format is wrong", ProcessingStatus.Error, message.EM_Status);
		});
	}

	public void TestEmailGroupRegistryItem()
	{
		var method = typeof(BranchCustomsApplicationTypeMessageProcessor).GetMethod("GetEmailGroupRegistryItem", BindingFlags.NonPublic | BindingFlags.Instance);

		AssertNotNull(method);

		var emailGroupRegistryItem = method!.Invoke(MessageProcessor, Array.Empty<object>());
		Assert(
			message: $"{typeof(TMessageProcessor)} email group should rely on \"{ExpectedEmailGroupRegistryItem?.Category}/{ExpectedEmailGroupRegistryItem?.Caption}\" registry item",
			condition: ReferenceEquals(ExpectedEmailGroupRegistryItem, emailGroupRegistryItem));
	}

	protected void AssertWasLogError(BaseEDIMessage message, string expectedErrorRegEx)
	{
		var messageInfo = @$"\. \(Interchange Number:{message.EM_InterchangeNumber}, Number:{message.EM_MessageNum}, Type:{message.EM_MessageType}\); message status set to {ProcessingStatus.Error}\.$";
		message.AssertHasLogMessageRegEx("Assert message log has error", Events.ErrorReport, expectedErrorRegEx);
		var expectedServiceLogWithMessageInfoRegEx = expectedErrorRegEx + messageInfo;
		serviceLogger.AssertHasLogMessageRegEx("Assert service log has error", LogType.Error, expectedServiceLogWithMessageInfoRegEx);
	}

	public void TestIsFailureNotification()
	{
		AssertEquals(ExpectedIsFailureNotification, messageProcessor.GetType().GetProperty("IsFailureNotification", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(messageProcessor));
	}

	protected void AssertEmail(string expectedEmailAddress, string expectedSubject, string expectedBody, IRelatedJob relatedJob, string descriptionPrefix = "")
	{
		var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
		AssertNotNull("Email created", email);
		if (email == null)
		{
			return;
		}
		AssertEquals("Only one email is created", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

		var recipients = email.Recipients.Cast<RecipientDef>().Select(x => x.Email).ToArray();
		AssertArrayEqualsByElements(descriptionPrefix + "Email send to expected address", [expectedEmailAddress], recipients);
		AssertEquals(descriptionPrefix + "Email expected subject", expectedSubject, email.Subject);
		AssertContains(descriptionPrefix + "Email contains expected message body", expectedBody, email.Body);
		var jobUri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(relatedJob);
		var jobNumber = relatedJob.JobNumber;
		var jobReference = $"<a href=\"{jobUri}\">{jobNumber}</a>";
		AssertContains(descriptionPrefix + "Email body contains job reference", jobReference, email.Body);
	}

	protected void AssertDocumentsAdded(BusinessObject businessObject, params (string FileName, string DocType, int Size)[] expectedFiles)
	{
		var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
		var documentFactory = documentFactoryProvider.GetFactory(Factory);
		var businessObjectStorage = documentFactory.GetStorageMainForPK(businessObject.PK);

		AssertNotNull("Related job should have storage available", businessObjectStorage);
		AssertEquals($"Related job should contain {expectedFiles.Length} file(s)", expectedFiles.Length, businessObjectStorage.Files.Count);

		CombineAssertions(() =>
		{
			for (var i = 0; i < expectedFiles.Length; i++)
			{
				var expectedFile = expectedFiles[0];
				var businessObjectDocument = businessObjectStorage.Files[0];

				AssertEquals($"Document #{i} should have expected file name", expectedFile.FileName, businessObjectDocument.FileName);
				AssertEquals($"Document #{i} should have expected document type", expectedFile.DocType, businessObjectDocument.DocType);
				AssertEquals($"Document #{i} should have expected size", expectedFile.Size, businessObjectDocument.ImageData.Length);
			}
		});
	}

	public void TestMessageInterpreterType()
	{
		AssertEquals(ExpectedMessageInterpreterType, messageProcessor.GetType().GetProperty("MessageInterpreterType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(messageProcessor));
	}

	protected virtual TMessageProcessor MessageProcessor => messageProcessor;

	protected abstract string ExpectedMessageFriendlyName { get; }

	protected abstract bool ExpectedIsFailureNotification { get; }

	protected virtual Type ExpectedMessageInterpreterType => null;

	protected virtual IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendExportAcknowledgements;

	protected override void SetUp()
	{
		base.SetUp();
		serviceLogger = new LoggingInformation();
		dataProviderMock = new Mock<TDataProvider>() { CallBase = true };
		if (typeof(ICorrelationProvider).IsAssignableFrom(typeof(TDataProvider)))
		{
			var correlationProviderMock = dataProviderMock.As<ICorrelationProvider>();
			correlationProviderMock.Setup(x => x.CorrelationIdentifier).Returns((string)null);
		}
		if (typeof(IJobIdentification).IsAssignableFrom(typeof(TDataProvider)))
		{
			var jobIdentificationMock = dataProviderMock.As<IJobIdentification>();
			jobIdentificationMock.Setup(x => x.LRN).Returns((string)null);
			jobIdentificationMock.Setup(x => x.MRN).Returns((string)null);
		}
		messageProcessorMock = new Mock<TMessageProcessor>(serviceLogger) { CallBase = true };
		messageProcessorMock.Setup(m => m.GetMessageDataProvider(It.IsAny<BaseEDIMessage>())).Returns(dataProviderMock.Object);
		messageProcessor = messageProcessorMock.Object;
	}

	protected LoggingInformation serviceLogger;
	protected Mock<TDataProvider> dataProviderMock;
	protected Mock<TMessageProcessor> messageProcessorMock;
	protected TMessageProcessor messageProcessor;
}

sealed class BaseMessageProcessorTest : BaseMessageProcessorTestCase<BaseMessageProcessorForTest, object>
{
	public void TestEmail_UserEmailProvided()
		=> TestEmail(userEmail: "abc@abc.com", communicationEmailChannelMail: "dfr@abc.com", expectedEmailAddress: "abc@abc.com");

	public void TestEmail_NoUserEmailProvided()
		=> TestEmail(userEmail: string.Empty, communicationEmailChannelMail: "abc@abc.com", expectedEmailAddress: "abc@abc.com");

	void TestEmail(string userEmail, string communicationEmailChannelMail, string expectedEmailAddress)
	{
		const string userCode = "ABC";

		var messageProcessor = messageProcessorMock.Object;

		var inboundMessage = Factory.New<EDIMessage>();
		inboundMessage.EM_Status = ProcessingStatus.PreProcessedOK;
		inboundMessage.EM_MessageInterpretation = "TEST_Interpretation";

		var transmitMessage = Factory.New<EDIMessage>();
		transmitMessage.EM_SystemCreateUser = userCode;

		var user = Factory.New<GlbStaff>();
		user.GS_Code = userCode;
		user.GS_LoginName = userCode;
		user.GS_EmailAddress = userEmail;

		var urlCreatorMock = new Mock<IShowEditFormUrlCreator>();
		urlCreatorMock.Setup(x => x.Create(messageProcessor.RelatedJob)).Returns("https://www.wisetechglobal.com/");
		using var urlCreatorSubstituteHolder = ObjectFactory.Substitute(urlCreatorMock.Object);

		using var temporaryRegistryValueHolder = PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.SetTemporaryValue(companyPk: Guid.Empty, branchPk: Guid.Empty, departmentPk: Guid.Empty, temporaryValue: communicationEmailChannelMail);
		messageProcessor.TransmitMessage = transmitMessage;
		messageProcessor.ProcessMessage(inboundMessage);
		CombineAssertions(() => AssertEmail(
			expectedEmailAddress,
			expectedSubject: BaseMessageProcessorForTest.TestEmailSubject + $" Response for {messageProcessor.RelatedJob.JobNumber}",
			expectedBody: inboundMessage.EM_MessageInterpretation,
			messageProcessor.RelatedJob));
	}

	protected override string ExpectedMessageFriendlyName => "Test PL Core Message Processor";

	protected override bool ExpectedIsFailureNotification => false;

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => null;
}

public class BaseMessageProcessorForTest : BaseMessageProcessor<object>
{
	public const string TestEmailSubject = "TestEmailSubject";

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.PLCustoms;

	protected override LocatorBase Locator => new LocatorPLC();

	public IRelatedJob RelatedJob { get; }

	public BaseMessageProcessorForTest(LoggingInformation logger) : base(logger)
	{
		var relatedJobMock = new Mock<IRelatedJob>();
		relatedJobMock.Setup(x => x.JobNumber).Returns("JobNumber");
		RelatedJob = relatedJobMock.Object;
	}

	public BaseEDIMessage TransmitMessage { get; set; }

	protected override IRelatedJob GetRelatedJob(BaseEDIMessage message, object messageDataProvider) => RelatedJob;

	protected override string MessageFriendlyNameCore => "Test PL Core Message Processor";

	protected override BaseEDIMessage FindTransmitMessageCore(BaseEDIMessage message, object messageDataProvider)
	{
		var transmitMessage = TransmitMessage;
		transmitMessage.EM_SystemCreateUser = "ABC";
		return transmitMessage;
	}

	protected override string EmailSubject(BaseEDIMessage message, object messageDataProvider) => TestEmailSubject;

	protected override BusinessObject GetLinkedObject(BaseEDIMessage message, object messageDataProvider) => throw new NotImplementedException();

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, object messageDataProvider) => ProcessingResult.Succeed;
}
