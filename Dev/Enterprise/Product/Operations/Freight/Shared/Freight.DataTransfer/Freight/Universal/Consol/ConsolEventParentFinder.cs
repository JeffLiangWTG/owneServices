using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ConsolEventParentFinder<TConsol, TShipment, TContainer>
		: ConsolAndShipmentEventParentFinder<TConsol, TShipment, TContainer>
		where TConsol : CommonConsol
		where TShipment : CommonShipment
		where TContainer : CommonContainer
	{
		public ConsolEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger, IUniversalFreightHelper helper)
			: base(factory, manager, logger, helper)
		{
		}

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, Event eventData)
		{
			var result = new List<BusinessObject>();

			var logParent = logParents[0];
			var consol = logParent as TConsol;

			if (consol != null)
			{
				var linker = new ConsolLinker<TConsol>(factory, helper);
				var consolLogParents = linker.GetLogParent(consol, eventData, consol.IsAir);
				result.AddRange(consolLogParents != null && consolLogParents.Any() ? consolLogParents : new BusinessObject[] { logParent });

				MatchConsolResultService.UnRegister(factory);

				var supporter = consol.GetSupporter();
				var documentEventParent = supporter?.GetEventParent(eventData);

				if (documentEventParent is BusinessObject bizObj)
				{
					result.Add(bizObj);
				}
			}
			else
			{
				var shipment = logParent as TShipment;
				var linker = new ShipmentLinker<TShipment>(factory, helper);
				var linkerLogParents = linker.GetLogParent(shipment, eventData);
				result.AddRange(linkerLogParents != null && linkerLogParents.Any() ? linkerLogParents : new BusinessObject[] { logParent });
			}

			return base.GetChildrenIfSpecifiedInContext(result.ToArray(), eventData);
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			if (xmlEvent.EventType.GetValueOrDefault() == AutoEvents.SubscriptionRequestedCode)
			{
				return null;
			}

			return base.GetLogParentsForEventUsingContext(xmlEvent);
		}
	}
}
