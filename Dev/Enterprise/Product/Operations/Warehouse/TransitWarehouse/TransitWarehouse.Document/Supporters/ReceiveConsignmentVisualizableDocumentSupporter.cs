using System.Collections.Generic;
using System.Linq;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;

namespace Enterprise.Warehouse.Transit.Document
{
	public sealed class ReceiveConsignmentVisualizableDocumentSupporter : TransitWarehouseVisualizableDocumentSupporter<WhsItemReceiveConsignment>
	{
		public ReceiveConsignmentVisualizableDocumentSupporter(WhsItemReceiveConsignment rcn)
			: base(rcn)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.WhsItemReceiveConsignment;
		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;

		protected override string GetDataStoreNameFromDocumentName(string documentName, EventParameters eventParameters)
		{
			switch (documentName)
			{
				case TransitDocumentNames.CIN750InNotification:
					return TransitDocDataConstants.ReceiveConsignmentDataStoreNames.CIN750InFromRCN;
				case TransitDocumentNames.CIN750CorNotification:
					return TransitDocDataConstants.ReceiveConsignmentDataStoreNames.CIN750CorFromRCN;
				default:
					return ZString.Empty;
			}
		}

		public override IMessageLogCreator GetMessageLogCreator(IDocument document)
		{
			switch (document?.DataContext)
			{
				case TransitDocDataContext.CIN750WarehouseIn:
					return new CIN750InNotificationLogsCreator();
				case TransitDocDataContext.CIN750WarehouseCor:
					return new CIN750CorNotificationLogsCreator();
				default:
					return null;
			}
		}

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions)
		{
			switch (document?.DataContext)
			{
				case TransitDocDataContext.CIN750WarehouseIn:
					return new CIN750InNotificationExtensions(document, messageInstructions);
				case TransitDocDataContext.CIN750WarehouseCor:
					return new CIN750CorNotificationExtensions(document, messageInstructions);
				default:
					return null;
			}
		}

		public override IEnumerable<IMacroLibrary> GetLibraries(string dataContext)
		{
			yield break;
		}

		public override Either<string, object> GetAdditionalData(object obj, IStmMenuItem menuItem)
		{
			var contextTypes = GetContextTypes(menuItem);

			if (contextTypes != null && contextTypes.Length > 0 && obj is WhsItemReceiveConsignment rcn)
			{
				if (contextTypes.Contains(TransitDocDataContext.CIN750WarehouseIn))
				{
					return CIN750NotificationValidation.GetNotificationAdditionalData(rcn, CIN750NotificationMessageTypes.CIN750InNotification);
				}
				else if (contextTypes.Contains(TransitDocDataContext.CIN750WarehouseCor))
				{
					return CIN750NotificationValidation.GetNotificationAdditionalData(rcn, CIN750NotificationMessageTypes.CIN750CorNotification);
				}
			}

			return (object)null;
		}
	}
}
