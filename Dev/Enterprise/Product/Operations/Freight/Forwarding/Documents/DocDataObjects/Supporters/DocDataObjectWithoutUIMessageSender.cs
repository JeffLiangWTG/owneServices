using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects
{
	public abstract class DocDataObjectWithoutUIMessageSender : IDocDataObjectWithoutUIMessageSender
	{
		public bool SendMessage(BusinessObject bizObj, INotifications notifications)
		{
			if (bizObj == null
				|| notifications == null)
			{
				return false;
			}

			if (!IsValidBeforeSending(bizObj, notifications))
			{
				return false;
			}

			var docDataObject = GetDocDataObject(bizObj);

			if (docDataObject == null)
			{
				return false;
			}

			notifications.AddRange(docDataObject.NotificationsIncludingChildren);
			ProcessNotifications(notifications, docDataObject, bizObj);

			if (docDataObject.HasMessageErrors)
			{
				return false;
			}

			var documentDataStorage = LoadOrCreateDocumentDataStorage(bizObj, DataStoreName);
			var messageInstructions = GetMessageInstructions(bizObj.Factory);
			var res = NoUIMessageSender.SendUXml(messageInstructions, documentDataStorage, docDataObject, notifications);

			return res;
		}

		protected abstract DocDataObject GetDocDataObject(BusinessObject bizObj);

		protected abstract ZString DataStoreName { get; }

		protected virtual IVisualizerDocumentData LoadOrCreateDocumentDataStorage(BusinessObject bizObj, ZString dataStoreName)
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

		protected abstract IMessageInstructions GetMessageInstructions(BusinessObjectFactory factory);

		protected abstract bool IsValidBeforeSending(BusinessObject bizObj, INotifications notifications);

		protected virtual void ProcessNotifications(INotifications notifications, DocDataObject docDataObject, BusinessObject bizObj) { }
	}
}
