using Enterprise.Warehouse.Transit.Business;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750NotificationDocData
	{
		public CIN750NotificationDocData(CIN750NotificationMessageTypes messageType)
		{
			switch (messageType)
			{
				case CIN750NotificationMessageTypes.CIN750InNotification:
					DocumentName = nameof(TransitDocumentNames.CIN750InNotification);
					DataContext = TransitDocDataContext.CIN750WarehouseIn;
					XmlNamespace = TransitDocDataConstants.NotificationXMLNamespace.CIN750InNotification;
					DataStoreName = TransitDocDataConstants.ReceiveConsignmentDataStoreNames.CIN750InFromRCN;
					break;
				case CIN750NotificationMessageTypes.CIN750CorNotification:
					DocumentName = nameof(TransitDocumentNames.CIN750CorNotification);
					DataContext = TransitDocDataContext.CIN750WarehouseCor;
					XmlNamespace = TransitDocDataConstants.NotificationXMLNamespace.CIN750CorNotification;
					DataStoreName = TransitDocDataConstants.ReceiveConsignmentDataStoreNames.CIN750CorFromRCN;
					break;
				case CIN750NotificationMessageTypes.CIN750DeconsNotification:
					DocumentName = nameof(TransitDocumentNames.CIN750DeconsNotification);
					DataContext = TransitDocDataContext.CIN750WarehouseDecons;
					XmlNamespace = TransitDocDataConstants.NotificationXMLNamespace.CIN750DeconsNotification;
					DataStoreName = TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750DeconsFromDCN;
					break;
				case CIN750NotificationMessageTypes.CIN750ConsNotification:
					DocumentName = nameof(TransitDocumentNames.CIN750ConsNotification);
					DataContext = TransitDocDataContext.CIN750WarehouseCons;
					XmlNamespace = TransitDocDataConstants.NotificationXMLNamespace.CIN750ConsNotification;
					DataStoreName = TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750ConsFromDCN;
					break;
				case CIN750NotificationMessageTypes.CIN750OutNotification:
					DocumentName = nameof(TransitDocumentNames.CIN750OutNotification);
					DataContext = TransitDocDataContext.CIN750WarehouseOut;
					XmlNamespace = TransitDocDataConstants.NotificationXMLNamespace.CIN750OutNotification;
					DataStoreName = TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750OutFromDCN;
					break;
			}
		}

		public string DataStoreName { get; }

		public string DocumentName { get; }

		public string DataContext { get; }

		public string XmlNamespace { get; }
	}
}
