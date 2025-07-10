using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	internal class LocalTransportEventParentFinder : EventParentFinder
	{
		internal LocalTransportEventParentFinder(BusinessObjectFactory factory, LocalTransportDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			var eventValueObject = (IXmlEventValueObject)xmlEvent;
			var cartageReferences = new CartageReferences();

			cartageReferences.ConnoteNumber = eventValueObject.Context.ConsignmentNoteNumber;
			cartageReferences.ClientOrderNumber = eventValueObject.Context.OrderNumber;
			cartageReferences.WaybillNumber = eventValueObject.Context.WaybillNumber;
			cartageReferences.QuoteNumber = eventValueObject.Context.QuoteNumber;
			cartageReferences.References = GetValidEventReferences(xmlEvent);

			var matcher = new CartageMatcher(factory, cartageReferences, logger);
			var bestMatch = matcher.GetBestMatch();

			return bestMatch != null ? new BusinessObject[] { bestMatch } : null;
		}

		List<KeyValuePair<ZString, ZString>> GetValidEventReferences(Event xmlEvent)
		{
			var result = new List<KeyValuePair<ZString, ZString>>();

			if (xmlEvent.ContextCollection != null)
			{
				foreach (var contextItem in xmlEvent.ContextCollection)
				{
					var referenceNumber = contextItem.Value.GetValueOrDefault();
					var referenceType = contextItem.Type == null ? ZString.Empty : contextItem.Type.Type.GetValueOrDefault();

					if (!referenceNumber.IsEmpty && ReferenceTypes.ContainsCode(referenceType))
					{
						result.Add(new KeyValuePair<ZString, ZString>(referenceType, referenceNumber));
					}
				}
			}

			return result;
		}

		ICodeDescriptionPairListWithDefaultCode ReferenceTypes
		{
			get { return referencesTypes ?? (referencesTypes = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value); }
		}

		ICodeDescriptionPairListWithDefaultCode referencesTypes;
	}
}
