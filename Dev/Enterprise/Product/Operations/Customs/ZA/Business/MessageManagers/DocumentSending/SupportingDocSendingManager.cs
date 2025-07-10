using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.EventProcessors;
using Enterprise.Customs.ZA.Business.MessageBuilders.DocumentSending;

namespace Enterprise.Customs.ZA.Business.MessageManagers.DocumentSending
{
	public class SupportingDocSendingManager : Customs.Business.SupportingDocSendingManager
	{
		public SupportingDocSendingManager(ISupportingDocSendingObjectParent sendingObjectParent, IMessageNotificationCollector notification)
			: base(sendingObjectParent, notification)
		{
			this.SendingObjectParent = Argument.NotNull(sendingObjectParent, "sendingObjectParent");
			this.notification = Argument.NotNull(notification, "notification");
		}

		protected readonly ISupportingDocSendingObjectParent SendingObjectParent;
		protected readonly IMessageNotificationCollector notification;

		#region Implementation

		protected override IEnumerable<SendableObject> GetObjectsToSend() => SendableObjects.GroupBy(g => (ZString)g.GetHashCode().ToString(CultureInfo.InvariantCulture)).Select(a => new SendableObject() { Group = a.Key, ObjectsToSend = a });

		protected override void UpdateStatus(ISupportingDocumentMessageDataProvider[] sendingObjects)
		{
			foreach (var sendingObject in sendingObjects)
			{
				SupportingDocumentStatusHelper.UpdateStatus((CusEntryHeader)sendingObject.Header, sendingObject.CaseNumber);
			}
		}

		protected override ZString GetDestination(ISupportingDocumentMessageDataProvider dataWrapper) => ZACInterchange.SARSeHubID + ":" + ((IZASupportingDocumentMessageDataProvider)dataWrapper).TradingPartyID;

		protected override void CollectNotificationsFromSendingObjects(MessageSendingNotificationCollection notifications)
		{
			ISupportingDocumentMessageDataProvider dataWrapper = SendingObjectParent.SendingObjects.Cast<SupportingDocSendingObject>().FirstOrDefault();
			if (dataWrapper != null)
			{
				if (((IZASupportingDocumentMessageDataProvider)dataWrapper).TradingPartyID.IsEmpty)
				{
					notifications.AddError(ValidationConstants.SupportingDocSendingManager.EmptyTradingPartyIDError);
				}
			}
		}
		#endregion
	}
}
