using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors
{
	abstract class NotificationUpdater
	{
		internal static NotificationUpdater New(EBACCANotificationType notification, ZStringBuilder emailBody)
		{
			switch (notification.MessageType)
			{
				case EBACCANotificationTypeMessageType.RequestMoreInfo:
					return new NotificationUpdaterRequestMoreInfo(notification, emailBody);

				case EBACCANotificationTypeMessageType.Cancellation:
					return new NotificationUpdaterCancellation(notification, emailBody);

				case EBACCANotificationTypeMessageType.Error:
					return new NotificationUpdaterError(notification, emailBody);

				case EBACCANotificationTypeMessageType.NotifyCRN:
					return new NotificationUpdaterNotifyCRN(notification, emailBody);
			}
			return null;
		}

		protected NotificationUpdater(EBACCANotificationType notification, ZStringBuilder emailBody)
		{
			Notification = notification;
			EmailBody = emailBody;
		}

		protected EBACCANotificationType Notification
		{
			get;
			private set;
		}

		protected ZStringBuilder EmailBody
		{
			get;
			private set;
		}

		protected abstract string ResponseTypeCode { get; }

		internal void UpdateMessage(NZMMessage message, NZMMessage originalMessage)
		{
			message.EM_MessageSubType = ResponseTypeCode;
			message.EM_MessageNum = Notification.References.CallerRefID;
			message.EM_ApplicationReference = Notification.References.ReceiptNumber;
			message.EM_MessageType = NZMMessage.MessageTypes.Receive.Notification;

			message.EM_LinkTable = originalMessage.EM_LinkTable;
			message.EM_LinkUniqueID = originalMessage.EM_LinkUniqueID;
			message.EM_GB = originalMessage.EM_GB;
		}

		internal abstract string ResponseTypeDescription { get; }

		internal abstract string GetResponseInstruction();

		internal abstract void UpdateStatusAndReferences(MAFMessagingBO mafMessaging);

		protected void UpdateStatusAndReferences(MAFMessagingBO mafMessaging, ZString newStatus)
		{
			EmailBody.Append(GetResponseInstruction());
			EmailBody.Append("");

			if (mafMessaging.ZX_MessagingStatus != newStatus)
			{
				mafMessaging.ZX_MessagingStatus = newStatus;
				EmailBody.Append("eBACCa Job Status now set to [" + mafMessaging.ZX_MessagingStatusDescription + "].");
			}
			else
			{
				EmailBody.Append("eBACCa Job Status remains as [" + mafMessaging.ZX_MessagingStatusDescription + "].");
			}

			mafMessaging.ZX_ReceiptNumber = Notification.References.ReceiptNumber;
			EmailBody.Append("");
			EmailBody.Append(ResponseTypeDescription + " Receipt # [" + mafMessaging.ZX_ReceiptNumber + "].");
			if (!string.IsNullOrEmpty(Notification.References.CallerRefID))
			{
				EmailBody.Append("Response to Message # [" + Notification.References.CallerRefID + "].");
			}

			if (!string.IsNullOrEmpty(Notification.References.ConsignmentNumber))
			{
				mafMessaging.ZX_ConsignmentNumber = Notification.References.ConsignmentNumber;
				EmailBody.Append("MPI Consignment # [" + mafMessaging.ZX_ConsignmentNumber + "].");
			}

			if (!string.IsNullOrEmpty(Notification.Description))
			{
				EmailBody.Append("MPI Comment: " + Notification.Description);
			}
		}
	}
}
