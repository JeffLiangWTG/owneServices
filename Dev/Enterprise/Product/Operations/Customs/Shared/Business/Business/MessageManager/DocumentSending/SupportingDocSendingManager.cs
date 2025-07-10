using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.Business
{
	public interface IMessageNotificationCollector : IUserNotification, INotifications
	{
		MessageSendingNotificationCollection Notifications { get; }
	}

	public class SupportingDocSendingManager
	{
		public SupportingDocSendingManager(ISupportingDocSendingObjectParent sendingObjectParent, IMessageNotificationCollector notification)
		{
			this.sendingObjectParent = Argument.NotNull(sendingObjectParent, "sendingObjectParent");
			this.notification = Argument.NotNull(notification, "notification");
		}

		readonly ISupportingDocSendingObjectParent sendingObjectParent;
		readonly IMessageNotificationCollector notification;

		#region Implementation

		public virtual void SendMessages()
		{
			if (CanSendMessages())
			{
				notification.Notifications?.Clear();
				foreach (var sendingObjects in GetObjectsToSend())
				{
					SendMessage(sendingObjects.ObjectsToSend.ToArray());
				}

				try
				{
					sendingObjectParent.Factory.Save();
					ShowMessageSendingResult();
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
				}
			}
			else
			{
				ShowMessageSendingResult();
			}
		}

		protected IEnumerable<ISupportingDocumentMessageDataProvider> SendableObjects => sendingObjectParent.SendingObjects.Cast<ISupportingDocumentMessageDataProvider>().Where(x => x.ShouldSend);

		protected virtual IEnumerable<SendableObject> GetObjectsToSend() => SendableObjects.GroupBy(g => g.LocalReferenceNumber).Select(a => new SendableObject() { Group = a.Key, ObjectsToSend = a });

		protected virtual void SendMessage(ISupportingDocumentMessageDataProvider[] sendingObjects)
		{
			var firstSendingObject = sendingObjects.FirstOrDefault();
			var universalEvents = firstSendingObject?.GetSupportingDocUniversalEventBuilder()?.BuildUniversalEvent(sendingObjects) ?? Enumerable.Empty<UniversalEvent>();

			var areAllDeliveriesSuccessful = true;
			foreach (var universalEvent in universalEvents)
			{
				using (universalEvent)
				{
					var isDeliverySuccessful = Deliver(firstSendingObject, universalEvent);

					if (!isDeliverySuccessful)
					{
						NotifyNotSent(firstSendingObject?.Document?.FileName ?? ZString.Empty);
					}
					areAllDeliveriesSuccessful &= isDeliverySuccessful;
				}
			}

			if (areAllDeliveriesSuccessful)
			{
				UpdateStatus(sendingObjects);
				NotifyQueuedForSending(sendingObjects);
			}
		}

		bool Deliver(ISupportingDocumentMessageDataProvider dataWrapper, UniversalEvent universalEvent)
		{
			var isDeliverySuccessful = false;

			if (universalEvent != null)
			{
				var context = new DeliveryContext(sendingObjectParent.Factory)
				{
					ParentInfo = EntityInfo.New(dataWrapper.BusinessObject),
					ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
					MessageTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalEvent,
					MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalEvent,
					Notifications = notification
				};

				var delivery = new SupportingDocEHubDelivery(universalEvent);
				var mode = new NonPersistentEDICommunicationMode
				{
					EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
					EK_Destination = GetDestination(dataWrapper)
				};
				var result = delivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(context.ParentInfo, universalEvent, new XmlWriter(), UniversalXmlInfo.Namespace_2011_11));

				isDeliverySuccessful = result.Succeeded;
			}

			return isDeliverySuccessful;
		}

		protected virtual void UpdateStatus(ISupportingDocumentMessageDataProvider[] sendingObjects) { }

		protected virtual ZString GetDestination(ISupportingDocumentMessageDataProvider dataWrapper) => string.Empty;

		void NotifyNotSent(string fileName)
		{
			var message = Res.GetString("b05ab8e2-49d2-4812-8a66-2cb0f4e737fa", "An error occurred while trying to send the document: {0}", fileName);
			notification.Notifications.AddError(message);
		}

		protected virtual void NotifyQueuedForSending(ISupportingDocumentMessageDataProvider[] sendingObjects)
		{
			var message = Res.GetString("a35cffb0-ab68-4c59-bb58-0d331e544a34", "{0} queued for sending: {1}",
									sendingObjects.Length > 1
									? Res.GetString("5b7fc8c6-91d8-42b4-843b-d79afaa9ad72", "Documents")
									: Res.GetString("c6b7ba8b-cf9e-4daa-8a3b-cf1e907771b7", "Document"),
									string.Join(", ", sendingObjects.Select(x => x.Document?.FileName ?? ZString.Empty)));
			notification.Notifications.AddInformation(message);
		}

		protected virtual bool CanSendMessages()
		{
			var result = true;
			var notifications = notification?.Notifications;
			if (notifications != null)
			{
				CollectNotificationsFromSendingObjects(notifications);

				if (notifications.ContainsError())
				{
					notification.ShowError(notifications.ErrorNotificationsAsString(), Res.GetString("890C5862-7351-401F-8D5D-E2ED0DD577F7", "Cannot send Supporting Documents"));
					result = false;
				}
				if (notifications.ContainsWarning())
				{
					result = notification.ShowConfirmation(notifications.WarningNotificationsAsString(), Res.GetString("2DF24F4A-97FC-48E4-AC0F-6CA105704853", "Continue sending with warning?"));
				}

				notifications.Clear();
			}
			return result;
		}

		protected virtual void CollectNotificationsFromSendingObjects(MessageSendingNotificationCollection notifications)
		{
		}

		protected void ShowMessageSendingResult()
		{
			var notifications = notification?.Notifications;
			if (notifications != null)
			{
				if (notifications.ContainsError())
				{
					notification.ShowError(notifications.ErrorNotificationsAsString(), Res.GetString("adaa548d-83be-4930-830e-0c35fa839912", "Error in Sending Supporting Documents"));
				}
				if (notifications.ContainsInformation())
				{
					notification.ShowInformation(GetMessageSendingResultInformation(notifications), Res.GetString("dbb5cc19-1050-40cc-bd51-5993e1bc1534", "Supporting Document Sending Result"));
				}
			}
		}

		protected virtual ZString GetMessageSendingResultInformation(MessageSendingNotificationCollection notifications)
		{
			return notifications.InformationNotificationsAsString();
		}

		#endregion

		public class SendableObject
		{
			public ZString Group { get; set; }

			public IEnumerable<ISupportingDocumentMessageDataProvider> ObjectsToSend { get; set; }
		}
	}
}
