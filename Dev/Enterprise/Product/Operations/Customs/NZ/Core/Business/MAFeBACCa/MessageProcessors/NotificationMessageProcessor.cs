using System.Xml.Serialization;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors
{
	class NotificationMessageProcessor<T> : MessageProcessor where T : EBACCANotificationType
	{
		public NotificationMessageProcessor(LoggingInformation logger)
			: base(logger, "eBACCa Notification")
		{
		}

		public override bool CanProcess(string xmlMessageType)
		{
			var rootAttribute = (XmlRootAttribute)typeof(T).GetCustomAttributes(typeof(XmlRootAttribute), false)[0];
			return xmlMessageType == rootAttribute.ElementName;
		}

		protected sealed override string DoProcessingReturningStatus()
		{
			var reader = new SafeXmlReader<T>();
			if (reader.TryReadFromXML(Message.EM_MessageText))
			{
				return ProcessXMLReturningStatus(reader.Result);
			}
			else
			{
				throw new MessageProcessingException("Errors occurred reading in EBACCANotification:-\r\n" + reader.ErrorText, Message, true, false);
			}
		}

		string ProcessXMLReturningStatus(EBACCANotificationType notification)
		{
			if (notification.References == null)
			{
				throw new MessageProcessingException("<References> Element does not exist in Message.", Message, false, true);
			}

			Logger.Log("Processing Notification Receipt # [" + notification.References.ReceiptNumber + "]...");

			var updater = GetUpdater(notification);
			responseTypeDescription = updater.ResponseTypeDescription;

			OriginalMessage = GetOriginalMessage(notification.References.CallerRefID, notification.References.OrganisationCode);
			updater.UpdateMessage(Message, OriginalMessage);

			var mafMessaging = Message.MAFMessaging;
			if (mafMessaging != null)
			{
				jobNumber = mafMessaging.PlugInSupport.JobNumber;
				Logger.Log("Updating Job [" + jobNumber + "]...");
				updater.UpdateStatusAndReferences(mafMessaging);
				return NZMMessage.Status.Received;
			}
			else
			{
				throw new MessageProcessingException("Could not find JobDeclaration linked to EDIMessage with PK [" + Message.PK.ToString() + "].", Message, false, true);
			}
		}

		NotificationUpdater GetUpdater(EBACCANotificationType notification)
			=> NotificationUpdater.New(notification, EmailBody)
				?? throw new MessageProcessingException("Could not find MessageType in Notification message.", Message, false, true);
	}
}
