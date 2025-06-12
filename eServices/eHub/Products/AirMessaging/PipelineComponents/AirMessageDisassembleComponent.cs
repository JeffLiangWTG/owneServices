using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.XPath;
using BiztalkVirtualStream = Microsoft.BizTalk.Streaming.VirtualStream;
using WTG.ErrorReporting;
using CargoWise.eHub.DataAccess.Sql;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	[Guid("6473A2BB-373D-4035-97DD-BFF30257C230")]
	public class AirMessageDisassembleComponent : IBaseComponent, IComponentUI, IDisassemblerComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "AirMessage Disassembler"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IComponentUI Members

		public IntPtr Icon
		{
			get { return IntPtr.Zero; }
		}

		public IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		#endregion

		#region IDisassemblerComponent Members

		public void Disassemble(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (ConfigurationManager.AppSettings["IssueManagerUri"] == null)
			{
				var configMissingException = $"Errors occurred when processing message, Can not find the config file, kill the process to restart the application pool";
				WriteToEventLog(configMissingException);
				#if !DEBUG
				Process.GetCurrentProcess().Kill();
				#endif
				throw new AggregateException(configMissingException);
			}
			logger = GetPipelineLogger(message);
			LoggerHelpers.LogComponentStart(logger, this);

			#region Route When not enabled
			if (!Enabled)
			{
				MessageQueue.Enqueue(message);

				if (logger.IsTraceEnabled)
				{
					logger.Trace("Message enqueued.");
					LogMessageInfo(message);
				}
				LoggerHelpers.LogComponentEnd(logger, this);
				return;
			}
			#endregion

			receiveLocation = (string)message.Context.Read("ReceiveLocationName", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
			sender = PipelineContextExtensions.ReadPropertyString<BTS.SourceParty>(message.Context);
			var trackingID = Guid.NewGuid();
			var trackingIDString = trackingID.ToString().ToUpperInvariant();
			bool infoLogged = false;
			bool dataLogged = false;

			logger.TraceFormat("ReceiveLocation: {0}, Sender: {1}, MessageTrackingId: {2}", receiveLocation, sender, trackingID);

			try
			{
				PipelineContextExtensions.WriteProperty<InternalTrackingID>(message.Context, trackingIDString);
				PipelineContextExtensions.WriteProperty<MessageTrackingID>(message.Context, trackingIDString);

				if (infoLogged = logger.IsDebugEnabled) LogMessageInfo(message);
				new PipelineHelpers().CreateSeekableMessageStream(pipelineContext, message, logger);
				InsertToInbox(message, sender, trackingID);
				if (dataLogged = logger.IsTraceEnabled) LogMessageData(message);

				#region Parameter check when enabled
				if (string.IsNullOrEmpty(ServiceProviderID)) throw new ApplicationException("ServiceProviderID must be specified.");
				if (EnvelopeMessageSchema == null) throw new ApplicationException("EnvelopeMessageSchema must be specified.");
				if (FSUMessageSchema == null || string.IsNullOrEmpty(FSUMessageSchema.SchemaName)) throw new ApplicationException("FSUMessageSchema must be specified.");
				if (FMAFNAMessageSchema == null || string.IsNullOrEmpty(FMAFNAMessageSchema.SchemaName)) throw new ApplicationException("FMAFNAMessageSchema must be specified.");
				#endregion

				if (NormalizeLineEndings)
					message.BodyPart.Data = new NormalizedLineEndingsStream(message.BodyPart.GetOriginalDataStream());

				var envelopedMessage = message;

				if (!string.IsNullOrEmpty(EnvelopeMessageSchema.SchemaName))
				{
					logger.TraceFormat("Before FFDisassembler for SchemaName: {0}", EnvelopeMessageSchema.SchemaName);
					var ffDasm = GetFFDisassembler();
					envelopedMessage = ffDasm.Disassemble(pipelineContext, message, new SchemaWithNone(EnvelopeMessageSchema.ToString()));
				}

				if (!String.IsNullOrWhiteSpace(this.BatchedMessageXpath))
				{
					foreach (var bodyMessage in DebatchEvelopedMessage(pipelineContext, envelopedMessage))
						ProcessBodyMessage(pipelineContext, message, bodyMessage);
				}
				else
				{
					ProcessBodyMessage(pipelineContext, message, envelopedMessage);
				}
			}
			catch (Exception originalException)
			{
				if (!infoLogged) LogMessageInfo(message);
				if (!dataLogged) LogMessageData(message);
				logger.Trace("Caught exception", originalException);

				try
				{
					GetExceptionsAccessor().SubmitErrorAndUpdateStatus(Guid.NewGuid(), "BIZ", "Failure", BuildExceptionDescription(message, originalException), trackingID, Guid.Empty, trackingID, Guid.Empty, null, false);
				}
				catch (Exception accessorException)
				{
					var exception = new AggregateException($"Errors occurred when processing {sender}'s message.", originalException, accessorException);
					ReportErrorToIssueManager(message, exception);
					throw exception;
				}
				throw;
			}
			finally
			{
				LoggerHelpers.LogComponentEnd(logger, this);
			}
		}

		static void WriteToEventLog(string message)
		{
			string source = "eHub BizTalk";
			EventLog.WriteEntry(source, message, EventLogEntryType.Error);
		}

		private void LogMessageInfo(IBaseMessage message)
		{
			LoggerHelpers.LogProperties(logger, this);
			LoggerHelpers.LogMessageContextProperties(message, logger);
		}

		private void LogMessageData(IBaseMessage message)
		{
			logger.Error("========== START MESSAGE DATA ===================================================");
			var position = message.BodyPart.GetOriginalDataStream().Position;
			using (var msgData = new MemoryStream())
			{
				message.BodyPart.GetOriginalDataStream().CopyTo(msgData);
				logger.Error(Encoding.UTF8.GetString(msgData.ToArray()));
			}
			if (message.BodyPart.GetOriginalDataStream().CanSeek)
				message.BodyPart.GetOriginalDataStream().Position = position;
			logger.Error("========== END MESSAGE DATA ====================================================");
		}

		private void ReportErrorToIssueManager(IBaseMessage message, Exception e)
		{
			var errorBuilder = new EnterpriseErrorReportBuilder();
			errorBuilder = errorBuilder.SetKey($"eHub.Alert {receiveLocation} {sender} {e.Message}");
			errorBuilder.SetRandomErrorReportID();
			errorBuilder.SetTimeOfException(System.DateTime.Now);
			errorBuilder.SetSubject("eHub Alert");
			errorBuilder.SetRootException(e);
			errorBuilder.SetExceptionDescription(BuildExceptionDescription(message, e));
			errorBuilder.SetExeCreationTime(System.DateTime.Now);

			if (!string.IsNullOrWhiteSpace(issueMgrUri))
			{
				logger.Error("Sending error to issue manager.");
				using (var errorReporter = GetErrorReportingClient())
					errorReporter.PostCrashReportAsync(errorBuilder).Wait();
			}
			else
				logger.Error("Issue Manager reporting disabled in testing environment. Report details for logging only.");

			var reportStream = new MemoryStream();
			errorBuilder.WriteToAsync(reportStream).Wait();
			logger.ErrorFormat("Report details:\r\n{0}", Encoding.UTF8.GetString(reportStream.ToArray()));
		}

		private string BuildExceptionDescription(IBaseMessage message, Exception e)
		{
			var description = new StringBuilder(e.ToString()).Append(IssueMessageDelimiter);
			string name, ns;
			for (int i = 0; i < message.Context.CountProperties; i++)
			{
				object value = message.Context.ReadAt(i, out name, out ns);
				description.AppendFormat("{0}#{1} = {2}", ns, name, value).AppendLine();
			}

			return description.ToString();
		}

		internal virtual void InsertToInbox(IBaseMessage message, string sender, Guid trackingID)
		{
			string emailSubject = PipelineContextExtensions.ReadPropertyString<OverrideEmailSubject>(message.Context) ?? string.Empty;
			string fileName = PipelineContextExtensions.ReadPropertyString<OverrideFilename>(message.Context) ?? string.Empty;
			var messageStreamPosition = message.BodyPart.GetOriginalDataStream().Position;

			logger.TraceFormat("InsertToInbox started, Sender: {0}, EmailSubject: {1}, FileName: {2}, MessageStreamPosition: {3}, MessageTrackingId: {4}", sender, emailSubject, fileName, messageStreamPosition, trackingID);
			GetInboxAccessor().InsertToInbox(
				sender,
				Guid.Empty,
				trackingID,
				MessageStatus.Processing,
				new eHubGatewayMessage()
				{
					MessageTrackingID = trackingID,
					ApplicationCode = "BIZ",
					SchemaName = string.Empty,
					SchemaType = MessageSchemaType.Xml,
					EmailSubject = emailSubject,
					FileName = fileName,
					MessageStream = message.BodyPart.GetOriginalDataStream().CompressAndEncode()
				});
			logger.Trace("InsertToInbox ended");

			message.BodyPart.GetOriginalDataStream().Position = messageStreamPosition;
		}

		internal virtual void ProcessBodyMessage(IPipelineContext pipelineContext, IBaseMessage message, IBaseMessage bodyMessage)
		{
			logger.Trace("Start ProcessBodyMessage");
			var wrappedMessage = streamWrapper.Value.Execute(pipelineContext, bodyMessage);

			var context = CreateEnvelopContext(wrappedMessage);
			string recipientID = PipelineContextExtensions.ReadPropertyString<BTS.DestinationParty>(message.Context);
			if (!String.IsNullOrEmpty(recipientID)) context.RecipientID = recipientID;

			string senderID = PipelineContextExtensions.ReadPropertyString<BTS.SourceParty>(message.Context);
			if (!String.IsNullOrEmpty(senderID)) context.SenderID = senderID;

			if (string.IsNullOrEmpty(context.SenderID))
				throw new ApplicationException(
					string.IsNullOrEmpty(context.SenderPIMA) ?
					"The message sender can not be recognized." :
					string.Format("The message sender can not be recognized, the senderPIMA is {0}", context.SenderPIMA)
						);
			if (string.IsNullOrEmpty(context.MessageType))
				throw new ApplicationException("MessageType shoudn't be empty.");
			if (string.IsNullOrEmpty(context.InternalMessage))
				throw new ApplicationException("InternalMessage shoudn't be empty.");

			var internalMessageSchema = GetMessageBodySchema(context.MessageType, context.OriginalMessageType);
			if (internalMessageSchema == null)
				throw new ApplicationException(String.Format("Body Message Schema '{0}' not recognized.", context.MessageType));

			var extractedMessage = GetInternalMessage(pipelineContext, wrappedMessage, context.InternalMessage);
			DisassembleInternalMessage(pipelineContext, context, extractedMessage, message, internalMessageSchema);
		}

		internal IEnumerable<IBaseMessage> DebatchEvelopedMessage(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var xRdr = new XmlTextReader(message.BodyPart.Data);
			var xpathCollection = new XPathCollection();
			xpathCollection.Add(this.BatchedMessageXpath);
			XPathReader xpathReader = new XPathReader(xRdr, xpathCollection);
			int pos = 1;
			while (xpathReader.ReadUntilMatch())
			{
				while (xpathReader.NodeType != XmlNodeType.EndElement && xpathReader.Match(this.BatchedMessageXpath))
				{
					var messageCopy = pipelineContext.GetMessageFactory().CreateMessage();
					messageCopy.AddPart("Body", pipelineContext.GetMessageFactory().CreateMessagePart(), true);
					messageCopy.BodyPart.Data = new VirtualStream();
					using (var xWriter = XmlWriter.Create(messageCopy.BodyPart.Data, new XmlWriterSettings() { NewLineHandling = NewLineHandling.None }))
						xWriter.WriteRaw(xpathReader.ReadOuterXml());
					messageCopy.BodyPart.Data.Position = 0;
					messageCopy.Context = PipelineUtil.CloneMessageContext(message.Context);
					if (pos++ > 1)
						PipelineContextExtensions.WriteProperty<MessageTrackingID>(messageCopy.Context, Guid.NewGuid().ToString().ToUpper());
					yield return messageCopy;
				}
			}
		}

		internal virtual EnvelopContext CreateEnvelopContext(IBaseMessage wrappedMessage)
		{
			var providers = (SubServiceProviderIDs ?? string.Empty).Split(new []{ ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x));
			return CreateEnvelopContext(wrappedMessage, ServiceProviderID, new PartyResolver(ServiceProviderID) { SubServiceProviders = providers });
		}

		internal static EnvelopContext CreateEnvelopContext(IBaseMessage wrappedMessage, string serviceProviderID, PartyResolver partyResolver)
		{
			var serviceProvider = GetServiceProvider(serviceProviderID);

			var context = EnvelopContextFactory.Create(serviceProvider);

			if (context == null)
			{
				throw new ApplicationException(string.Format("ServiceProvider {0} exist in Enum but doesn't have implementation class", serviceProviderID));
			}

			context.Populate(wrappedMessage, partyResolver);
			return context;
		}

		internal static ServiceProvider GetServiceProvider(string serviceProviderID)
		{
			var serviceProviderIDCore = serviceProviderID.Split('_')[0];

			ServiceProvider serviceProvider;
			if (!Enum.TryParse<ServiceProvider>(serviceProviderIDCore, true, out serviceProvider))
			{
				throw new ApplicationException(string.Format("Ask developer to implement EnvelopContext for {0} ServiceProvider",
					serviceProviderIDCore));
			}

			return serviceProvider;
		}

		internal virtual void DisassembleInternalMessage(IPipelineContext pipelineContext, EnvelopContext envelopContext, IBaseMessage extractedMessage, IBaseMessage originalMessage, Schema messageSchema)
		{
			SubscriptionInfo[] subscriptions = null;
			var ffDasm = GetFFDisassembler();
			var pipelineHelpers = new PipelineHelpers();
			if (!extractedMessage.BodyPart.GetOriginalDataStream().CanSeek)
				pipelineHelpers.CreateSeekableMessageStream(pipelineContext, extractedMessage);
			var internalMessage = ffDasm.Disassemble(pipelineContext, extractedMessage, new SchemaWithNone(messageSchema.ToString()));

			if (ShouldSendRawMessage(pipelineContext, messageSchema, internalMessage, originalMessage, envelopContext))
			{
				return;
			}

			logger.Trace("Should not send raw message, go on processing internal message");
			PipelineContextExtensions.WriteProperty<BTS.SourceParty>(internalMessage.Context, envelopContext.SenderID);
			string internalMessageSchemaName = GetMessageBodySchemaName(envelopContext.MessageType, envelopContext.OriginalMessageType);
			if (!String.IsNullOrEmpty(internalMessageSchemaName)) PipelineContextExtensions.PromoteProperty<BTS.MessageType>(internalMessage.Context, internalMessageSchemaName);
			if (!String.IsNullOrWhiteSpace(envelopContext.ClientAWB))
			{
				try
				{
					var trackingID = PipelineContextExtensions.ReadPropertyString<MessageTrackingID>(extractedMessage.Context);
					DatabaseAccessHelpers.AccessDatabaseWithRetries(() => InsertSubscriptionValue("AIRAWB",
						envelopContext.SenderID, envelopContext.RecipientID ?? envelopContext.SenderID,
						$"{envelopContext.ClientAWB}-{trackingID}-Inbound", trackingID, "TrackingID"));
				}
				catch (Exception ex)
				{
					const string errorMsg = "Error creating Air Messaging Waybill subscription.";
					logger.Warn(errorMsg, ex);
					ReportErrorToIssueManager(extractedMessage, new Exception(errorMsg, ex));
				}
			}
			var recipients = new List<string> { envelopContext.RecipientID };
			var replacedBySubscription = false;
			if (ProcessSubscriptions)
			{
				pipelineHelpers.CreateSeekableMessageStream(pipelineContext, internalMessage);
				subscriptions = GetSubsriptionAccessor().SelectSubscriptions(internalMessage);
				replacedBySubscription = string.IsNullOrEmpty(envelopContext.RecipientID)
											? subscriptions.Any() && ReplaceRecipientOnSubscription
											: subscriptions.Any()
												&& (ReplaceRecipientOnSubscription
													^ (ReplaceRecipientOverride ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
														.Any(s => Regex.IsMatch(envelopContext.RecipientID, String.Format("^{0}$", s.Trim().Replace("*", ".*")))));
				recipients.AddRange(subscriptions.Select(x => x.Subscriber).Distinct());
			}
			for (int i = 0; i < recipients.Count; i++)
			{
				IBaseMessage outputMessage;
				var messageType = string.Empty;
				var trackingId = string.Empty;
				if (i == 0)
				{
					if (string.IsNullOrEmpty(envelopContext.RecipientID))
					{
						if (replacedBySubscription)
							continue;

						outputMessage = pipelineHelpers.CloneMessage(pipelineContext, internalMessage);
						string errorDescription = string.IsNullOrEmpty(envelopContext.RecipientPIMA)
							? "The message recipient can not be recognized."
							: string.Format("The message recipient can not be recognized, the recipientPIMA is {0}",
								envelopContext.RecipientPIMA);
						PipelineContextExtensions.PromoteProperty<ErrorReport.ErrorType>(outputMessage.Context, "FailedMessage");
						PipelineContextExtensions.PromoteProperty<ErrorReport.Description>(outputMessage.Context, errorDescription);
						PipelineContextExtensions.PromoteProperty<BTS.MessageType>(outputMessage.Context, "AlertMessage");
					}
					else
					{
						if (envelopContext.RecipientID == AirCargoMessagingId)
						{
							extractedMessage.BodyPart.GetOriginalDataStream().Position = 0;
							outputMessage = WrapMessageInXmlEnvelop(
								pipelineHelpers.CloneMessage(pipelineContext, extractedMessage),
								string.Format(AirCargoMessageHeader, envelopContext.SenderID, envelopContext.RecipientID),
								AirCargoMessageFooter,
								ref messageType);
							PipelineContextExtensions.PromoteProperty<BTS.MessageType>(outputMessage.Context, messageType);
							PipelineContextExtensions.WriteProperty<BTS.SourceParty>(outputMessage.Context, envelopContext.SenderID);
							recipients[0] = AirCargoMessagingId;
						}
						else if (replacedBySubscription)
						{
							continue;
						}
						else
						{
							outputMessage = internalMessage;
						}
					}
				}
				else
				{
					extractedMessage.BodyPart.GetOriginalDataStream().Position = 0;
					var prependSubscription = ProcessSubscriptions ? subscriptions.Where(x => x.Subscriber == recipients[i] && x.ReferenceType == "EnvelopePrepend") : null;
					var appendSubscription = ProcessSubscriptions ? subscriptions.Where(x => x.Subscriber == recipients[i] && x.ReferenceType == "EnvelopeAppend") : null;
					if (prependSubscription != null &&  prependSubscription.Any() &&  appendSubscription != null && appendSubscription.Any())
					{
						var header = string.Format(prependSubscription.First().Reference, envelopContext.SenderID, envelopContext.RecipientID ?? string.Empty);
						var footer = appendSubscription.First().Reference;
						outputMessage = WrapMessageInXmlEnvelop(pipelineHelpers.CloneMessage(pipelineContext, extractedMessage), header, footer, ref messageType); ;
						PipelineContextExtensions.PromoteProperty<BTS.MessageType>(outputMessage.Context, messageType);
						PipelineContextExtensions.WriteProperty<BTS.SourceParty>(outputMessage.Context, envelopContext.SenderID);
					}
					else
					{
						outputMessage = pipelineHelpers.CloneMessage(pipelineContext, internalMessage);
					}
					trackingId = Guid.NewGuid().ToString().ToUpper();
					PipelineContextExtensions.WriteProperty<MessageTrackingID>(outputMessage.Context, trackingId);
				}

				PipelineContextExtensions.WriteProperty<BTS.DestinationParty>(outputMessage.Context, recipients[i]);
					MessageQueue.Enqueue(outputMessage);
				PipelineContextExtensions.WriteProperty<OverrideFilename>(outputMessage.Context,
					GetMessageAggregationInformation(envelopContext) ?? string.Empty);
			
				logger.TraceFormat("DisassembleInternalMessage inside for loop, MessageType: {0}, SenderID: {1}, RecipientID: {2}, MessageTrackingID: {3}, OverrideEmailSubject: {4}, OverrideFilename: {5}, LoopIndex: {6}", messageType, envelopContext.SenderID, recipients[i], trackingId, envelopContext.RecipientID, envelopContext.Reference, i);
			}
		}

		internal bool ShouldSendRawMessage(IPipelineContext pipelineContext, Schema messageSchema, IBaseMessage message, IBaseMessage originalMessage, EnvelopContext envelopContext)
		{
			if (messageSchema.Equals(FMAFNAMessageSchema) && !string.IsNullOrEmpty(ISACXPath))
			{
				if (!message.BodyPart.GetOriginalDataStream().CanSeek)
				{
					var pipelineHelpers = new PipelineHelpers();
					pipelineHelpers.CreateSeekableMessageStream(pipelineContext, message);
				}
				var isacSection = PromotedValue.FindByXPath(message.BodyPart.GetOriginalDataStream(), ISACXPath);
				if (!string.IsNullOrEmpty(isacSection))
				{
					logger.Trace("Should send raw message, start InsertRawMessageToOutbox");
					InsertRawMessageToOutbox(originalMessage, pipelineContext, envelopContext);
					return true;
				}
			}

			return false;
		}

		internal virtual void InsertRawMessageToOutbox(IBaseMessage message, IPipelineContext pipelineContext, EnvelopContext envelopContext)
		{
			if (string.IsNullOrEmpty(envelopContext.RecipientID)) throw new ApplicationException("The message recipient can not be recognized.");

			var trackingId = PipelineContextExtensions.ReadPropertyString<MessageTrackingID>(message.Context);
			var senderId = PipelineContextExtensions.ReadPropertyString<BTS.SourceParty>(message.Context);
			string emailSubject = PipelineContextExtensions.ReadPropertyString<OverrideEmailSubject>(message.Context) ?? string.Empty;
			string fileName = GetMessageAggregationInformation(envelopContext) ?? string.Empty;

			logger.TraceFormat("InsertToOutbox started SenderId: {0}, RecipientId: {1}, EmailSubject: {2}, Filename: {3}, MessageTrackingID: {4}", senderId, envelopContext.RecipientID, emailSubject, fileName, trackingId);
			GetOutboxAccessor().InsertToOutbox(senderId, Guid.Empty, new eHubGatewayMessage()
			{
				MessageTrackingID = new Guid(trackingId),
				ClientID = envelopContext.RecipientID,
				ApplicationCode = "BIZ",
				SchemaName = !string.IsNullOrEmpty(RawMessageType) ? RawMessageType : envelopContext.MessageType,
				SchemaType = MessageSchemaType.FlatFile,
				EmailSubject = emailSubject,
				FileName = fileName,
				MessageStream = message.BodyPart.GetOriginalDataStream().CompressAndEncode()
			}, new Guid(trackingId));
			logger.Trace("InsertToOutbox ended");
		}

		internal static IBaseMessage GetInternalMessage(IPipelineContext pipelineContext, IBaseMessage message, string internalMessage)
		{
			var result = pipelineContext.GetMessageFactory().CreateMessage();
			result.Context = PipelineUtil.CloneMessageContext(message.Context);
			var internalMessagePart = pipelineContext.GetMessageFactory().CreateMessagePart();
			string partName = string.Empty;
			message.GetPartByIndex(0, out partName);
			result.AddPart(partName, internalMessagePart, true);
			result.BodyPart.Charset = message.BodyPart.Charset;
			result.BodyPart.ContentType = message.BodyPart.ContentType;

			var stream = new VirtualStream();
			var writer = new StreamWriter(stream);
			writer.Write(internalMessage);
			writer.Flush();
			stream.SeekBegin();
			result.BodyPart.Data = stream;
			message.BodyPart.Data.SeekBegin();

			return result;
		}

		Schema GetMessageBodySchema(string messageType, string originalMessageType)
		{
			switch (messageType)
			{
				case "FSU":
				case "FSA":return FSUMessageSchema;
				case "FNA": 
				case "FMA":
					return originalMessageType == "CMD" ? CMDFMAFNAMessageSchema : FMAFNAMessageSchema;
				case "CMA": return CMDCMAMessageSchema;
				case "FFAFMAFNA": return FFAFMAFNAMessageSchema;
				default:
					return null;
			}
		}

		string GetMessageBodySchemaName(string messageType, string originalMessageType)
		{
			switch (messageType)
			{
				case "FSU":
				case "FSA":
					return "http://cargowise.com/ehub/clients/edi/2010/12#FSU_FSA12"; ;
				case "FNA":
				case "FMA":
					return originalMessageType == "CMD"
						? "http://cargowise.com/ehub/clients/edi/2011/09#CMD_FMA_FNA"
						: "http://cargowise.com/ehub/clients/edi/2011/09#FMA_FNA";
				case "CMA": return "http://cargowise.com/ehub/clients/edi/2011/09#CMD_CMA";
				case "FFAFMAFNA": return "http://www.wisetechglobal.com/ehub/clients/edi/2011/09#FFA_FMA_FNA";
				default:
					return null;
			}
		}

		internal string GetMessageAggregationInformation(EnvelopContext envelopContext)
		{
			return $"{envelopContext.MessageType}_{envelopContext.ClientAWB}__{GetAirlineCode(envelopContext.ClientAWB)}_{envelopContext.RecipientPIMA}";
		}

		string GetAirlineCode(string clientAWB)
		{
			return string.IsNullOrWhiteSpace(clientAWB) ? "" : GetPartyAccessor().GetAirlineCodeFromPrefix(clientAWB.Substring(0, 3));
		}

		internal virtual IFFDisassembleHelper GetFFDisassembler()
		{
			return new FFDisassembleHelper();
		}

		internal virtual IInboxAccessor GetInboxAccessor() => GetAccessorInstance(DataAccessFactories.NewInboxAccessorInstance);
		internal virtual IOutboxAccessor GetOutboxAccessor() => GetAccessorInstance(DataAccessFactories.NewOutboxAccessorInstance);
		internal virtual IExceptionsAccessor GetExceptionsAccessor() => GetAccessorInstance(DataAccessFactories.NewExceptionsAccessorInstance);
		internal virtual ISubscriptionAccessor GetSubsriptionAccessor() => GetAccessorInstance(DataAccessFactories.NewSubscriptionAccessorInstance);
		internal virtual IPartyAccessor GetPartyAccessor() => GetAccessorInstance(DataAccessFactories.NewPartyAccessorInstance);

		private I GetAccessorInstance<I>(Func<I> accessorFactory)
		{
			try
			{
				return accessorFactory();
			}
			catch (ConfigurationErrorsException e) when (e.Message.EndsWith("could not be found in .config file."))
			{
				logger.ErrorFormat("Exception creating instance of {0}. Will refresh appSettings and retry.", e, typeof(I).Name);
				ConfigurationManager.RefreshSection("appSettings");
				return accessorFactory();
			}
		}

		internal virtual eHubTransactionsContext GetEHubTransactionsContext()
		{
			return new eHubTransactionsContext();
		}

		public virtual void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value, string reference, string referenceType)
		{
			logger.TraceFormat("InsertSubscriptionValue SubscriptionType: {0}, ProviderID: {1}, SubscriberID: {2}, Value: {3}, Reference: {4}, ReferenceType: {5}", subscriptionType, providerID, subscriberID, value, reference, referenceType);

			using (var context = GetEHubTransactionsContext())
			{
				var subType = (from s in context.eHubSubscriptionTypes where s.ST_ID == subscriptionType select s).First();
				var provider = (from c in context.eHubClients where c.CC_ID == providerID select new { c.CC_PK }).First();
				var subscriber = (from c in context.eHubClients where c.CC_ID == subscriberID select new { c.CC_PK }).First();
				var subValue = new eHubSubscriptionValue
				{
					SV_PK = Guid.NewGuid(),
					SV_ST = subType.ST_PK,
					SV_CC_Sender = provider.CC_PK,
					SV_CC_Recipient = subscriber.CC_PK,
					SV_Value = value,
					SV_Reference = reference,
					SV_ReferenceType = referenceType,
					SV_SubscribedUTC = DateTime.UtcNow,
					SV_ExpiryUTC = subType.ST_ExpiryDays.HasValue ? DateTime.UtcNow.AddDays(subType.ST_ExpiryDays.Value) : (DateTime?)null
				};
				context.eHubSubscriptionValues.Add(subValue);
				context.SaveChanges();
			}
		}

		IBaseMessage WrapMessageInXmlEnvelop(IBaseMessage message, string header, string footer, ref string messageType)
		{
			var content = System.Security.SecurityElement.Escape(message.BodyPart.GetOriginalDataStream().ReadToEnd());
			var resultStream = new BiztalkVirtualStream(BiztalkVirtualStream.MemoryFlag.AutoOverFlowToDisk);
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;
			XmlWriter writter = XmlWriter.Create(resultStream, settings);
			var envelopDoc = XDocument.Parse(header + content + footer);
			envelopDoc.WriteTo(writter);
			writter.Flush();

			resultStream.SeekBegin();
			message.BodyPart.Data = resultStream;

			messageType = string.Format("{0}{1}{2}", envelopDoc.Root.Name.Namespace, string.IsNullOrWhiteSpace(envelopDoc.Root.Name.Namespace.ToString()) ? string.Empty : "#", envelopDoc.Root.Name.LocalName);

			return message;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("6473A2BB-373D-4035-97DD-BFF30257C229");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, "Enabled", errorLog);
			if (var != null) Enabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "ServiceProviderID", errorLog);
			if (var != null) ServiceProviderID = (string)var;

			var = LoadProperty(propertyBag, "SubServiceProviderIDs", errorLog);
			if (var != null) SubServiceProviderIDs = (string)var;

			var = LoadProperty(propertyBag, "EnvelopeMessageSchema", errorLog);
			if (var != null) EnvelopeMessageSchema = new Schema((string)var);

			var = LoadProperty(propertyBag, "FSUMessageSchema", errorLog);
			if (var != null) FSUMessageSchema = new Schema((string)var);
				
			var = LoadProperty(propertyBag, "CMDFMAFNAMessageSchema", errorLog);
			if (var != null) CMDFMAFNAMessageSchema = new Schema((string)var);

			var = LoadProperty(propertyBag, "FMAFNAMessageSchema", errorLog);
			if (var != null) FMAFNAMessageSchema = new Schema((string)var);

			var = LoadProperty(propertyBag, "CMDCMAMessageSchema", errorLog);
			if (var != null) CMDCMAMessageSchema = new Schema((string)var);

			var = LoadProperty(propertyBag, "BatchedMessageXpath", errorLog);
			if (var != null) BatchedMessageXpath = (string)var;

			var = LoadProperty(propertyBag, "ProcessSubscriptions", errorLog);
			if (var != null) ProcessSubscriptions = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "NormalizeLineEndings", errorLog);
			if (var != null) NormalizeLineEndings = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "ReplaceRecipientOnSubscription", errorLog);
			if (var != null) ReplaceRecipientOnSubscription = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "ReplaceRecipientOverride", errorLog);
			if (var != null) ReplaceRecipientOverride = (string)var;

			var = LoadProperty(propertyBag, "FFAFMAFNAMessageSchema", errorLog);
			if (var != null) FFAFMAFNAMessageSchema = new Schema((string)var);

			var = LoadProperty(propertyBag, "ISACXPath", errorLog);
			if (var != null) ISACXPath = (string)var;

			var = LoadProperty(propertyBag, "RawMessageType", errorLog);
			if (var != null) RawMessageType = (string)var;
		}

		object LoadProperty(IPropertyBag propertyBag, string propertyName, int errorLog)
		{
			object result = null;
			try
			{
				propertyBag.Read(propertyName, out result, errorLog);
			}
			catch { }
			return result;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);

			val = ServiceProviderID;
			propertyBag.Write("ServiceProviderID", ref val);

			val = SubServiceProviderIDs;
			propertyBag.Write("SubServiceProviderIDs", ref val);

			val = EnvelopMessageSchemaSpecified ? EnvelopeMessageSchema.ToString() : string.Empty;
			propertyBag.Write("EnvelopeMessageSchema", ref val);

			val = FSUMessageSchemaSpecified ? FSUMessageSchema.ToString() : string.Empty;
			propertyBag.Write("FSUMessageSchema", ref val);

			val = FMAFNAMessageSchemaSpecified ? FMAFNAMessageSchema.ToString() : string.Empty;
			propertyBag.Write("FMAFNAMessageSchema", ref val);

			val = CMDCMAMessageSchemaSpecified ? CMDCMAMessageSchema.ToString() : string.Empty;
			propertyBag.Write("CMDCMAMessageSchema", ref val);

			val = CMDFMAFNAMessageSchemaSpecified ? CMDFMAFNAMessageSchema.ToString() : string.Empty;
			propertyBag.Write("CMDFMAFNAMessageSchema", ref val);

			val = FFAFMAFNAMessageSchemaSpecified ? FFAFMAFNAMessageSchema.ToString() : string.Empty;
			propertyBag.Write("FFAFMAFNAMessageSchema", ref val);

			val = BatchedMessageXpath;
			propertyBag.Write("BatchedMessageXpath", ref val);

			val = ProcessSubscriptions;
			propertyBag.Write("ProcessSubscriptions", ref val);

			val = NormalizeLineEndings;
			propertyBag.Write("NormalizeLineEndings", ref val);

			val = ReplaceRecipientOnSubscription;
			propertyBag.Write("ReplaceRecipientOnSubscription", ref val);

			val = ReplaceRecipientOverride;
			propertyBag.Write("ReplaceRecipientOverride", ref val);

			val = ISACXPath;
			propertyBag.Write("ISACXPath", ref val);

			val = RawMessageType;
			propertyBag.Write("RawMessageType", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public string ServiceProviderID { get; set; }
		public string SubServiceProviderIDs { get; set; }

		public Schema EnvelopeMessageSchema { get; set; }
		public string BatchedMessageXpath { get; set; }
		public Schema FSUMessageSchema { get; set; }
		public Schema FMAFNAMessageSchema { get; set; }
		public Schema CMDFMAFNAMessageSchema { get; set; }
		public Schema CMDCMAMessageSchema { get; set; }
		public Schema FFAFMAFNAMessageSchema { get; set; }
		public bool ProcessSubscriptions { get; set; }
		public bool NormalizeLineEndings { get; set; }
		public bool ReplaceRecipientOnSubscription { get; set; }
		public string ReplaceRecipientOverride { get; set; }
		public string ISACXPath { get; set; }
		public string RawMessageType { get; set; }
		bool EnvelopMessageSchemaSpecified
		{
			get { return (EnvelopeMessageSchema != null && !String.IsNullOrEmpty(EnvelopeMessageSchema.AssemblyName)); }
		}

		bool FSUMessageSchemaSpecified
		{
			get { return (FSUMessageSchema != null && !String.IsNullOrEmpty(FSUMessageSchema.AssemblyName)); }
		}

		bool FMAFNAMessageSchemaSpecified
		{
			get { return (FMAFNAMessageSchema != null && !String.IsNullOrEmpty(FMAFNAMessageSchema.AssemblyName)); }
		}

		bool CMDFMAFNAMessageSchemaSpecified
		{
			get { return (CMDFMAFNAMessageSchema != null && !String.IsNullOrEmpty(CMDFMAFNAMessageSchema.AssemblyName)); }
		}

		bool CMDCMAMessageSchemaSpecified
		{
			get { return (CMDCMAMessageSchema != null && !String.IsNullOrEmpty(CMDCMAMessageSchema.AssemblyName)); }
		}

		bool FFAFMAFNAMessageSchemaSpecified
		{
			get { return (FFAFMAFNAMessageSchema != null && !String.IsNullOrEmpty(FFAFMAFNAMessageSchema.AssemblyName)); }
		}

		#endregion

		Lazy<StreamWrapperComponent> streamWrapper = new Lazy<StreamWrapperComponent>();

		public IBaseMessage GetNext(IPipelineContext pipelineContext)
		{
			IBaseMessage message = null;
			LoggerHelpers.LogComponentStart(logger, this);

			if (MessageQueue.Count > 0)
			{
				message = MessageQueue.Dequeue() as IBaseMessage;
				logger.TraceFormat("Dequeuing message to pipeline with MessageID: {0}", message.MessageID);
			}
			else
			{
				logger.Trace("End of queued messages");
			}

			LoggerHelpers.LogComponentEnd(logger, this);
			return message;
		}

		Queue MessageQueue
		{
			get { return messageQueue ?? (messageQueue = new Queue()); }
		}
		Queue messageQueue;

		internal ILog logger = new NoOpLogger();
		internal virtual ILog GetPipelineLogger(IBaseMessage message) => LoggerHelpers.GetPipelineLogger(message);

		private static readonly string issueMgrUri = ConfigurationManager.AppSettings["IssueManagerUri"];
		private string receiveLocation;
		private string sender;

		internal virtual IErrorReportingClient GetErrorReportingClient() => new ErrorReportingClient(new Uri(issueMgrUri));
		const string IssueMessageDelimiter = "\r\n----------BIZTALK PROPERTIES----------\r\n";

		private const string AirCargoMessagingId = "AIR_CARGO_MESSAGING";
		private const string AirCargoMessageHeader = "<ns0:AirCargoMessage xmlns:ns0=\"http://CargoWise.eHub.Products.AirMessaging.Schemas.AirCargoMessaging\"><Header><SenderID>{0}</SenderID><RecipientID>{1}</RecipientID></Header><Body>";
		private const string AirCargoMessageFooter = "</Body></ns0:AirCargoMessage>";
	}
}
