using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750WithoutUIMessageSender : IDocDataObjectWithoutUIMessageSender
	{
		public bool SendMessage(BusinessObject bizObj, INotifications notifications)
		{
			if (bizObj == null || notifications == null)
			{
				return false;
			}

			if (!IsValidBeforeSending(bizObj, notifications))
			{
				return false;
			}

			var docDataObject = GetDocDataObject(bizObj, notifications);
			if (docDataObject == null)
			{
				return false;
			}

			var errorMessage = ValidateDocDataObject(docDataObject);
			if (!errorMessage.IsEmpty)
			{
				notifications.AddMessageError(errorMessage);
				(docDataObject as CIN750Notification).PopulateCINMessageNote(errorMessage);

				return false;
			}

			docDataObject.ValidateAllIncludingChildren();
			if (docDataObject.HasMessageErrors || docDataObject.HasErrors)
			{
				var errorMessages = docDataObject.GetMessageErrors().Select(n => n.Message);
				errorMessage = string.Join(System.Environment.NewLine, errorMessages);
				notifications.AddMessageError(errorMessage);
				(docDataObject as CIN750Notification).PopulateCINMessageNote(errorMessage);

				return false;
			}

			(docDataObject as CIN750Notification).PopulateCINMessageNote(errorMessage);

			var messageInstructions = new CIN750MessageInstructions(docData);
			notifications.AddRange(docDataObject.NotificationsIncludingChildren);
			var documentDataStorage = LoadOrCreateDocumentDataStorage(bizObj, docData.DataStoreName);
			var res = NoUIMessageSender.SendUXml(messageInstructions, documentDataStorage, docDataObject, notifications);

			return res;
		}

		DocDataObject GetDocDataObject(BusinessObject bizObj, INotifications notifications)
		{
			switch (bizObj)
			{
				case WhsItemReceiveConsignment rcn:
					return GetDocDataObjectFromRCN(rcn, notifications);
				case WhsItemDispatchConsignment dcn:
					return GetDocDataObjectFromDCN(dcn, notifications);
			}
			return null;
		}

		DocDataObject GetDocDataObjectFromRCN(WhsItemReceiveConsignment rcn, INotifications notifications)
		{
			var historyManager = new ReceiveConsignmentCIN750NotificationHistoryManager(rcn);
			var (messageType, errorMessage) = historyManager.GetNextMessageTypeCore();
			if (messageType != null)
			{
				docData = new CIN750NotificationDocData(messageType.Value);
				return DocDataObjectProvider.GetFromReceiveConsignment(rcn, docData.DataContext);
			}
			else
			{
				AddErrorMessage(rcn, notifications, errorMessage);
			}

			return null;
		}

		DocDataObject GetDocDataObjectFromDCN(WhsItemDispatchConsignment dcn, INotifications notifications)
		{
			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(dcn);
			var (messageType, errorMessage) = historyManager.GetNextMessageTypeCore();
			if (messageType != null)
			{
				docData = new CIN750NotificationDocData(messageType.Value);
				switch (docData.DataContext)
				{
					case TransitDocDataContext.CIN750WarehouseDecons:
						return new CIN750DeconsNotificationBuilder(dcn).Build();
					case TransitDocDataContext.CIN750WarehouseCons:
						return new CIN750ConsNotificationBuilder(dcn, historyManager.AdditionalDataForCons).Build();
					case TransitDocDataContext.CIN750WarehouseOut:
						return new CIN750OutNotificationBuilder(dcn, historyManager.AdditionalDataForOut).Build();
				}
			}
			else
			{
				AddErrorMessage(dcn, notifications, errorMessage);
			}

			return null;
		}

		static void AddErrorMessage(BusinessObject bo, INotifications notifications, string errorMessage)
		{
			if (!errorMessage.IsNullOrEmpty())
			{
				notifications.AddMessageError(errorMessage);
			}
			else
			{
				notifications.AddMessageError(Res.GetString("b13673aa-5849-457b-8557-b3fa914b901b", "Cannot create CIN Notification from current {0}", bo.HumanReadableName));
			}
		}

		IVisualizerDocumentData LoadOrCreateDocumentDataStorage(BusinessObject bizObj, string dataStoreName)
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			var documentData = documentDataLoader.Load(bizObj, dataStoreName);

			if (documentData == null)
			{
				documentData = bizObj.Factory.New<IVisualizerDocumentData>();
				documentData.Parent = bizObj;
				documentData.Name = dataStoreName;
			}

			return documentData;
		}

		bool IsValidBeforeSending(BusinessObject bizObj, INotifications notifications)
		{
			if (bizObj is WhsItemReceiveConsignment || bizObj is WhsItemDispatchConsignment)
			{
				return true;
			}

			notifications.AddMessageError(Res.GetString("37098011-317f-4af4-a02e-d5c041a2b62b", "We cannot send CIN 750 message from {0}", bizObj.HumanReadableName));
			return false;
		}

		ZString ValidateDocDataObject(DocDataObject docDataObject)
		{
			var errorMessage = ZString.Empty;
			if (docDataObject is CIN750InNotification inNotification)
			{
				errorMessage = CIN750NotificationValidation.GetInNotificationValidationMessage(inNotification);
			}
			else if (docDataObject is CIN750CorNotification corNotification)
			{
				errorMessage = CIN750NotificationValidation.GetCorNotificationValidationMessage(corNotification);
			}
			else if (docDataObject is CIN750DeconsNotification deconsNotification)
			{
				errorMessage = CIN750NotificationValidation.GetDeconsNotificationValidationMessage(deconsNotification);
			}
			else if (docDataObject is CIN750ConsNotification consNotification)
			{
				errorMessage = CIN750NotificationValidation.GetConsNotificationValidationMessage(consNotification);
			}
			else if (docDataObject is CIN750OutNotification outNotification)
			{
				errorMessage = CIN750NotificationValidation.GetOutNotificationValidationMessage(outNotification);
			}

			return errorMessage;
		}

		TransitDocDataObjectProvider DocDataObjectProvider => docDataObjectProvider ?? (docDataObjectProvider = new TransitDocDataObjectProvider());
		TransitDocDataObjectProvider docDataObjectProvider;

		CIN750NotificationDocData docData;
	}
}
