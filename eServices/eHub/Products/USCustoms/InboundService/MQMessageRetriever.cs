using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.eHub.Common;
using CargoWise.eServices.USCustoms.Integration;
using CargoWise.eServices.USCustoms.MQConfiguration;
using Common.Logging;
using IBM.WMQ;
using ServiceBroker.Common;

namespace CargoWise.eServices.USCustoms.InboundService
{
	public interface IMQMessageRetriever : IDisposable
	{
		Stream Retrieve();
		void RetrieveSafe();
		void BeginTransaction();
		void CommitTransaction();
		void RollbackTransaction();
	}

	public class MQMessageRetriever : IMQMessageRetriever
	{
		public MQMessageRetriever(IMQConfiguration config, ILog logger, ILog messageLogger, string messageType)
		{
			if (config == null) throw new ArgumentNullException("config");
			this.config = config;

			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;

			if (messageLogger == null) throw new ArgumentNullException("messageLogger");
			this.messageLogger = messageLogger;

			if (messageType == null) throw new ArgumentNullException("messageType");
			this.messageType = messageType;
		}

		public void RetrieveSafe()
		{
			var messages = RetrieveMessagesFromMQ(isSafe: true);
			if (messages.Length == 0) return;
			throw new FailedMQMessageException(string.Format("RetrieveSafe read {0} messages from {1}", messages.Length, config.QueueName), messages);
		}

		public Stream Retrieve()
		{
			var messages = RetrieveMessagesFromMQ();
			if (messages.Length == 0) return null;
			return Validate(messages) ? CombineMessagesIntoStream(messages) : null;
		}

		Stream CombineMessagesIntoStream(IEnumerable<MQMessage> messages)
		{
			var result = new VirtualStream();
			var writer = XmlWriter.Create(result, new XmlWriterSettings() { OmitXmlDeclaration = true });
			writer.WriteStartElement(Constants.MessageAttributes.StartElement);
			writer.WriteAttributeString(Constants.MessageAttributes.IsProduction, ConfigurationManager.AppSettings.Get(Constants.MessageAttributes.IsProduction));
			writer.WriteAttributeString(Constants.MessageAttributes.MessageType, messageType);

			var contentStream = new VirtualStream();
			var contentWriter = new StreamWriter(contentStream);

			foreach (var message in messages) WriteMessageToStream(message, contentWriter);

			contentWriter.Flush();
			LogMessageContent(contentStream);

			writer.WriteRaw("<![CDATA[");
			new StreamReader(contentStream.CompressAndEncode()).WriteToXmlWriter(writer);
			writer.WriteRaw("]]>");
			writer.WriteEndElement();
			writer.Flush();
			result.Position = 0;

			if (logger.IsTraceEnabled) logger.Trace(string.Format("End joining {0} messages", messages.Count()));
			return result;
		}

		void LogMessageContent(Stream contentStream)
		{
			if (messageLogger.IsTraceEnabled) messageLogger.Trace(contentStream.ReadToEnd());
		}

		void WriteMessageToStream(MQMessage message, StreamWriter contentWriter)
		{
			int messageLength = message.MessageLength;
			while (messageLength != 0)
			{
				var length = Math.Min(50000, messageLength);
				string messageText = message.ReadString(length).Replace('\0', ' ');

				if (!string.IsNullOrEmpty(messageText))
				{
					contentWriter.Write(messageText);
				}
				messageLength -= length;
			}
		}

		static bool Validate(MQMessage[] messages)
		{
			if (messages[0].Format != MQC.MQFMT_STRING)
			{
				throw new FailedMQMessageException("A message with an unknown format was received " + messages[0].Format, messages);
			}

			return true;
		}

		MQMessage[] RetrieveMessagesFromMQ(bool isSafe = false)
		{
			if (queue == null) throw new ApplicationException("BeginTransaction() method must be called before retrieve.");

			var messageReceived = new List<MQMessage>();

			try
			{
				var singleSegmentMessageOptions = new MQGetMessageOptions
				{
					Options = MQC.MQGMO_SYNCPOINT + MQC.MQGMO_WAIT + MQC.MQGMO_FAIL_IF_QUIESCING + (isSafe ? 0 : MQC.MQGMO_CONVERT),
					WaitInterval = 30000
				};

				var multipleSegmentsMessageOptions = new MQGetMessageOptions
				{
					// Do not use MQC.MQGMO_SYNCPOINT here. It will affect rollback by only doing partial rollback and leaving only the first message in the queue.
					Options = MQC.MQGMO_WAIT + MQC.MQGMO_FAIL_IF_QUIESCING + MQC.MQMO_MATCH_MSG_ID + (isSafe ? 0 : MQC.MQGMO_CONVERT),
					// 2 minutes
					WaitInterval = 120000
				};

				var msgOption = singleSegmentMessageOptions;
				var message = new MQMessage(){ Version = 2};
				while (true)
				{
					queue.Get(message, msgOption);
					if (logger.IsTraceEnabled) logger.Trace(message.PropertiesToString());
					messageReceived.Add(message);

					if (message.MessageType == MQC.MQMT_APPL_FIRST)
					{
						msgOption = multipleSegmentsMessageOptions;
						var previousMsgId = message.MessageId;
						message = new MQMessage
						{
							MessageId = previousMsgId,
							Version = 2
						};
					}
					else
					{
						// Exit condition: Normally, would check == MQMT_APPL_LAST, but for AES it's == MQMT_DATAGRAM. So != MQMT_APPL_FIRST does the trick for both
						RetryCount = 0;
						break;
					}
				}
			}
			catch (MQException ex)
			{
				if (ex.ReasonCode != MQC.MQRC_NO_MSG_AVAILABLE)
				{
					throw;
				}

				if (messageReceived.Count > 0)
				{
					if (RetryCount >= MaxRetryCount)
					{
						RetryCount = 0;
						throw new FailedMQMessageException("Poison multi-segment message", messageReceived.ToArray());
					}

					RetryCount++;

					logger.WarnFormat("MQRC_NO_MSG_AVAILABLE happened when retrieving a multi-segment message. Will retry on next run.\r\nQueueName: {0}. Current retrieved messages count: {1}. Current Queue Depth: {2}. Retry #{3}",
						config.QueueName, messageReceived.Count, GetCurrentDepth(), RetryCount);

					messageReceived.Clear();
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error when retrieving messages from MQ", ex);
				throw;
			}

			return messageReceived.ToArray();
		}

		public virtual void BeginTransaction()
		{
			currentQueueManager = QueueManagerProvider.GetOrCreateNewQueueManager(config);
			queue = currentQueueManager.AccessQueue(config.QueueName, MQC.MQOO_INPUT_EXCLUSIVE + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_INQUIRE);
			queue.SetCloseOptions(MQC.MQCO_NONE);
		}

		public virtual void CommitTransaction()
		{
			CloseTheQueueAndDisposeTheManager(() =>
			{
				if (currentQueueManager != null)
				{
					currentQueueManager.Commit();
				}
			});
		}

		public virtual void RollbackTransaction()
		{
			CloseTheQueueAndDisposeTheManager(() =>
			{
				if (currentQueueManager != null)
				{
					try
					{
						currentQueueManager.Backout();
					}
					catch (Exception exception)
					{
						logger.Error("Exception when backing out currentQueueManager", exception);
					}
				}
			});
		}

		void CloseTheQueueAndDisposeTheManager(Action queueManagerAction)
		{
			if (queue != null)
			{
				try
				{
					queue.Close();
				}
				catch (Exception exception)
				{
					logger.Error("Exception when closing the queue", exception);
				};

				queue = null;
			}

			if (queueManagerAction != null)
			{
				queueManagerAction.Invoke();
			}

			if (currentQueueManager != null)
			{
				currentQueueManager = null;
			}
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				CloseTheQueueAndDisposeTheManager(null);

				if (queueManagerProvider != null)
				{
					queueManagerProvider.Dispose();
					queueManagerProvider = null;
				}
			}
		}

		#endregion

		#region Implementation

		int GetCurrentDepth()
		{
			try
			{
				return queue.CurrentDepth;
			}
			catch (Exception exception)
			{
				logger.Error(exception);
				return -1;
			}
		}

		public int MaxRetryCount { get; set; } = 12;
		public int RetryCount { get; set; } = 0;

		IMQQueueManagerProvider QueueManagerProvider
		{
			get { return queueManagerProvider ?? (queueManagerProvider = GetMQQueueManagerProvider()); }
		}
		IMQQueueManagerProvider queueManagerProvider;

		protected virtual IMQQueueManagerProvider GetMQQueueManagerProvider()
		{
			return new MQQueueManagerProvider(logger);
		}

		#endregion

		readonly IMQConfiguration config;
		readonly ILog logger;
		readonly ILog messageLogger;
		readonly string messageType;
		IMQQueueManagerWrapper currentQueueManager;
		public IMQQueueWrapper queue;
	}
}
