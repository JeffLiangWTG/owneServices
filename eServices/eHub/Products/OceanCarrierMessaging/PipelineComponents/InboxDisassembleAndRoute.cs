using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Xml;
using System.Xml.XPath;
using CargoWise.eHub.Common;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Common.Logging;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using eServices.Routing.OceanCarrierMessaging;
using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.PipelineComponents
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[System.Runtime.InteropServices.Guid("ca53f1dc-e599-4775-aaf9-624074a852b8")]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	public class InboxDisassembleAndRoute : ComponentBase, IDisassemblerComponent
	{
		protected override Guid ClassID
		{
			get { return new Guid("161dc4d4-cedd-47c6-a49a-d8697395f812"); }
		}

		protected override string DisplayName
		{
			get { return "Inbox Disassemble and Route"; }
		}

		readonly Queue<IBaseMessage> messageQueue = new Queue<IBaseMessage>();
		const string NsUri = "http://schemas.microsoft.com/Sql/2008/05/TypedPolling/SelectInboxMessagesByStatus";
		const string Client_OCM_Splitting = "OCM_Splitting";
		const string Client_OCM_Copying = "OCM_MutipleRecipientsCopying";

		ILog logger;

		public string NextStep { get; set; }
		internal Queue<IBaseMessage> MessageQueue
		{
			get { return messageQueue; }
		}

		private string GetNamespaceUrl(XPathNavigator navigator, string localName)
		{
			LoggerHelpers.LogComponentStart(logger, this);
			var iterator = navigator.Select(@"(//*[local-name()='" + localName + "'])[1]");
			return iterator.MoveNext() ? iterator.Current.NamespaceURI : string.Empty;
		}

		public void Disassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			logger = GetLogger(pInMsg);

			try
			{
				var pipelineHelpers = new PipelineHelpers();
				string senderId;
				string recipientId;
				string destinationParty;

				var msgStream = ExtractMessage(pContext, pInMsg, out senderId, out recipientId);
				using (var context = GetDbContext())
				using (var xpStream = new VirtualStream())
				{
					msgStream.CopyTo(xpStream);
					msgStream.Position = xpStream.Position = 0;
					var xpDoc = new XPathDocument(xpStream);
					var xpNav = xpDoc.CreateNavigator();
					xpNav.MoveToFirstChild();
					pInMsg.Context.PromoteProperty<BTS.MessageType>(xpNav.NamespaceURI + '#' + xpNav.LocalName);

					var uniShipmentNamespace = GetNamespaceUrl(xpNav, "UniversalShipment");
					if (!string.IsNullOrEmpty(uniShipmentNamespace) && CargoWiseOneToCargoWiseOneMessageTypes.MessageTypes.TryGetValue(uniShipmentNamespace,
							out destinationParty))
					{
						pInMsg.Context.WriteProperty<BTS.DestinationParty>(destinationParty);
						IBaseMessage originalMsg = XmlDisassemble(pContext, pInMsg);
						pipelineHelpers.CreateSeekableMessageStream(pContext, originalMsg, logger);
						messageQueue.Enqueue(originalMsg);
					}
					else
					{
						xpNav.MoveToRoot();

						var xpathFactResolver = new RoutingRuleXpathNavFactResolver(xpNav);
						var propertyFactResolver = new RoutingRulePropertyFactResolver(pInMsg);
						var sqlFactResolver = new RoutingRuleSqlFactResolver(context);

						var ruleFactory = new RoutingRuleFactory(context, logger);
						var resolvers = new IFactResolver[] { xpathFactResolver, propertyFactResolver, sqlFactResolver };

						var ocmSplittingResult = RoutingRuleEvaluation.EvaluateOCM(context, ruleFactory, Client_OCM_Splitting, resolvers, logger)[0];

						IBaseMessage outMsg = XmlDisassemble(pContext, pInMsg);
						pipelineHelpers.CreateSeekableMessageStream(pContext, outMsg, logger);

						if (ocmSplittingResult?.Recipient != null) // BlackList & WhiteList for splitting
						{
							WriteMessageProperties(outMsg, ocmSplittingResult, recipientId, senderId);
							messageQueue.Enqueue(outMsg);
						}
						else
						{
							var results = RoutingRuleEvaluation.EvaluateOCM(context, ruleFactory, Client_OCM_Copying, recipientId, resolvers, logger);
							WriteMessageProperties(outMsg, results[1], recipientId, senderId);
							messageQueue.Enqueue(outMsg);

							if (results.Count == 3) // Copying the message
							{
								var msgCopy = CopyMessageWithContent(pContext, outMsg, senderId);

								WriteMessageProperties(msgCopy, results[2], recipientId, senderId);
								messageQueue.Enqueue(msgCopy);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Warn("Message processing failure: ", ex);
				throw;
			}
			finally
			{
				logger.Info("Finished processing message");
				LoggerHelpers.LogComponentEnd(logger, this);
			}
		}

		private static IBaseMessage CopyMessageWithContent(IPipelineContext pContext, IBaseMessage pInMsg, string senderId)
		{
			var msgCopy = pContext.GetMessageFactory().CreateMessage();
			msgCopy.AddPart("Body", pContext.GetMessageFactory().CreateMessagePart(), true);
			msgCopy.BodyPart.Data = new VirtualStream();
			msgCopy.Context = PipelineUtil.CloneMessageContext(pInMsg.Context);
			msgCopy.Context.WriteProperty<BTS.SourceParty>(senderId);
			msgCopy.Context.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", null);
			msgCopy.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", Guid.NewGuid().ToString());
			long dataStartPos = pInMsg.BodyPart.Data.Position;
			pInMsg.BodyPart.Data.CopyTo(msgCopy.BodyPart.Data);
			pInMsg.BodyPart.Data.Position = dataStartPos;
			msgCopy.BodyPart.Data.Position = 0;
			return msgCopy;
		}

		public IBaseMessage GetNext(IPipelineContext pContext)
		{
			IBaseMessage message = null;
			LoggerHelpers.LogComponentStart(logger, this);

			if (messageQueue.Count > 0)
			{
				message = messageQueue.Dequeue();
				logger.InfoFormat("Dequeuing message to pipeline with MessageID: {0}", message.MessageID);
			}
			else
				logger.Info("End of queued messages");

			LoggerHelpers.LogComponentEnd(logger, this);
			return message;
		}

		private static void WriteMessageProperties(IBaseMessage pInMsg, Result result, string recipientId, string senderId)
		{
			if (result.Recipient != null)
			{
				pInMsg.Context.WriteProperty<BTS.DestinationParty>(result.Recipient.CC_ID);
			}
			else if (result.Recipient == null && (!string.IsNullOrWhiteSpace(result.ErrorCode) ||
													!string.IsNullOrWhiteSpace(result.ErrorDescription)))
			{
				pInMsg.Context.WriteProperty<BTS.SourceParty>(recipientId);
				pInMsg.Context.WriteProperty<BTS.DestinationParty>(senderId);
			}
			if (!string.IsNullOrWhiteSpace(result.ErrorCode))
				pInMsg.Context.WriteProperty<ErrorCode>(result.ErrorCode);
			if (!string.IsNullOrWhiteSpace(result.ErrorDescription))
				pInMsg.Context.WriteProperty<ErrorDescription>(result.ErrorDescription);
		}

		private VirtualStream ExtractMessage(IPipelineContext pContext, IBaseMessage msg, out string senderId,
			out string recipientId)
		{
			LoggerHelpers.LogComponentStart(logger, this);
			logger.DebugFormat("Processing message with BizTalk MessageID: {0}", msg.MessageID);

			var msgStream = new VirtualStream();
			pContext.ResourceTracker.AddResource(msgStream);

			string inboxPk, messageTrackingId, envelopeTrackingId, email, fileName;

			using (var reader = XmlReader.Create(msg.BodyPart.GetOriginalDataStream()))
			{
				reader.MoveToContent();
				reader.ReadToDescendant("InboxPK", NsUri);
				inboxPk = reader.ReadString();
				reader.ReadToNextSibling("MessageTrackingID", NsUri);
				messageTrackingId = reader.ReadString();

				logger.InfoFormat("Processing inbox message. InboxPK: {0} MessageTrackingId: {1}", inboxPk,
					messageTrackingId);

				reader.ReadToNextSibling("EnvelopeTrackingID", NsUri);
				envelopeTrackingId = reader.ReadString();
				reader.ReadToNextSibling("SenderID", NsUri);
				senderId = reader.ReadString();
				reader.ReadToNextSibling("RecipientID", NsUri);
				recipientId = reader.ReadString();
				reader.ReadToNextSibling("EmailSubject", NsUri);
				email = reader.ReadString();
				reader.ReadToNextSibling("FileName", NsUri);
				fileName = reader.ReadString();

				reader.ReadToNextSibling("Content", NsUri);
				using (var decoder = new BufferReaderStream(reader.ReadElementContentAsBase64))
				using (var decompressor = new GZipStream(decoder, CompressionMode.Decompress))
					decompressor.CopyTo(msgStream);
			}
			msgStream.Position = 0;
			msg.BodyPart.Data = msgStream;

			msg.Context.WriteProperty<InternalTrackingID>(inboxPk);
			msg.Context.WriteProperty<MessageTrackingID>(messageTrackingId);
			msg.Context.WriteProperty<EnvelopeTrackingID>(envelopeTrackingId);
			msg.Context.WriteProperty<BTS.SourceParty>(senderId);
			msg.Context.WriteProperty<BTS.DestinationParty>(recipientId);
			msg.Context.WriteProperty<OverrideEmailSubject>(email);
			msg.Context.WriteProperty<OverrideFilename>(fileName);
			msg.Context.WriteProperty<UncompressedLength>(msgStream.Length.ToString());
			msg.Context.PromoteProperty<NextStep>(NextStep);
			return msgStream;
		}

		internal virtual IBaseMessage XmlDisassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			var xmlDasm = new XmlDasmComp { AllowUnrecognizedMessage = true };
			xmlDasm.Disassemble(pContext, pInMsg);
			return xmlDasm.GetNext(pContext);
		}

		internal static Func<IBaseMessage, ILog> GetLogger = (pInMsg) => LoggerHelpers.GetPipelineLogger(pInMsg);
		internal static Func<eHubTransactionsContext> GetDbContext = () => new eHubTransactionsContext();
	}


}