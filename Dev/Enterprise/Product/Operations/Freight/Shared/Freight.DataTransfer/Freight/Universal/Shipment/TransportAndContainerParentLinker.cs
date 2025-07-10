using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Freight.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.DataTransfer.Universal
{
	abstract class TransportAndContainerParentLinker<T>
		where T : BusinessObject, ITransportParent
	{
		protected TransportAndContainerParentLinker(BusinessObjectFactory factory, IUniversalFreightHelper helper)
		{
			this.factory = factory;
			this.helper = Argument.NotNull(helper, "helper");
		}

		protected readonly BusinessObjectFactory factory;
		protected readonly IUniversalFreightHelper helper;

		internal BusinessObject[] GetLogParent(T parent, IXmlEventValueObject xmlEvent, bool isAir)
		{
			var eventLegPorts = new EventLegPorts(xmlEvent, parent.Factory);
			var containers = GetContainerLinker(parent).GetLogParent(xmlEvent);
			var eventShouldBeLinkedToContainer = EventIsRailOrRoad(xmlEvent) && ConsolIsSea(parent) && containers != null && containers.Any();
			var eventTypeShouldLinkTransportByPorts = EventTransformerHelper.IsVesselEvent(xmlEvent.EventType)
				|| xmlEvent.EventType == AutoEvents.ArrivalCode
				|| xmlEvent.EventType == AutoEvents.DepartureCode
				|| xmlEvent.EventType == AutoEvents.FreightLoadedCode;

			var linkTransportByPorts = eventLegPorts.HasAPort && eventTypeShouldLinkTransportByPorts;

			if (linkTransportByPorts && !eventShouldBeLinkedToContainer)
			{
				var parents = GetParentTransports(parent, xmlEvent, eventLegPorts);
				if (parents != null)
				{
					return parents;
				}
			}

			if (containers != null && containers.Any())
			{
				return containers.ToArray();
			}

			if (!linkTransportByPorts)
			{
				var transportID = isAir ? xmlEvent.Context.FlightNumber.GetValueOrDefault() : (ZString)(xmlEvent.Context.VoyageNumber.GetValueOrDefault() + xmlEvent.Context.VesselName.GetValueOrDefault());
				if (!transportID.IsEmpty || eventLegPorts.HasAPort)
				{
					var parents = GetParentTransports(parent, xmlEvent, eventLegPorts);
					if (parents != null)
					{
						return parents;
					}
				}
			}

			return new BusinessObject[] { parent };
		}

		ZBool ConsolIsSea(T parent)
		{
			if (parent is CommonConsol consol)
			{
				return consol.JK_TransportMode.EqualsIgnoringCase(Core.Constants.TransportModes.Sea);
			}
			return ZBool.False;
		}

		ZBool EventIsRailOrRoad(IXmlEventValueObject xmlEvent)
		{
			ZBool eventIsRailOrRoad;
			var eventTransportMode = ZString.Empty;

			if (xmlEvent is UniversalDataBuss.DataObjects.Universal.Event concreteEvent && !string.IsNullOrEmpty(concreteEvent.EventParameters?.TransportMode.GetValueOrDefault()))
			{
				eventTransportMode = concreteEvent.EventParameters.TransportMode.Value;
			}

			eventIsRailOrRoad = eventTransportMode.EqualsIgnoringCase(Core.Constants.TransportModes.Rail) || eventTransportMode.EqualsIgnoringCase(Core.Constants.TransportModes.Road);
			return eventIsRailOrRoad;
		}

		Transport[] GetParentTransports(T parent, IXmlEventValueObject xmlEvent, EventLegPorts eventLegPorts)
		{
			var transport = new TransportLinker().GetLogParent(parent, xmlEvent, eventLegPorts);
			if (transport != null)
			{
				return new Transport[] { transport };
			}

			return null;
		}

		protected abstract IContainerLinker GetContainerLinker(T parent);
	}
}
