using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.ContainerYard.Business
{
	public sealed class GateTransportEventParentFinder : EventParentFinder
	{
		public GateTransportEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger) : base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalDataBuss.DataObjects.Universal.Event xmlEvent)
		{
			var eventValueObject = (IXmlEventValueObject)xmlEvent;

			if (eventValueObject.EventType != Events.FreightUnloadedCode && eventValueObject.EventType != Events.FreightLoadedCode)
			{
				return null;
			}

			var gateReferences = new GateReference();

			if ((eventValueObject.Context.ContainerNumbers?.Count ?? 0) > 0)
			{
				foreach (var containerNumber in eventValueObject.Context.ContainerNumbers)
				{
					gateReferences.ContainerNumbers.Add(containerNumber);
				}
			}
			else if (!string.IsNullOrEmpty(eventValueObject.Context.TransportReference))
			{
				gateReferences.VehicleRegistrationNumber = eventValueObject.Context.TransportReference;
			}

			var matcher = new GateTransportMatcher(factory, gateReferences, logger);
			var bestMatch = matcher.GetBestMatch();

			return bestMatch == null ? null : new BusinessObject[] { bestMatch };
		}
	}
}
