using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	interface IMessageHelper
	{
		bool IsAutoSubscriptionRequired(IBaseMessage message);
		void InsertAutoSubscriptionsForSender(IBaseMessage message);
		void EnqueueMessage(IPipelineContext pipelineContext, Queue messageQueue, IBaseMessage message, IBaseMessage uncompressedMessage, bool processSubscriptions, bool discardUnsubscribed);
		List<string> GetSubscribers(IBaseMessage message);
	}

	internal class MessageHelper : IMessageHelper
	{
		public bool IsAutoSubscriptionRequired(IBaseMessage message)
		{
			var subsAccessor = GetSubscriptionAccessor();
			return subsAccessor.IsAutoSubscriptionRequired(message);
		}

		public void InsertAutoSubscriptionsForSender(IBaseMessage message)
		{
			var subsAccessor = GetSubscriptionAccessor();
			subsAccessor.InsertAutoSubscriptionsForSender(message);
		}

		public void EnqueueMessage(IPipelineContext pipelineContext, Queue messageQueue, IBaseMessage message, IBaseMessage uncompressedMessage, bool processSubscriptions, bool discardUnsubscribed)
		{
			long len = uncompressedMessage.BodyPart.GetOriginalDataStream().GetLengthEx();
			if (len > 0)
			{
				message.Context.WriteProperty<UncompressedLength>(len);
			}

			if (processSubscriptions)
			{
				var subscribers = GetSubscribers(message);
				if (subscribers.Count > 0)
					message.Context.WriteProperty<BTS.DestinationParty>(subscribers[0]);

				EnqueueMessageWithRouting(pipelineContext, messageQueue, message, discardUnsubscribed);

				for (int i = 1; i < subscribers.Count; i++)
				{
					var messageCopy = CloneMessage(pipelineContext, message);
					messageCopy.Context.WriteProperty<BTS.DestinationParty>(subscribers[i]);
					EnqueueMessageWithRouting(pipelineContext, messageQueue, messageCopy, discardUnsubscribed);
				}
			}
			else
				EnqueueMessageWithRouting(pipelineContext, messageQueue, message, discardUnsubscribed);
		}

		void EnqueueMessageWithRouting(IPipelineContext pipelineContext, Queue messageQueue, IBaseMessage message, bool discardUnsubscribed)
		{
			string senderID = message.Context.ReadPropertyString<BTS.SourceParty>();
			string recipientID = message.Context.ReadPropertyString<BTS.DestinationParty>();
			if (!String.IsNullOrWhiteSpace(recipientID))
			{
				using (var dbContext = GetDBContext())
				{
					var logger = LoggerHelpers.GetPipelineLogger(message, "RoutingRuleEngine");
					var ruleFactory = new RoutingRuleFactory(dbContext, logger);
					var routingRule = ruleFactory.GetForReading(recipientID);
					if (routingRule != null)
					{
						var factResolver = new RoutingRuleMessageFactResolver(message);
						var sqlResolver = new RoutingRuleSqlFactResolver(dbContext);
						var results = routingRule.Evaluate(dbContext, new IFactResolver[] { factResolver, sqlResolver }, logger);
						if (results.Count > 0)
						{
							for (int i = 0; i < results.Count; i++)
							{
								var routedMessage = i == 0 ? message : CloneMessage(pipelineContext, message);
								if (results[i].Value == "ERROR")
								{
									routedMessage.Context.PromoteProperty<ErrorReport.ErrorType>("FailedMessage");
									routedMessage.Context.WriteProperty<ErrorReport.Description>(results[i].ErrorDescription);
									routedMessage.Context.PromoteProperty<ErrorReport.ReceivePortName>(routedMessage.Context.ReadPropertyString<BTS.ReceivePortName>());
									routedMessage.Context.WriteProperty<BTS.MessageType>("AlertMessage");
								}
								else if (results[i].Recipient != null)
									routedMessage.Context.WriteProperty<BTS.DestinationParty>(results[i].Recipient.CC_ID);
								else if (results[i].Recipient == null && (!String.IsNullOrWhiteSpace(results[i].ErrorCode) || !String.IsNullOrWhiteSpace(results[i].ErrorDescription)))
								{
									routedMessage.Context.WriteProperty<BTS.SourceParty>(recipientID);
									routedMessage.Context.WriteProperty<BTS.DestinationParty>(senderID);
								}
								if (!String.IsNullOrWhiteSpace(results[i].ErrorCode))
									routedMessage.Context.WriteProperty<CargoWise.eHub.Core.PropertySchemas.ErrorCode>(results[i].ErrorCode);
								if (!String.IsNullOrWhiteSpace(results[i].ErrorDescription))
									routedMessage.Context.WriteProperty<CargoWise.eHub.Core.PropertySchemas.ErrorDescription>(results[i].ErrorDescription);
								EnqueueMessage(messageQueue, routedMessage);
							}
						}
						else
						{
							if (!discardUnsubscribed)
								EnqueueMessage(messageQueue, message);
						}
					}
					else
					{
						EnqueueMessage(messageQueue, message);
					}
				}
			}
			else
			{
				if (!discardUnsubscribed)
					EnqueueMessage(messageQueue, message);
			}
		}

		private static IBaseMessage CloneMessage(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (!message.BodyPart.Data.CanSeek)
				message.BodyPart.Data = new ReadOnlySeekableStream(message.BodyPart.Data, new VirtualStream());
			var messageCopy = pipelineContext.GetMessageFactory().CreateMessage();
			messageCopy.AddPart("Body", pipelineContext.GetMessageFactory().CreateMessagePart(), true);
			messageCopy.BodyPart.Data = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
			messageCopy.Context = PipelineUtil.CloneMessageContext(message.Context);
			message.BodyPart.Data.Position = 0;
			message.BodyPart.Data.CopyTo(messageCopy.BodyPart.Data);
			message.BodyPart.Data.Position = 0;
			messageCopy.BodyPart.Data.Position = 0;
			return messageCopy;
		}

		void EnqueueMessage(Queue messageQueue, IBaseMessage message)
		{
			if (MessageTrackingIDList.Contains(message.Context.ReadPropertyString<MessageTrackingID>()))
			{
				message.Context.PromoteProperty<MessageTrackingID>(Guid.NewGuid().ToString().ToUpper());
			}
			MessageTrackingIDList.Add(message.Context.ReadPropertyString<MessageTrackingID>());
			messageQueue.Enqueue(message);
		}

		public List<string> GetSubscribers(IBaseMessage message)
		{
			var persistingStream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
			message.BodyPart.Data = new ReadOnlySeekableStream(message.BodyPart.GetOriginalDataStream(), persistingStream);

			var subsAccessor = GetSubscriptionAccessor();
			var subscribers = subsAccessor.SelectSubscribedClients(message);

			return new List<string>(subscribers);
		}

		List<string> MessageTrackingIDList
		{
			get { return messageTrackingIDList ?? (messageTrackingIDList = new List<string>()); }
		}
		List<string> messageTrackingIDList;

		internal virtual ISubscriptionAccessor GetSubscriptionAccessor()
		{
			return DataAccessFactories.NewSubscriptionAccessorInstance();
		}

		internal virtual eHubTransactionsContext GetDBContext()
		{
			Database.SetInitializer<eHubTransactionsContext>(null);
			return new eHubTransactionsContext();
		}
	}
}
