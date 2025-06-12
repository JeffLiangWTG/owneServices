using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Enterprise.Customs.FR.TransportSvc.Utilities;

namespace Enterprise.Customs.FR.TransportSvc.Messages
{
	public class PlatformResponsesProcessor : BaseMessagesProcessor
	{
		public PlatformResponsesProcessor(Logger logger) : base(logger)
		{
			System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
		}

		public bool ProcessResponses()
		{
			FtpHelper.FtpConnect(Logger);

			//Get the files from the Platform FTP 
			try
			{
				FilesToReplayNextLoop = new List<string>();
				var fileCount = 0;
				Logger.AddNotification("Retrieving platform responses.", Notification.MessageType.Information, Notification.Events.MessageReception, verboseModeOnly: true);
				if (!ExecuteForDebugging)
				{
					fileCount = DowloadNewPlatformResponseMessages();
				}
				var plural = fileCount > 0 ? "s" : string.Empty;
				Logger.AddNotification($"{fileCount} response{plural} downloaded from platform.", Notification.MessageType.Information, Notification.Events.FTPFileRecovery, string.IsNullOrEmpty(plural));
			}
			catch (Exception ex)
			{
				Logger.AddNotification(ex.Message, Notification.MessageType.Error, Notification.Events.FTPFileRecovery, verboseModeOnly: false);
				return false;
			}

			foreach (var filePath in Directory.GetFiles(WorkingDirectory, $"*{Constants.Extensions.Xml}"))
			{
				CurrentMessageContentBeforeAnyChange = File.ReadAllText(filePath);
				try
				{
					var processed = Process(filePath);
					if (processed)
					{
						Logger.AddNotification($"Response file {filePath} processed OK.", Notification.MessageType.Information, Notification.Events.ProcessOver, verboseModeOnly: false);
					}
					else
					{
						Logger.AddNotification($"Response file {filePath} processed KO.", Notification.MessageType.Error, Notification.Events.ProcessOver, verboseModeOnly: false);
					}
					File.Delete(filePath);
				}
				catch (Exception ex)
				{
					Logger.AddNotification(ex.Message, Notification.MessageType.Error, Notification.Events.UnkownError, verboseModeOnly: false);
					if (IsNetworkException(ex))
					{
						RevertAnyChangeAndAddToNextRoundQueue(filePath);
					}
					Logger.AddNotification($"Response file {filePath} processed KO.", Notification.MessageType.Error, Notification.Events.ProcessOver, verboseModeOnly: false);
				}
			}

			CleanDirectoryWisely(WorkingDirectory);
			FtpHelper.DisconnectAndWait(Logger);

			return true;
		}

		int DowloadNewPlatformResponseMessages() => FtpHelper.GetFTPDirectoryContent(FtpHelper.FtpSendDirectory, WorkingDirectory);

		public bool Process(string filePath)
		{
			var messageProcessed = false;
			Logger.AddNotification($"Processing response file {filePath} .", Notification.MessageType.Information, Notification.Events.ProcessInProgress, verboseModeOnly: false);
			var xmlOutputSourceDoc = LoadAndTweakResponseXmlFile(filePath);

			//Transformation or Universal Event ?
			var responseType = GetMessageType(xmlOutputSourceDoc);

			if (responseType == Constants.MessageTypes.DeltaAck)
			{
				messageProcessed = ProcessFunctionalResponse(xmlOutputSourceDoc, filePath, Constants.UniversalEvent.Statuses.MessageAcknowledged);
			}
			else if (responseType == Constants.MessageTypes.DeltaNack)
			{
				messageProcessed = ProcessFunctionalResponse(xmlOutputSourceDoc, filePath, Constants.UniversalEvent.Statuses.MessageRejectedByMareva);
			}
			else if (responseType == Constants.MessageTypes.DeltaReceiveErr1)
			{
				messageProcessed = ProcessFunctionalResponse(xmlOutputSourceDoc, filePath, Constants.UniversalEvent.Statuses.MessageRejectedByCustoms);
			}
			else if (Constants.StandardResponseMessageTypeList.Contains(responseType))
			{
				messageProcessed = ProcessStandardResponse(xmlOutputSourceDoc);
			}
			else
			{
				messageProcessed = SkipMessage(filePath, responseType);
			}
			return messageProcessed;
		}

		public bool ProcessStandardResponse(XmlDocument xmlOutputSourceDoc)
		{
			bool messageProcessed;

			xmlOutputSourceDoc = AddOrUpdateElements(xmlOutputSourceDoc);

			var transformedMessage = GetTransformedMessage(xmlOutputSourceDoc, isResponse: true);


			if (transformedMessage != null)
			{
				var outputMessage = transformedMessage.ToString();
				if (IsDeltaTPhase5Interchange(xmlOutputSourceDoc))
				{
					outputMessage = ToolBox.UpdateTP5NameSpaces(outputMessage, ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "messageType"));
				}

				using (var memoryStream = ToolBox.GenerateStreamFromString(outputMessage))
				{
					memoryStream.Seek(0, SeekOrigin.Begin);
					var messageGuid = SendMemoryStreamToCW1(memoryStream, "EAD", ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "RecipientID"), ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "SenderID"));
					MessageStringTransformed.Dispose();
				}

				transformedMessage.Dispose();
				messageProcessed = true;
			}
			else
			{
				messageProcessed = false;
			}
			return messageProcessed;
		}

		public XmlDocument AddOrUpdateElements(XmlDocument xmlOutputSourceDoc)
		{
			if (xmlOutputSourceDoc != null)
			{
				var application = ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "Application");

				if (application == Constants.ApplicationTypes.DeltaT)
				{
					var referenceDossier = ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "RefDossier");
					xmlOutputSourceDoc = ToolBox.UpdateElementTextByTagName(xmlOutputSourceDoc, "MesIdeMES19", referenceDossier);

					var interchangeNumber = ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "InterchangeNumber");
					var messageType = ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "messageType");
					if (string.IsNullOrEmpty(messageType))
					{
						messageType = ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "MesTypMES20");
					}
					xmlOutputSourceDoc = ToolBox.UpdateElementTextByTagName(xmlOutputSourceDoc, "InterchangeNumber", interchangeNumber + "." + messageType + "." + GetTime());
				}
				else if (application == Constants.ApplicationTypes.DeltaCG || application == Constants.ApplicationTypes.DeltaDG || application == Constants.ApplicationTypes.DeltaC || application == Constants.ApplicationTypes.DeltaD || application == Constants.ApplicationTypes.APPLUS)
				{
					var xmlMessageRoot = ToolBox.GetElementSafely(xmlOutputSourceDoc.DocumentElement);
					XmlNode xmlInterchangeDataNode = xmlOutputSourceDoc.CreateElement("InterchangeData");
					xmlMessageRoot.AppendChild(xmlInterchangeDataNode);
					ToolBox.AddElement(xmlInterchangeDataNode, xmlOutputSourceDoc, "InterchangeTime", GetTime());
				}
				else if (application == Constants.ApplicationTypes.DeltaIE)
				{
					var xmlMessageRoot = ToolBox.GetElementSafely(xmlOutputSourceDoc.DocumentElement);

					var interchangeNumber = ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "InterchangeNumber");
					var messageType = ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "TypeMessage");
					xmlOutputSourceDoc = ToolBox.UpdateElementTextByTagName(xmlOutputSourceDoc, "InterchangeNumber", interchangeNumber + "." + messageType + "." + GetTime());

					var jsonMessage = ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "MessageJson");
					jsonMessage = @"{""SchemaId"":""#SCHEMAIDPLACEHOLDER#"",""TransactionId"":""#TRANSACTIONIDIDPLACEHOLDER#"",""MessageJson"":" + jsonMessage + "}";
					jsonMessage = jsonMessage.Replace("#SCHEMAIDPLACEHOLDER#", ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "TypeMessage"));
					jsonMessage = jsonMessage.Replace("#TRANSACTIONIDIDPLACEHOLDER#", CleanTransactionID(ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "TransactionId")));
					jsonMessage = ToolBox.CleanBadlyEncodedString(jsonMessage);
					ToolBox.AddElement(xmlMessageRoot, xmlOutputSourceDoc, "Body", jsonMessage);
				}
				else if (application == Constants.ApplicationTypes.PNTS)
				{
					var interchangeNumber = ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "InterchangeNumber");
					var messageType = ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "TypeMessage");
					xmlOutputSourceDoc = ToolBox.UpdateElementTextByTagName(xmlOutputSourceDoc, "InterchangeNumber", interchangeNumber + "." + messageType + "." + GetTime());
				}
				else
				{
					Logger.AddNotification("Response application code unknown.", Notification.MessageType.Error, Notification.Events.ProcessOver, verboseModeOnly: false);
				}
			}

			return xmlOutputSourceDoc;
		}

		bool ProcessFunctionalResponse(XmlDocument xmlOutputSourceDoc, string filePath, string eventStatus)
		{
			Logger.AddNotification("Sending an UEM of type " + eventStatus + " back to CW1.", Notification.MessageType.Information, Notification.Events.ProcessInProgress, verboseModeOnly: false);

			var outputFilePath = CreateUniversalEventMessageFile(filePath, eventStatus, GetMessageAdditionalInfo(xmlOutputSourceDoc), xmlOutputSourceDoc);
			if (string.IsNullOrEmpty(outputFilePath))
			{
				return false;
			}

			if (!ExecuteForDebugging)
			{
				SendFileToCW1(outputFilePath, ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "RecipientID"), ToolBox.GetElementTextByTagNameSafely(xmlOutputSourceDoc, "SenderID"));
			}

			File.Delete(outputFilePath);

			Logger.AddNotification(eventStatus + " notification sent back to CW1", Notification.MessageType.Information, Notification.Events.NotificationSent, verboseModeOnly: false);
			return true;
		}

		string CreateUniversalEventMessageFile(string filePath, string eventStatus, string eventAdditionalInfo, XmlDocument messageDocument)
		{
			var application = ToolBox.GetElementTextByTagNameSafely(messageDocument, "Application");
			var targetType = UniversalEventHelper.GetTargetType(application);
			var eventType = UniversalEventHelper.GetEventType(application);
			var recipientID = ToolBox.GetElementTextByTagNameSafely(messageDocument, "RecipientID");
			var senderID = ToolBox.GetElementTextByTagNameSafely(messageDocument, "SenderID");
			var declarationReference = GetJobReference(ToolBox.GetElementTextByTagNameSafely(messageDocument, "RefDossier"));
			var interchangeNumber = ToolBox.GetElementTextByTagNameSafely(messageDocument, "InterchangeNumber");
			var transactionID = CleanTransactionID(ToolBox.GetElementTextByTagNameSafely(messageDocument, "TransactionId"));
			var eventDate = GetDateTime();
			var xmlUniversalEventMessage = UniversalEventHelper.GetUniversalMessageContent(AssemblyPath, targetType, eventType, eventStatus, eventAdditionalInfo, eventDate, recipientID, senderID, declarationReference, interchangeNumber, transactionID);

			var outputFileName = new FileInfo(filePath).Name;
			outputFileName = outputFileName.Replace("." + new FileInfo(outputFileName).Extension, string.Empty) + "_" + eventStatus + ".xml";
			var outputFilePath = Path.Combine(WorkingDirectory, outputFileName);

			File.WriteAllText(outputFilePath, xmlUniversalEventMessage);

#pragma warning disable WTG2002 // Avoid conditional compilation based on DEBUG.
#if DEBUG
			UniversalEventContentForTest = xmlUniversalEventMessage;
#pragma warning restore WTG2002 // Avoid conditional compilation based on DEBUG.
#endif

			return outputFilePath;
		}

		bool SkipMessage(string filePath, string responseType)
		{
			File.Delete(filePath);
			var fileInfo = new FileInfo(filePath);
			Logger.AddNotification($"{fileInfo.Name} type ({responseType}) is unknown. Message skipped.", Notification.MessageType.Warning, Notification.Events.UnkownError, verboseModeOnly: false);
			return true;
		}

		public string SendMemoryStreamToCW1(MemoryStream memoryStream, string messageNamespace, string senderID, string recipientId)
		{
			var result = string.Empty;

			if (memoryStream == null)
			{
				throw new IOException("Null memory message stream!");
			}

			if (!ExecuteForDebugging)
			{
				var password = senderID == EHubProductionLogin ? EHubProductionPassword : EHubTestPassword;
				var ehubAddress = GetEhubAddressToUploadTo(recipientId);
				result = eAdaptorSampleWebClient.SendMessage(ehubAddress, memoryStream, recipientId, senderID, password, messageNamespace);
				if (!string.IsNullOrEmpty(eAdaptorSampleWebClient.SendErrorMessage))
				{
					Logger.AddNotification(eAdaptorSampleWebClient.SendErrorMessage, Notification.MessageType.Error, Notification.Events.SendingError, verboseModeOnly: false);
				}
			}
			else
			{
				result = Guid.Empty.ToString();
			}

			if (!ExecuteForDebugging)
			{
				Logger.AddNotification($"Transformed platform message {result}.xml" + " sent to eHub.", Notification.MessageType.Information, Notification.Events.MessageSent, verboseModeOnly: false);
			}
			else
			{
				Logger.AddNotification($"Transformed platform message not sent to eHub in debug mode.", Notification.MessageType.Information, Notification.Events.MessageSent, verboseModeOnly: false);
			}

			return result;
		}

		public static XmlDocument LoadAndTweakResponseXmlFile(string filePath)
		{
			var xmlInput = File.ReadAllText(filePath, Encoding.GetEncoding(1252));
			var messageDocument = new XmlDocument() { XmlResolver = null };
			var sreader = new System.IO.StringReader(xmlInput);
			using (var reader = XmlReader.Create(sreader, new XmlReaderSettings() { XmlResolver = null }))
			{
				messageDocument.Load(reader);

				//Lets revert the transaction ID as it was originally sent by CW1
				var transactionId = ToolBox.GetElementTextByTagNameSafely(messageDocument, "TransactionId");
				messageDocument = ToolBox.UpdateElementTextByTagName(messageDocument, "TransactionId", CleanTransactionID(transactionId));
			}

			return messageDocument;
		}
	}
}
