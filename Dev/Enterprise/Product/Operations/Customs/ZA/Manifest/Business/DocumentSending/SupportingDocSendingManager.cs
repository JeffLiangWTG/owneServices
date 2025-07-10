using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class SupportingDocSendingManager : ZA.Business.MessageManagers.DocumentSending.SupportingDocSendingManager
	{
		public SupportingDocSendingManager(ISupportingDocSendingObjectParent sendingObjectParent, IMessageNotificationCollector notification)
			: base(sendingObjectParent, notification)
		{
		}

		#region Implementation

		protected override IEnumerable<SendableObject> GetObjectsToSend() => SendableObjects.GroupBy(g => g.LocalReferenceNumber).Select(a => new SendableObject() { Group = a.Key, ObjectsToSend = a });

		protected override ZString GetDestination(ISupportingDocumentMessageDataProvider dataWrapper) => "ZACustoms";

		protected override void CollectNotificationsFromSendingObjects(MessageSendingNotificationCollection notifications)
		{
		}

		#endregion
	}
}
