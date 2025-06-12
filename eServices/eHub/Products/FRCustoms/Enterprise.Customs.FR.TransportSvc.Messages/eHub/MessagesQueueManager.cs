using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using CargoWise.eHub.Adapter;
using Enterprise.Customs.FR.TransportSvc.Utilities;

namespace Enterprise.Customs.FR.TransportSvc.Messages
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	public static class MessagesQueueManager
	{
		public static void DownloadIncomingMessages(Logger logger)
		{
			//Recovery of messages from eHub
			var messageList = new List<IeHubMessage>();

			try
			{
				if (ShouldProcessProductionMessages)
				{
					logger.AddNotification("Retrieving production messages from production eHub.", Notification.MessageType.Information, Notification.Events.MessageReception, verboseModeOnly: true);
					using (var myAdapter = new eHubAdapter(eAdaptorSampleWebClient.GetConfiguration(EHubAddress), EHubProductionLogin, EHubProductionPassword))
					{
						GetRemoteMessages(logger, myAdapter, messageList);
					}
				}

				if (ShouldProcessTestMessages)
				{
					logger.AddNotification("Retrieving test messages from production eHub.", Notification.MessageType.Information, Notification.Events.MessageReception, verboseModeOnly: true);
					using (var myAdapter = new eHubAdapter(eAdaptorSampleWebClient.GetConfiguration(EHubAddress), EHubTestLogin, EHubTestPassword))
					{
						GetRemoteMessages(logger, myAdapter, messageList);
					}
					logger.AddNotification("Retrieving test messages from test eHub.", Notification.MessageType.Information, Notification.Events.MessageReception, verboseModeOnly: true);
					using (var myTestAdapter = new eHubAdapter(eAdaptorSampleWebClient.GetConfiguration(EHubForTestAddress), EHubTestLogin, EHubTestPassword))
					{
						GetRemoteMessages(logger, myTestAdapter, messageList);
					}
				}
			}
			catch (Exception ex)
			{
				logger.AddNotification(ex.Message, Notification.MessageType.Error, Notification.Events.UnkownError, verboseModeOnly: false);
			}

			BackUpMessagesForProcessingPurpose(logger, messageList);
		}

		public static List<IeHubMessage> GetRemoteMessages(Logger logger, eHubAdapter adapter, List<IeHubMessage> messageList)
		{
			if (adapter != null)
			{
				try
				{
					if (!ExecuteForDebugging)
					{
						adapter.RetrieveMessages();
						messageList.AddRange(adapter.Inbox);
						adapter.Inbox.MarkAsRead();
					}
				}
				catch (Exception ex)
				{
					logger.AddNotification(ex.Message, Notification.MessageType.Error, Notification.Events.ReceptionError, verboseModeOnly: false);
				}
			}

			return messageList;
		}

		public static void BackUpMessagesForProcessingPurpose(Logger logger, List<IeHubMessage> messageList)
		{
			foreach (var message in messageList)
			{
				var filePath = Path.Combine(WorkingDirectory, message.TrackingID.ToString()) + Constants.Extensions.Xml;

				using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					try
					{
						message.MessageStream.CopyTo(stream);
						stream.Close();
					}
					catch (Exception ex)
					{
						logger.AddNotification(ex.Message, Notification.MessageType.Error, Notification.Events.UnkownError, verboseModeOnly: false);
					}
				}
			}
		}

		public static List<IeHubMessage> RebuildCW1MessagesQueue(Logger logger)
		{
			var messageList = new List<IeHubMessage>();

			foreach (var filePath in Directory.GetFiles(WorkingDirectory, "*" + Constants.Extensions.Xml))
			{
				var fileInfo = new FileInfo(filePath);
				try
				{
					var messageDocument = new XmlDocument() { XmlResolver = null };
					var sreader = new System.IO.StringReader(File.ReadAllText(filePath));

					using (var reader = XmlReader.Create(sreader, new XmlReaderSettings() { XmlResolver = null }))
					{
						messageDocument.Load(reader);
						var senderID = ToolBox.GetElementTextByTagNameSafely(messageDocument, "SenderID");
						var recipientID = ToolBox.GetElementTextByTagNameSafely(messageDocument, "RecipientID");

						//Get guid from filename
						var separatorPosition = fileInfo.Name.IndexOf(".", StringComparison.InvariantCulture);
						var fileGuid = fileInfo.Name.Substring(0, separatorPosition);
						var messageGuid = new Guid(fileGuid);

						//Add message to queue
						messageList.Add(new IeHubMessageCustom(senderID, recipientID, "GMD", messageGuid, File.ReadAllText(filePath)));
					}
				}
				catch (Exception ex)
				{
					logger.AddNotification($"RebuildCW1MessagesQueue: skipping file {filePath} because of error : " + ex.Message, Notification.MessageType.Error, Notification.Events.UnkownError, verboseModeOnly: false);
					File.Delete(filePath);
				}
			}

			logger.AddMessageListNotification(messageList);

			return messageList;
		}

		static bool ExecuteForDebugging => ApplicationConfig.Instance.ExecuteForDebugging == "1";
		static bool ShouldProcessProductionMessages => ApplicationConfig.Instance.EHubProductionEnabled == "1";
		static bool ShouldProcessTestMessages => ApplicationConfig.Instance.EHubTestEnabled == "1";
		static string EHubAddress => ApplicationConfig.Instance.EHubAddress;
		static string EHubForTestAddress => ApplicationConfig.Instance.EHubForTestAddress;
		static string EHubProductionLogin => ApplicationConfig.Instance.EHubProductionLogin;
		static string EHubProductionPassword => ApplicationConfig.Instance.EHubProductionPassword;
		static string EHubTestLogin => ApplicationConfig.Instance.EHubTestLogin;
		static string EHubTestPassword => ApplicationConfig.Instance.EHubTestPassword;
		static string WorkingDirectory => ApplicationConfig.Instance.WorkingDirectory;
	}
}
