using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.PL.MessageContracts.DataProviders;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using ProcessingStatus = Enterprise.Messaging.Business.EDIMessage.Status;

namespace Enterprise.Customs.PL.Business;

public abstract class BaseMessageProcessor<TDataProvider>(LoggingInformation logger) : BranchCustomsApplicationTypeMessageProcessor(logger)
	where TDataProvider : class
{
	protected abstract LocatorBase Locator { get; }

	protected IReadOnlyCollection<IMessageDefinition> SupportedMessages => RecognizableMessages.All.ByMessageProvider[typeof(TDataProvider)];

	protected sealed record ProcessingResult(ZString MessageStatusToSet)
	{
		public static ProcessingResult Succeed => new (ProcessingStatus.ProcessedOK);
		public static ProcessingResult Fail => new (ProcessingStatus.Failed);
		public static ProcessingResult Discarded => new (ProcessingStatus.Discarded);
	}

	protected virtual BusinessObject GetLinkedObject(BaseEDIMessage message, TDataProvider dataProvider)
	{
		if (dataProvider is IIncomingMessage incomingMessage &&
			Locator.FindBusinessObjectForIncomingMessage(message.Factory, incomingMessage) is { } result)
		{
			return result;
		}

		LogMessageProcessingError(message, Res.GetString("C910438B-1071-49D1-8FA9-78B04C92204F",
			"The message pre-process failed because the message could not be linked to a job"));
		return null;
	}

	protected virtual IMessageInterpreter<TDataProvider> CreateMessageInterpreter(BaseEDIMessage message) => null;

	protected sealed override void PreProcessMessageCore(EnterpriseEDIMessage ediMessage)
	{
		if (ediMessage is not BaseEDIMessage message || message.EM_Status != ProcessingStatus.Queued)
		{
			return;
		}

		TDataProvider messageDataProvider;
		try
		{
			messageDataProvider = GetMessageDataProvider(message);
		}
		catch (Exception ex)
		{
			message.EM_Status = ProcessingStatus.Error;
			var error = ex.Message;
			if (ex.InnerException is XmlSchemaValidationException or InvalidOperationException)
			{
				error += " - " + ex.InnerException.Message;
			}
			LogMessageProcessingError(message, $"Unable to create data message provider: {error}");
			throw;
		}

		if (GetLinkedObject(message, messageDataProvider) is BusinessObject linkedObject)
		{
			message.EM_LinkedObject = linkedObject;
			if (GetBranchPK(linkedObject) is { IsValid: true } branchPk)
			{
				message.EM_GB = branchPk;
			}

			PreProcessMessageCore(message, messageDataProvider);
		}
		else
		{
			message.EM_Status = ProcessingStatus.Failed;
			LogMessageProcessingError(message, (NoResString)"Unable to find a linked business object for message");
		}
	}

	protected void LogMessageProcessingError(EnterpriseEDIMessage message, string error)
	{
		message.Logs.AddNew(Events.ErrorReport, error);
		Logger.LogError(error.LastOrDefault() == '.'
			? $"{error} {GetMessageDetailsForError(message)}."
			: $"{error}. {GetMessageDetailsForError(message)}.");
	}

	protected virtual void PreProcessMessageCore(BaseEDIMessage message, TDataProvider messageDataProvider) => base.PreProcessMessageCore(message);

	protected sealed override void ProcessMessageCore(EnterpriseEDIMessage ediMessage)
	{
		var message = (BaseEDIMessage)ediMessage;

		if (message.EM_Status != ProcessingStatus.PreProcessedOK)
		{
			message.EM_Status = ProcessingStatus.Error;
			LogMessageProcessingError(message, $"Message status must be {ProcessingStatus.PreProcessedOK} for processing");
			return;
		}

		var messageDataProvider = GetMessageDataProvider(message);
		var processingResult = ProcessMessageCore(message, messageDataProvider);
		if (processingResult == ProcessingResult.Succeed)
		{
			InterpretMessageCore(message, messageDataProvider);
			AddDocumentsToRelatedJob(message, messageDataProvider);
			SendEmail(message, messageDataProvider);
		}
		message.EM_Status = processingResult.MessageStatusToSet;
	}

	protected virtual bool IsFailureNotification => false;

	void SendEmail(BaseEDIMessage message, TDataProvider messageDataProvider)
	{
		var emailSubject = EmailSubject(message, messageDataProvider);
		if (emailSubject == null)
		{
			return;
		}

		var emailBody = GetEmailBody(message, messageDataProvider);
		if (string.IsNullOrWhiteSpace(emailBody))
		{
			return;
		}

		GenerateHtmlEmailAndSendToOriginalOrGroup(
			factory: message.Factory,
			relatedJob: GetRelatedJob(message, messageDataProvider),
			messageTypeInSubject: emailSubject,
			body: emailBody,
			isFailure: IsFailureNotification,
			branchForEmailLogo: message.Branch,
			sourceBusinessObject: message.EM_LinkedObject,
			getEmailAddressToSendTo: () => GetSentTo(message, messageDataProvider));
	}

	protected BaseEDIMessage GetTransmitMessage(BaseEDIMessage message, TDataProvider messageDataProvider)
		=> message.Factory.GetCachedValue($"Find_TransmitMessage_For_{message.PK}",
			() => FindTransmitMessageCore(message, messageDataProvider));

	protected virtual BaseEDIMessage FindTransmitMessageCore(BaseEDIMessage message, TDataProvider messageDataProvider)
		=> messageDataProvider is IIncomingMessage incomingMessage
			? Locator.FindTransmitMessage(message.Factory, incomingMessage)
			: null;

	protected virtual string GetSentTo(BaseEDIMessage message, TDataProvider messageDataProvider)
	{
		var transmitMessage = GetTransmitMessage(message, messageDataProvider);
		if (transmitMessage == null)
		{
			return string.Empty;
		}

		var emailAddress = GetEmailAddressToSendToFromQueuedUser(transmitMessage);
		return emailAddress.IsEmpty ? PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.Value : emailAddress;
	}

	protected virtual string GetEmailBody(BaseEDIMessage message, TDataProvider messageDataProvider) => message.EM_MessageInterpretation;

	protected virtual string EmailSubject(BaseEDIMessage message, TDataProvider messageDataProvider) => string.Empty;

	protected virtual IRelatedJob GetRelatedJob(BaseEDIMessage message, TDataProvider messageDataProvider) => null;

	protected abstract ProcessingResult ProcessMessageCore(BaseEDIMessage message, TDataProvider messageDataProvider);

	protected virtual void InterpretMessageCore(BaseEDIMessage message, TDataProvider messageDataProvider)
	{
		if (MessageInterpreterType is not null && MessageInterpreterType.IsSubclassOfRawGeneric(typeof(MessageInterpreterBase<,>)))
		{
			var interpretMessageObject = CreateMessageInterpreter(message);
			message.EM_MessageInterpretation = interpretMessageObject.Interpret(messageDataProvider);
		}
	}

	protected internal virtual TDataProvider GetMessageDataProvider(BaseEDIMessage message) => GetCachedDataProvider(message);

	TDataProvider GetCachedDataProvider(BaseEDIMessage message) => message.Factory.GetCachedValue($"{typeof(TDataProvider)}_{message.PK}", () => CreateNewMessageDataProvider(message));

	protected internal virtual TDataProvider CreateNewMessageDataProvider(BaseEDIMessage message)
	{
		using var textReader = message.GetEM_MessageTextReader();
		using var xmlReader = XmlHelper.CreateReaderAndGotoRootNode(textReader, closeInput: true);
		if (SoapHelper.IsSoap(xmlReader))
		{
			xmlReader.MoveToSoapBody();
		}
		var dataProviderFactory = new DataProviderFactory(RecognizableMessages.All);
		return dataProviderFactory.NewOrNull<TDataProvider>(xmlReader)
			?? throw new XmlException("Please check the XML");
	}

	protected virtual Type MessageInterpreterType => null;

	static ZGuid GetBranchPK(BusinessObject businessObject) => businessObject is IBranchProvider branchProvider ? branchProvider.Branch.PK : ZGuid.Empty;

	void AddDocumentsToRelatedJob(BaseEDIMessage message, TDataProvider messageDataProvider)
	{
		if (message.DocManagerInfo.Files.Count == 0 || GetRelatedJob(message, messageDataProvider) is not IDocManagerSupport docManagerSupport)
		{
			return;
		}

		docManagerSupport.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(true);
		foreach (IeDoc document in message.DocManagerInfo.AllEDocs)
		{
			docManagerSupport.DocManagerInfo.AddFileOrDocument(document.ImageData, document.FileName, document.DocType);
		}
	}

	string GetMessageDetailsForError(EnterpriseEDIMessage message) => $"(Interchange Number:{message.EM_InterchangeNumber}, Number:{message.EM_MessageNum}, Type:{message.EM_MessageType}); message status set to {message.EM_Status}";
}
