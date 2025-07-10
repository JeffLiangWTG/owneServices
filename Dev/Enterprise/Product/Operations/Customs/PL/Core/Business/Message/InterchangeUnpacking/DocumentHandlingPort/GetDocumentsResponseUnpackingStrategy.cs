using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.PL.MessageContracts.DataProviders;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.DocumentHandlingPort;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.PL.Business.Constants;
using static Enterprise.Messaging.Integration.EDIInterchangeStatusList.Codes;
using static Enterprise.ZArchitecture.Business.AutoEvents;
using CusPollingTransaction = Enterprise.Customs.Business.CusPollingTransaction;

namespace Enterprise.Customs.PL.Business;

sealed class GetDocumentsResponseUnpackingStrategy(DataProviderFactory dataProviderFactory) : IXmlInterchangeUnpackingStrategy
{
	DataProviderFactory DataProviderFactory { get; } = Argument.NotNull(dataProviderFactory, nameof(dataProviderFactory));
	IMessageLocatorResolver MessageLocatorResolver { get; } = new MessageLocatorResolver();

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Log strings")]
	public EDIInterchangeUnpackerResult Unpack(
		EDIInterchange interchange,
		EDIInterchange outgoingInterchange,
		EnterpriseEDIMessage outgoingMessage,
		XmlReader reader,
		ISimpleLogger logger)
	{
		if (outgoingMessage is null)
		{
			return new EDIInterchangeUnpackerResult(errorReason:
				(NoResString)"Interchange processing failed because related transmit message couldn't be located.");
		}

		if (!IsCusPollingTransactionFound(interchange, outgoingMessage, logger))
		{
			return new EDIInterchangeUnpackerResult(errorReason: (NoResString)"Corresponding CusPoolingTransaction not found!");
		}

		var containedMessagesBeforeProcessing = interchange.ContainedMessages.Count;
		var initialDepth = reader.Depth;
		var processingResults = new List<ResponseDocumentProcessingResult>();
		var documentNumber = 0;
		reader.Read();
		while (!reader.EOF && reader.Depth >= initialDepth)
		{
			if (reader.NodeType != XmlNodeType.Element)
			{
				if (!reader.MoveToFirstElement())
				{
					break;
				}
				continue;
			}

			switch (reader.LocalName)
			{
				case "documentsCount":
					if (reader.ReadElementContentAsString() is { } documentsCountStr &&
						int.TryParse(documentsCountStr, out var documentsCount) &&
						documentsCount == 0)
					{
						interchange.Logs.AddNew(InterchangeReceived, "During GetDocumentsResponse Interchange processing no documents were successfully processed.");
						return new EDIInterchangeUnpackerResult(Array.Empty<EnterpriseEDIMessage>());
					}
					break;

				case "document":
					var documentProcessingResult = ProcessResponseDocument(interchange, documentNumber, reader);
					processingResults.Add(documentProcessingResult);
					documentNumber++;
					break;

				default:
					reader.Skip();
					break;
			}
		}

		if (processingResults.Count == 0)
		{
			return new EDIInterchangeUnpackerResult(errorReason:
				(NoResString)"Interchange processing failed because GetDocumentsResponse doesn't contain any documents.");
		}

		var hasProcessedDocument = false;
		foreach (var (processingResult, contentNames) in processingResults
			.Select(x => (x.ResultCode, ContentName: x.IsSuccessful ? x.MessageName : x.FileName.IfEmptyUse(() => (ZString)$"Document number {x.DocumentNumber}")))
			.GroupBy(x => x.ResultCode)
			.Select(x => (x.Key, x.Select(doc => doc.ContentName).Distinct().ToList())))
		{
			var contentNamesCommaText = string.Join(", ", contentNames);
			var multipleItemsSuffix = contentNames.Count > 1 ? "s" : "";
			switch (processingResult)
			{
				case XmlProcessingResult.NotSupportedContent:
					LogHelper.Log(ErrorReport, $"Not supported content: {contentNamesCommaText}.", serviceLog: logger, interchange);
					break;
				case XmlProcessingResult.XmlParsingError:
					LogHelper.Log(ErrorReport, $"Xml parse error{multipleItemsSuffix}: {contentNamesCommaText}.", serviceLog: logger, interchange);
					break;
				case XmlProcessingResult.TransmitMessageNotFound:
					LogHelper.Log(ErrorReport, $"Transmit message{multipleItemsSuffix} not found: {contentNamesCommaText}.", serviceLog: logger, interchange);
					break;
				case XmlProcessingResult.MessageIdentificationMaxLengthExceeded:
					LogHelper.Log(ErrorReport, $"Message{multipleItemsSuffix} have too long identification: {contentNamesCommaText}.", serviceLog: logger, interchange);
					break;
				case XmlProcessingResult.Processed:
					var createdMessagesCount = interchange.ContainedMessages.Count - containedMessagesBeforeProcessing;
					var createdMessagesMsg = createdMessagesCount > 0
						? $"created {createdMessagesCount} messages."
						: (NoResString)"all messages found to be already was created.";
					LogHelper.Log(InterchangeAcknowledged, $"GetDocumentsResponse acknowledgement{multipleItemsSuffix} of {contentNamesCommaText} is processed, {createdMessagesMsg}",
						serviceLog: logger,
						interchange);
					hasProcessedDocument = true;
					break;
				case XmlProcessingResult.AlreadyProcessed:
					break;
				default:
					throw new InvalidOperationException($"Invalid value of {nameof(processingResult)}: {processingResult.ToString()}");
			}
		}

		if (!hasProcessedDocument)
		{
			interchange.EI_Status = Error;
		}

		return new EDIInterchangeUnpackerResult(processingResults.Select(x => x.CreatedMessage).WhereNotNull().ToArray());
	}

	ResponseDocumentProcessingResult ProcessResponseDocument(EDIInterchange interchange, int documentNumber, XmlReader reader)
	{
		var factory = interchange.Factory;
		IResponseDocument responseDocument;
		try
		{
			responseDocument = DataProviderFactory.NewOrNull<IResponseDocument>(reader);
		}
		catch (Exception e) when (e.Find<XmlException>() is not null)
		{
			return new(XmlProcessingResult.XmlParsingError, documentNumber, FileName: default, MessageName: default, CreatedMessage: null);
		}
		if (responseDocument == null)
		{
			return new(XmlProcessingResult.XmlParsingError, documentNumber, FileName: default, MessageName: default, CreatedMessage: null);
		}

		var filename = responseDocument.Filename;
		string messageName = null;
		try
		{
			var body = responseDocument.Content;
			string documentXml;
			try
			{
				documentXml = Encoding.UTF8.GetString(body);
			}
			catch (Exception e) when (e.Find<DecoderFallbackException>() is not null || e.Find<FormatException>() is not null)
			{
				return new(XmlProcessingResult.NotSupportedContent, documentNumber, filename, MessageName: default, CreatedMessage: null);
			}
			using var xmlReader = XmlHelper.CreateReaderAndGotoRootNode(documentXml);

			var messageNode = new XmlQualifiedName(xmlReader.LocalName, xmlReader.NamespaceURI);
			if (!RecognizableMessages.All.ByXmlName.TryGetValue(messageNode, out var messageDefinition) ||
				MessageLocatorResolver.GetMessageLocator(messageDefinition.MessageType) is not { } messageLocator)
			{
				return new(XmlProcessingResult.NotSupportedContent, documentNumber, filename, MessageName: default, CreatedMessage: null);
			}
			messageName = messageDefinition.Name;

			var messageDataProvider = DataProviderFactory.NewOrNull(xmlReader);
			if (messageDataProvider is not IIncomingMessage incomingMessage)
			{
				return new(XmlProcessingResult.NotSupportedContent, documentNumber, filename, messageName, CreatedMessage: null);
			}

			string messageIdentification = null;
			string externalSystemID = null;
			if (messageDataProvider is IMessageWithIdentification messageWithIdentification)
			{
				if (string.IsNullOrEmpty(messageIdentification = messageWithIdentification.MessageIdentification.Trim()))
				{
					return new(XmlProcessingResult.NotSupportedContent, documentNumber, filename, messageName, CreatedMessage: null);
				}
				if (messageIdentification.Length > EDIMessageSchema.EM_MessageNum.MaxLength)
				{
					return new(XmlProcessingResult.MessageIdentificationMaxLengthExceeded, documentNumber, filename, messageName, CreatedMessage: null);
				}
				if (factory.MessageWithMessageNumExist(AllSupportedApplications, messageIdentification, ReceiveTransmitList.Codes.Receive))
				{
					return new(XmlProcessingResult.AlreadyProcessed, documentNumber, filename, messageName, CreatedMessage: null);
				}
			}
			else if (messageDataProvider is IExternalSystemIdProvider externalSystemIdProvider)
			{
				if (string.IsNullOrEmpty(externalSystemID = externalSystemIdProvider.ExternalSystemID.Trim()))
				{
					return new(XmlProcessingResult.NotSupportedContent, documentNumber, filename, messageName, CreatedMessage: null);
				}
				if (externalSystemID.Length > EDIMessageSchema.EM_ApplicationReference.MaxLength)
				{
					return new(XmlProcessingResult.MessageIdentificationMaxLengthExceeded, documentNumber, filename, messageName, CreatedMessage: null);
				}
				if (factory.MessageByExternalSystemIDExist(AllSupportedApplications, externalSystemID, ReceiveTransmitList.Codes.Receive))
				{
					return new(XmlProcessingResult.AlreadyProcessed, documentNumber, filename, messageName, CreatedMessage: null);
				}
			}

			if (messageLocator.FindTransmitMessage(factory, incomingMessage) is not BaseEDIMessage transmitMessage)
			{
				return new(XmlProcessingResult.TransmitMessageNotFound, documentNumber, filename, messageName, CreatedMessage: null);
			}

			var messageSubType = GetMessageSubTypeFromDataProvider(messageDefinition, messageDataProvider);

			var message = (BaseEDIMessage)interchange.ContainedMessages.AddNew(transmitMessage.GetType());
			if (!string.IsNullOrEmpty(messageIdentification))
			{
				message.EM_MessageNum = messageIdentification;
			}
			message.EM_ApplicationCode = transmitMessage.EM_ApplicationCode;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageType = transmitMessage.EM_MessageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_GB = transmitMessage.EM_GB;
			message.EM_EM_RequestMessage = transmitMessage.PK;
			message.EM_LinkedObject = transmitMessage.EM_LinkedObject;
			if (!string.IsNullOrEmpty(externalSystemID))
			{
				message.EM_ApplicationReference = externalSystemID;
			}
			message.EM_Status = EDIMessage.Status.Queued;
			message.SetEM_MessageTextOrDataSource(new StringReader(documentXml).CopyAndDispose());

			if (responseDocument.Attachments.Count > 0)
			{
				message.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(true);
				responseDocument.Attachments.ForEach(attachment =>
					message.DocManagerInfo.AddFileOrDocument(attachment.Content, attachment.Filename, Core.Constants.RefDocTypes.TransitAccompanyingDocument));
			}

			var documentCodes = message.EM_MessageType != message.EM_MessageSubType
				? $"{transmitMessage.EM_ApplicationCode}:{message.EM_MessageType}/{message.EM_MessageSubType}"
				: $"{transmitMessage.EM_ApplicationCode}:{message.EM_MessageType}";
			interchange.Logs.AddNew(Events.InterchangeInProgress, $"{documentCodes} message was created from document [{documentNumber}]={filename}.");

			return new(XmlProcessingResult.Processed, documentNumber, filename, messageName, CreatedMessage: message);
		}
		catch (Exception e) when (e.Find<XmlException>() is not null)
		{
			return new(XmlProcessingResult.XmlParsingError, documentNumber, filename, messageName, CreatedMessage: null);
		}
	}

	static string GetMessageSubTypeFromDataProvider(IMessageDefinition definition, object dataProvider)
	{
		var result = definition.SubCode;
		if (dataProvider is IConfirmation { NotificationType: { } notificationType } &&
			!string.IsNullOrEmpty(notificationType))
		{
			result = notificationType;
		}
		return result;
	}

	bool IsCusPollingTransactionFound(
		EDIInterchange interchange,
		EnterpriseEDIMessage outgoingMessage,
		ISimpleLogger logger)
	{
		if (outgoingMessage.EM_LinkedObject is not CusPollingTransaction cusPollingTransaction)
		{
			LogHelper.Log(ErrorReport, "CusPoolingTransaction for transmit message not found.", serviceLog: logger, interchange);
			return false;
		}

		if (cusPollingTransaction.CPT_Type != Core.Constants.Customs.CusPollingTransactionType.Codes.BLG
			&& cusPollingTransaction.CPT_Type != Core.Constants.Customs.CusPollingTransactionType.Codes.PLC)
		{
			LogHelper.Log(ErrorReport, $"Unsupported type of CusPoolingTransaction :{cusPollingTransaction.CPT_Status}.", serviceLog: logger, interchange);
			return false;
		}

		return true;
	}

	public IReadOnlyCollection<XmlQualifiedName> SupportedXmlNodes => [PUESC.DocumentHandlingPort.XmlNodes.GetDocumentsResponse];

	sealed record ResponseDocumentProcessingResult(XmlProcessingResult ResultCode, int DocumentNumber, ZString FileName, ZString MessageName, EnterpriseEDIMessage CreatedMessage)
	{
		public bool IsSuccessful => ResultCode == XmlProcessingResult.Processed;
	}
}
