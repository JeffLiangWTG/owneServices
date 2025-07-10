using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderEventParentFinder : EventParentFinder
	{
		internal OrderEventParentFinder(BusinessObjectFactory factory, OrderManagerOrderDataContextManager manager, IXmlImportLogger logger)
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

			var matcher = new OrderMatcher(factory, orderReferences, logger);
			var bestMatch = matcher.GetBestMatch();

			return bestMatch == null ? null : new BusinessObject[] { bestMatch };
		}
	}
}

