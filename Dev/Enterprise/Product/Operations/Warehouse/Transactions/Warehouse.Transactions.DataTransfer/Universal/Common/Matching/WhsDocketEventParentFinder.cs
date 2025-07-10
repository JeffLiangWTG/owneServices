using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsDocketEventParentFinder<TDocket> : EventParentFinder
		where TDocket : WhsDocket
	{
		protected WhsDocketEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected static List<KeyValuePair<ZString, ZString>> GetAdditionalReferencesForMatching(BusinessObjectFactory factory, UniversalEvent xmlEvent)
		{
			var references = new List<KeyValuePair<ZString, ZString>>();
			var referenceTypes = factory.GetCachedValue("WarehouseDataRegistry.Instance.AdditionalReferenceType", () => WarehouseDataRegistry.Instance.AdditionalReferenceType.Value);

			if (xmlEvent.ContextCollection != null)
			{
				foreach (var contextItem in xmlEvent.ContextCollection)
				{
					var contextItemType = contextItem.Type;
					if (contextItemType != null)
					{
						var referenceNumber = contextItem.Value.GetValueOrDefault();
						var referenceType = contextItemType.Type.GetValueOrDefault();
						if (!referenceNumber.IsEmpty && referenceTypes.ContainsCode(referenceType))
						{
							references.Add(new KeyValuePair<ZString, ZString>(referenceType, referenceNumber));
						}
					}
				}
			}

			return references;
		}
	}
}
