using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class BaseShipmentEventParentFinderHelper
	{
		protected BaseShipmentEventParentFinderHelper(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			this.factory = Argument.NotNull(factory, "BusinessObjectFactory factory");
			this.logger = Argument.NotNull(logger, "IXmlImportLogger logger");
		}

		protected readonly BusinessObjectFactory factory;
		protected readonly IXmlImportLogger logger;

		protected static void PopulateShipmentReference(IXmlEventValueObject xmlEvent, ShipmentReferences referencesParent, IUniversalFreightHelper helper)
		{
			Argument.NotNull(helper, "helper");

			referencesParent.HAWBNumber = xmlEvent.Context.HAWBNumber;
			referencesParent.HBOLNumber = xmlEvent.Context.HBOLNumber.GetValueOrDefault();
			referencesParent.ShippersReference = xmlEvent.Context.ShippersReference;
			referencesParent.CFSReference = xmlEvent.Context.CFSReference;

			if (helper.ShipmentHasOrders)
			{
				AddOrderNumbers(xmlEvent, referencesParent);
			}

			referencesParent.InterimReceipt = xmlEvent.Context.InterimReceipt;

			if (helper.ShipmentHasAdditionalReferences)
			{
				referencesParent.PopulateAdditionalReferences(xmlEvent);
			}

			referencesParent.OriginUNLOCO = xmlEvent.Context.HBOLOriginUNLOCO;
			referencesParent.DestinationUNLOCO = xmlEvent.Context.HBOLDestinationUNLOCO;
		}

		static void AddOrderNumbers(IXmlEventValueObject xmlEvent, ShipmentReferences referencesParent)
		{
			var orderNumbers = new List<ZString>();

			var uxmlEvent = xmlEvent as UniversalEvent;

			var contextCollection = uxmlEvent != null
				? uxmlEvent.ContextCollection
				: null;

			if (contextCollection == null)
			{
				return;
			}

			foreach (var contextItem in contextCollection)
			{
				var contextItemType = contextItem.Type;
				if (contextItemType.Type.GetValueOrDefault() == nameof(UniversalEvent.ContextTypes.OrderNumber))
				{
					orderNumbers.Add(contextItem.Value.GetValueOrDefault());
				}
			}

			referencesParent.OrderNumbers = orderNumbers;
		}
	}
}
