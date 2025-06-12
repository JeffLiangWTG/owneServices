using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using CargoWise.eHub.Integration;
using CargoWise.eServices.USCustoms.Integration;
using CargoWise.eServices.USCustoms.MQConfiguration;
using CargoWise.eServices.USCustoms.MQConfigurationABIACE;
using CargoWise.eServices.USCustoms.MQConfigurationAES;
using CargoWise.eServices.USCustoms.MQConfigurationAMA;
using CargoWise.eServices.USCustoms.MQConfigurationAMS;
using CargoWise.eServices.USCustoms.MQConfigurationMAN;
using CargoWise.eServices.USCustoms.MQConfigurationUEM;
using Common.Logging;
using IBM.WMQ;
using ServiceBroker.Common;

namespace CargoWise.eServices.USCustoms.OutboundProcessingService
{
	public class MQMessageSender
	{
		internal static readonly Encoding OutgoingEncoding = Encoding.ASCII;
		public MQMessageSender(ILog logger, IMQQueueManagerProvider queueManagerProvider, CancellationToken cancellationToken)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			if (queueManagerProvider == null) throw new ArgumentNullException("queueManagerProvider");

			this.logger = logger;
			this.queueManagerProvider = queueManagerProvider;
			this.cancellationToken = cancellationToken;
		}

		public void Send(USCustomsOutboundMessage customsMessage, out Stream responseMessageBody, bool isProductionService = true)
		{
			int responseStatus = Constants.ResponseStatus.Fail;
			string errorDescription = string.Empty;
			int retryCount = 0;
			bool retrying;
			string modeOfTransport = string.Empty;
			do
			{
				retrying = false;
				IMQQueueManagerWrapper queueManager = null;
				IMQQueueWrapper queue = null;
				MQMessage message = new MQMessage();

				try
				{
					if (customsMessage.IsProduction != isProductionService)
					{
						var messageEnvironment = customsMessage.IsProduction ? "Production" : "Test";
						var serviceEnvironment = isProductionService ? "Production" : "Test";
						throw new Exception($@"{messageEnvironment} message cannot be sent via a {serviceEnvironment} service.");
					}

					if (customsMessage.ApplicationCode == ApplicationCode.USExportManifest)
					{
						modeOfTransport = GetUEMTransportModeFromXML(GetStreamAsText(customsMessage.MessageStream));
					}

					var config = GetMQConfiguration(customsMessage.ApplicationCode, customsMessage.MessageType, customsMessage.IsProduction, modeOfTransport);
					if (retryCount == 0)
					{
						logger.Info(string.Format("Processing message with InboxPK {4} and MQ Config: {0}, {1}, {2}, {3}", config.QueueName, config.QueueManager, config.Hostname, config.Port, customsMessage.TrackingId));
					}
					queueManager = queueManagerProvider.GetOrCreateNewQueueManager(config);
					if (queueManager != null)
					{
						var options = new MQPutMessageOptions();
						options.Options = MQC.MQPMO_SYNCPOINT;

						queue = queueManager.AccessQueue(config.QueueName, MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);
						message.Encoding = 546;
						message.Format = MQC.MQFMT_STRING;
						message.Persistence = MQC.MQPER_PERSISTENT;
						message.CharacterSet = 437;

						if (logger.IsTraceEnabled)
						{
							logger.Trace(message.PropertiesToString());
						}

						Stream messageStream;
						switch (customsMessage.ApplicationCode)
						{
							case ApplicationCode.AMA:
								messageStream = ReplaceEscapedCharacters(customsMessage.MessageStream);
								break;
							default:
								messageStream = customsMessage.MessageStream;
								break;
						}
						bool isSegmented = WriteMessageBodyToMQMessage(messageStream, queue, options, message);

						if (message.MessageLength > 0)
						{
							if (isSegmented)
							{
								message.MessageType = MQC.MQMT_APPL_LAST;
							}

							TraceMessageProperties(message);
							PutToQueue(queue, options, message);
						}

						if (message.MessageType == MQC.MQMT_APPL_FIRST)
						{
							FaultyMessageLog.Warn(string.Format("Message with only FIRST Message Type(ClientId: {0}, InboxPK: {1}", customsMessage.ClientId, customsMessage.TrackingId));
							if (FaultyMessageLog.IsTraceEnabled)
							{
								FaultyMessageLog.Trace(message.PropertiesToString());

								if (message.MessageLength > 0)
								{
									message.Seek(0);
									var readMessage = message.ReadString(message.MessageLength);
									FaultyMessageLog.Trace(readMessage);
									message.Seek(0);
								}
							}
						}

						queue.Close();
						queue = null;
						queueManager.Commit();
						responseStatus = Constants.ResponseStatus.Pass;
					}
				}
				catch (Exception ex)
				{
					if (ex is MQException || ex is Win32Exception)
					{
						logger.Warn(string.Format(
							"Failed sending outbound MQ message(ClientId: {0}, InboxPK: {1}, RetryCount: {2})\r\n{3}\r\n{4}",
							customsMessage.ClientId, customsMessage.TrackingId, retryCount, ex,
							ex.HelpLink));

						cancellationToken.ThrowIfCancellationRequested();
						Retry(1000, queueManager);
						retryCount++;
						retrying = true;
					}
					else
					{
						errorDescription = ex.ToString();
						logger.Error(string.Format("ClientId: {0}, InboxPK: {1}\r\nException: {2}\r\n Message: {3}", customsMessage.ClientId, customsMessage.TrackingId, ex, GetStreamAsText(customsMessage.MessageStream)));
					}
				}
				finally
				{
					if (queue != null) queue.Close();
				}
			} while (retrying);

			responseMessageBody = GetResponseBody(customsMessage.ClientId, customsMessage.IsProduction, customsMessage.TrackingId, responseStatus, errorDescription);
			LogMessageContent("ServiceBroker OutboundCustomsMessageProcessingReply", responseMessageBody);
		}

		public void Retry(int interval, IMQQueueManagerWrapper queueManager)
		{
			if (queueManager != null)
			{
				queueManager.Dispose();
			}

			if (queueManagerProvider != null)
			{
				queueManagerProvider.Dispose();
				queueManagerProvider = NewMQQueueManagerProvider();
			}

			System.Threading.Thread.Sleep(interval);
		}


		static string GetStreamAsText(Stream messageStream)
		{
			if (messageStream != null && messageStream.CanSeek)
			{
				messageStream.Position = 0;
				return messageStream.ReadToEnd();
			}

			return string.Empty;
		}

		internal static string GetUEMTransportModeFromXML(string xmlString)
		{
			try
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xmlString);

				var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
				if (xmlDoc.DocumentElement != null)
					namespaceManager.AddNamespace("ns", xmlDoc.DocumentElement.NamespaceURI);

				var modeOfTransportNode = xmlDoc.SelectSingleNode("//ns:ModeOfTransportationCode/ns:Value", namespaceManager);
				if (modeOfTransportNode != null)
				{
					switch (modeOfTransportNode.InnerText)
					{
						case "10":
						case "11":
							return "Sea";
						case "40":
						case "41":
							return "Air";
					}
				}
			}
			catch (Exception e)
			{
				throw new ApplicationException("An error occurred while parsing the XML: " + e.Message);
			}
			return string.Empty;
		}

		#region Implementation

		protected virtual IMQConfiguration GetMQConfiguration(string appCode, string messageType, bool isProduction, string modeOfTransport = "")
		{
			if (appCode.Equals(ApplicationCode.USImport))
				return new MQABIACEConfiguration(isProduction);
			if (appCode.Equals(ApplicationCode.USExport))
				return new MQAESConfiguration(isProduction);
			if (appCode.Equals(ApplicationCode.AMS))
				return new MQAMSConfiguration(isProduction);
			if (appCode.Equals(ApplicationCode.USeManifest))
				return new MQMANConfiguration(isProduction);
			if (appCode.Equals(ApplicationCode.USVesselStow))
				return new MQSTWConfiguration(isProduction);
			if (appCode.Equals(ApplicationCode.AMA))
				return new MQAMAConfiguration(isProduction);
			if (appCode.Equals(ApplicationCode.USExportManifest))
			{
				if (modeOfTransport == "Sea")
				{
					return new MQUEMSeaConfiguration(isProduction);
				}
				if (modeOfTransport == "Air")
				{
					return new MQUEMAirConfiguration(isProduction);
				}
				throw new ApplicationException(string.Format("Mode of Transport '{0}' for type '{1}' is not supported", modeOfTransport, typeof(IMQConfiguration).Name));
			}
			throw new ApplicationException(string.Format("Application Code '{0}' for type '{1}' is not supported", appCode, typeof(IMQConfiguration).Name));
		}

		protected virtual IMQQueueManagerProvider NewMQQueueManagerProvider()
		{
			return new MQQueueManagerProvider();
		}

		Stream GetResponseBody(string clientId, bool isProduction, string trackingId, int responseStatus, string errorDescription)
		{
			var result = new MemoryStream();
			var writer = XmlWriter.Create(result, new XmlWriterSettings() { OmitXmlDeclaration = true });
			writer.WriteStartElement(Constants.MessageAttributes.SendResponse);
			writer.WriteAttributeString(Constants.MessageAttributes.ClientId, clientId);
			writer.WriteAttributeString(Constants.MessageAttributes.IsProduction, isProduction.ToString());
			writer.WriteAttributeString(Constants.MessageAttributes.MessageTrackingId, trackingId);
			writer.WriteAttributeString(Constants.MessageAttributes.ResponseStatus, responseStatus.ToString());
			writer.WriteAttributeString(Constants.MessageAttributes.ErrorDescription, errorDescription);
			writer.WriteEndElement();
			writer.Flush();

			return result;
		}

		#endregion

		#region Implementation

		bool WriteMessageBodyToMQMessage(Stream bodyStream, IMQQueueWrapper queue, MQPutMessageOptions options, MQMessage message)
		{
			bool isSegmented = false;
			var buffer = new char[Constants.MaximumBytesPerRead];

			LogMessageContent("MQ submitting message", bodyStream);

			var reader = new StreamReader(bodyStream, OutgoingEncoding);

			int dataRead;
			while ((dataRead = reader.Read(buffer, 0, Constants.MaximumBytesPerRead)) > 0)
			{
				isSegmented |= WriteToMessageAndPutItOnTheQueueIfNeeded(queue, options, message, buffer, dataRead);
			}
			return isSegmented;
		}

		void LogMessageContent(string streamName, Stream stream)
		{
			if (MessageLog.IsTraceEnabled)
			{
				try
				{
					stream.SeekBegin();
					MessageLog.TraceFormat("{0}: {1}", streamName, stream.ReadToEnd());
					stream.SeekBegin();
				}
				catch (Exception exception)
				{
					MessageLog.Warn("Failed to log message content", exception);
				}
			}
		}

		bool WriteToMessageAndPutItOnTheQueueIfNeeded(IMQQueueWrapper queue, MQPutMessageOptions options, MQMessage message, char[] buffer, int dataRead)
		{
			bool isSegmented = false;
			if (message.MessageLength + dataRead > Constants.MaximumBytesPerMessage)
			{
				int charToRead = Constants.MaximumBytesPerMessage - message.MessageLength;

				var bufferToWrite = (char[])buffer.Clone();
				if (charToRead < buffer.Length)
				{
					Array.Resize<char>(ref bufferToWrite, charToRead);
				}

				message.Write(OutgoingEncoding.GetBytes(bufferToWrite));
				message.MessageType = MQC.MQMT_APPL_FIRST;

				TraceMessageProperties(message);
				PutToQueue(queue, options, message);

				bufferToWrite = (char[])buffer.Clone();
				Array.Reverse(bufferToWrite);
				Array.Resize<char>(ref bufferToWrite, dataRead - charToRead);
				Array.Reverse(bufferToWrite);

				message.Write(OutgoingEncoding.GetBytes(bufferToWrite));
				isSegmented = true;
			}
			else
			{
				var bufferToWrite = (char[])buffer.Clone();
				Array.Resize<char>(ref bufferToWrite, dataRead);

				message.Write(OutgoingEncoding.GetBytes(bufferToWrite));
			}
			return isSegmented;
		}

		internal Stream ReplaceEscapedCharacters(Stream stream)
		{
			stream.SeekBegin();
			using (var reader = new StreamReader(stream, OutgoingEncoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
			{
				var unescapedText = reader.ReadToEnd();
				if (MessageLog.IsTraceEnabled)
				{
					MessageLog.Trace("Pre-unescaped MQ submitting message: " + unescapedText);
				}
				var result = Regex.Unescape(unescapedText);
				stream.SeekBegin();
				return new MemoryStream(OutgoingEncoding.GetBytes(result));
			}
		}

		void TraceMessageProperties(MQMessage message)
		{
			if (logger.IsTraceEnabled)
			{
				logger.Trace(message.PropertiesToString());

				if (message.MessageLength > 0)
				{
					message.Seek(0);
					var readMessage = message.ReadString(message.MessageLength);
					logger.Trace(readMessage);
					message.Seek(0);
				}
			}
		}

		void PutToQueue(IMQQueueWrapper queue, MQPutMessageOptions options, MQMessage message)
		{
			queue.Put(message, options);
			options.ClearErrorCodes();
			message.ClearErrorCodes();
			message.ClearMessage();
		}

		#endregion

		public virtual ILog MessageLog
		{
			get { return messageLog ?? (messageLog = LogManager.GetLogger(Constants.LoggerNames.OutboundMessageLogger)); }
		}

		public virtual ILog FaultyMessageLog
		{
			get { return faultyMessageLog ?? (faultyMessageLog = LogManager.GetLogger(Constants.LoggerNames.FaultyOutboundMessageLogger)); }
		}

		ILog messageLog;
		ILog faultyMessageLog;
		readonly ILog logger;
		IMQQueueManagerProvider queueManagerProvider;
		CancellationToken cancellationToken;
	}
}
