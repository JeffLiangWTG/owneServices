using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderLineEventParentFinder : EventParentFinder
	{
		internal OrderLineEventParentFinder(BusinessObjectFactory factory, OrderLineDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			var eventValueObject = (IXmlEventValueObject)eventDataObject;

			var orderReferences = new OrderReferences();
			orderReferences.OrderNumber = eventValueObject.Context.OrderNumber;
			orderReferences.OrderNumberSplit = eventValueObject.Context.OrderNumberSplit;
			orderReferences.MAWBNumber = eventValueObject.Context.MAWBNumber;
			orderReferences.MBOLNumber = eventValueObject.Context.MBOLNumber.GetValueOrDefault();
			orderReferences.HAWBNumber = eventValueObject.Context.HAWBNumber;
			orderReferences.HBOLNumber = eventValueObject.Context.HBOLNumber.GetValueOrDefault();
			orderReferences.InvoiceNumber = eventValueObject.Context.CommercialInvoiceNumber;
			orderReferences.ShippersReference = eventValueObject.Context.ShippersReference;
			orderReferences.OriginUNLOCO = eventValueObject.Context.HBOLOriginUNLOCO;
			orderReferences.DestinationUNLOCO = eventValueObject.Context.HBOLDestinationUNLOCO;

			var orderLineNumber = eventValueObject.Context.OrderLineNumber;
			var orderLineSubLineNumber = eventValueObject.Context.OrderLineSubLineNumber;

			if (orderLineNumber != null && orderLineSubLineNumber != null)
			{
				var matcher = new OrderMatcher(factory, orderReferences, logger);
				var bestMatchOrder = matcher.GetBestMatch();

				if (bestMatchOrder != null)
				{
					var orderLine = bestMatchOrder.OrderLines
						.FirstOrDefault(l => l.JO_LineNo == orderLineNumber
							&& l.JO_SubLineNo == orderLineSubLineNumber);

					if (orderLine != null)
					{
						return new BusinessObject[] { orderLine };
					}
				}
			}

			return null;
		}
	}
}

