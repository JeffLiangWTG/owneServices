using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public static class EventProcessorHelper
	{
		public static List<WhsOrderLine> GetEventRelatedOrderLines(ZString eventType, UniversalEvent xmlEvent, List<WhsOrderLine> cachedOrderLines, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			if (cachedOrderLines == null)
			{
				cachedOrderLines = new List<WhsOrderLine>();

				if (xmlEvent.EventReference is ZString eventReference)
				{
					var eventEntryNumber = GetEventEntryNumber(xmlEvent);
					var eventBGMRefence = GetEventBGMReference(xmlEvent);

					if (!eventEntryNumber.IsEmpty && !eventBGMRefence.IsEmpty)
					{
						var ordersQuery = new ZDBOnlyQuery(typeof(WhsOrder));
						ordersQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, eventBGMRefence);
						ordersQuery.AddToFilter(WhsDocketSchema.WD_CustomerReference, eventEntryNumber);
						var orders = factory.Load<WhsOrder>(ordersQuery);
						orders.ForEach(x => cachedOrderLines.AddRange(x.Lines.Cast<WhsOrderLine>()));
					}
					else
					{
						logger.Log(LogType.Information, Res.GetString("330D9EF7-E1B0-4E9A-8AA9-50CCDEA18B51", "The {0} event misses declaration context data.", eventType));
					}
				}
			}
			return cachedOrderLines;
		}

		public static ZString GetEventBGMReference(UniversalEvent xmlEvent) => xmlEvent.ContextCollection.FirstOrDefault(x => x.Type.Type is ZString stringValue && stringValue == nameof(IXmlEventValueObjectContextValueList.DeclarationReference))?.Value ?? ZString.Empty;

		public static ZString GetEventEntryNumber(UniversalEvent xmlEvent) => xmlEvent.ContextCollection.FirstOrDefault(x => x.Type.Type is ZString stringValue && stringValue == nameof(IXmlEventValueObjectContextValueList.EntryNumber))?.Value ?? ZString.Empty;
	}
}
