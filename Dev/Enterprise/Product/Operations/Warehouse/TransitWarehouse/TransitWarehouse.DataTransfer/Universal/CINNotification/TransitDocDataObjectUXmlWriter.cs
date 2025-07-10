using System.Diagnostics.CodeAnalysis;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.DataTransfer.Document;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;
using DataWritingManager = Enterprise.Warehouse.Transit.DataTransfer.Document.DataWritingManager;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public sealed class TransitDocDataObjectUXmlWriter : ITransitDocDataObjectUXmlWriter
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ITopLevelDataObject GetDataObject(IDataObjectWriterStrategy writerStrategy, IDocument document, MessageType messageType)
		{
			var manager = new DataWritingManager(writerStrategy);
			var docDataObject = document?.Data?.Value;
			var dataContext = document?.DataContext ?? string.Empty;
			switch (dataContext)
			{
				case TransitDocDataContext.CIN750WarehouseIn:
					if (docDataObject is CIN750InNotification inNotification)
					{
						var writer = new CIN750InNotificationWriter(manager);

						return writer.GetDataObject(inNotification);
					}
					break;

				case TransitDocDataContext.CIN750WarehouseCor:
					if (docDataObject is CIN750CorNotification corNotification)
					{
						var writer = new CIN750CorNotificationWriter(manager);

						return writer.GetDataObject(corNotification);
					}
					break;

				case TransitDocDataContext.CIN750WarehouseCons:
					if (docDataObject is CIN750ConsNotification consNotification)
					{
						var writer = new CIN750ConsNotificationWriter(manager);

						return writer.GetDataObject(consNotification);
					}
					break;

				case TransitDocDataContext.CIN750WarehouseDecons:
					if (docDataObject is CIN750DeconsNotification notification)
					{
						var writer = new CIN750DeconsNotificationWriter(manager);

						return writer.GetDataObject(notification);
					}
					break;

				case TransitDocDataContext.CIN750WarehouseOut:
					if (docDataObject is CIN750OutNotification outNotification)
					{
						var writer = new CIN750OutNotificationWriter(manager);

						return writer.GetDataObject(outNotification);
					}
					break;
			}
			return null;
		}
	}
}
