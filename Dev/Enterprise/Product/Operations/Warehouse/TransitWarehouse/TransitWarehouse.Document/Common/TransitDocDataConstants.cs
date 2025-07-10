using CargoWise.Types;

namespace Enterprise.Warehouse.Transit.Document
{
	#region SuppressResourceStringsCheckRegion

	public static class TransitDocDataConstants
	{
		public const string NOTCIN = "NOTCIN";

		public const string DEFAULTREFERENCE = "-";

		public static class TransitDocDataContext
		{
			public const string CIN750WarehouseIn = "CIN750WarehouseIn";
			public const string CIN750WarehouseDecons = "CIN750WarehouseDecons";
			public const string CIN750WarehouseCons = "CIN750WarehouseCons";
			public const string CIN750WarehouseCor = "CIN750WarehouseCor";
			public const string CIN750WarehouseOut = "CIN750WarehouseOut";
		}

		public static class SourceNames
		{
			public const string ReceiveConsignment = "Receive Consignment";
			public const string DispatchConsignment = "Dispatch Consignment";
		}

		public static class NotificationXMLNamespace
		{
			public const string CIN750InNotification = "CIN750/In";
			public const string CIN750OutNotification = "CIN750/Out";
			public const string CIN750ConsNotification = "CIN750/Cons";
			public const string CIN750DeconsNotification = "CIN750/Decons";
			public const string CIN750CorNotification = "CIN750/Cor";
		}

		public static class NotificationTypes
		{
			public const string CIN750InNotification = "In";
			public const string CIN750OutNotification = "Out";
			public const string CIN750ConsNotification = "Consolidation";
			public const string CIN750DeconsNotification = "Deconsolidation";
			public const string CIN750CorNotification = "Correction";
		}

		public static class NotificationNames
		{
			public const string CIN750InNotification = "CIN 750 In Notification";
			public const string CIN750OutNotification = "CIN 750 Out Notification";
			public const string CIN750ConsNotification = "CIN 750 Cons Notification";
			public const string CIN750DeconsNotification = "CIN 750 Decons Notification";
			public const string CIN750CorNotification = "CIN 750 Cor Notification";
		}

		public static class AccompanyingDocumentTypes
		{
			public const string DefaultDocumentType = "T1";
		}

		public static class ReceiveConsignmentDataStoreNames
		{
			public const string CIN750InFromRCN = "CIN750InFromRCN";
			public const string CIN750CorFromRCN = "CIN750CorFromRCN";
		}

		public static class DispatchConsignmentDataStoreNames
		{
			public const string CIN750OutFromDCN = "CIN750OutFromDCN";
			public const string CIN750DeconsFromDCN = "CIN750DeconsFromDCN";
			public const string CIN750ConsFromDCN = "CIN750ConsFromDCN";
		}

		public static class TransitDocTemplateIDs
		{
			public static ZGuid CIN750InFromRCNTemplateID = new ZGuid("d241b6d2-8546-450e-bae5-4de9ccf30953");
			public static ZGuid CIN750OutFromDCNTemplateID = new ZGuid("0ec7091b-0a33-4ccd-9d8a-b78f3ab1860a");
			public static ZGuid CIN750CorFromRCNTemplateID = new ZGuid("31c02217-98f0-44ab-b988-7d389c634221");
			public static ZGuid CIN750ConsFromDCNTemplateID = new ZGuid("94786921-8d53-46b8-8f78-b0af5df6026b");
			public static ZGuid CIN750DeconsFromDCNTemplateID = new ZGuid("8e27c6a5-74ee-4dd3-8e1b-c973b20e12fe");
		}
	}

	public enum CIN750NotificationMessageTypes
	{
		CIN750InNotification,
		CIN750CorNotification,
		CIN750DeconsNotification,
		CIN750ConsNotification,
		CIN750OutNotification
	}

	#endregion
}
