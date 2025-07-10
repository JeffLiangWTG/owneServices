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
	public sealed class DispatchConsignmentVisualizableDocumentSupporter : TransitWarehouseVisualizableDocumentSupporter<WhsItemDispatchConsignment>
	{
		public DispatchConsignmentVisualizableDocumentSupporter(WhsItemDispatchConsignment dcn)
			: base(dcn)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.WhsItemDispatchConsignment;
		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;

		protected override string GetDataStoreNameFromDocumentName(string documentName, EventParameters eventParameters)
		{
			switch (documentName)
			{
				case TransitDocumentNames.CIN750ConsNotification:
					return TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750ConsFromDCN;
				case TransitDocumentNames.CIN750DeconsNotification:
					return TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750DeconsFromDCN;
				case TransitDocumentNames.CIN750OutNotification:
					return TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750OutFromDCN;
				default:
					return ZString.Empty;
			}
		}

		public override IMessageLogCreator GetMessageLogCreator(IDocument document)
		{
			switch (document?.DataContext)
			{
				case TransitDocDataContext.CIN750WarehouseDecons:
					return new CIN750DeconsNotificationLogsCreator();
				case TransitDocDataContext.CIN750WarehouseOut:
					return new CIN750OutNotificationLogsCreator();
				case TransitDocDataContext.CIN750WarehouseCons:
					return new CIN750ConsNotificationLogsCreator();
				default:
					return null;
			}
		}

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions)
		{
			switch (document?.DataContext)
			{
				case TransitDocDataContext.CIN750WarehouseDecons:
					return new CIN750DeconsNotificationExtensions(document, messageInstructions);
				case TransitDocDataContext.CIN750WarehouseOut:
					return new CIN750OutNotificationExtensions(document, messageInstructions);
				case TransitDocDataContext.CIN750WarehouseCons:
					return new CIN750ConsNotificationExtensions(document, messageInstructions);
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
			if (contextTypes == null)
			{
				return (object)null;
			}
			else if (obj is WhsItemDispatchConsignment dcnForOut && contextTypes.Contains(TransitDocDataContext.CIN750WarehouseOut))
			{
				return CIN750NotificationValidation.GetDCNNotificationAdditionalData(dcnForOut, CIN750NotificationMessageTypes.CIN750OutNotification);
			}
			else if (obj is WhsItemDispatchConsignment dcnForDecons && contextTypes.Contains(TransitDocDataContext.CIN750WarehouseDecons))
			{
				return CIN750NotificationValidation.GetDCNNotificationAdditionalData(dcnForDecons, CIN750NotificationMessageTypes.CIN750DeconsNotification);
			}
			else if (obj is WhsItemDispatchConsignment dcnForCons && contextTypes.Contains(TransitDocDataContext.CIN750WarehouseCons))
			{
				return CIN750NotificationValidation.GetDCNNotificationAdditionalData(dcnForCons, CIN750NotificationMessageTypes.CIN750ConsNotification);
			}
			else
			{
				return (object)null;
			}
		}
	}
}
