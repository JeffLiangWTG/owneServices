using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class ContainerStockEventParentFinder : EventParentFinder
	{
		public ContainerStockEventParentFinder(BusinessObjectFactory factory, ContainerStockDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			var results = new List<BusinessObject>();
			if (IsEventSupported(eventDataObject))
			{
				var processor = new UniversalCMMMessageProcessor(factory);
				processor.ProcessEvent(eventDataObject);
			}
			var eventValueObject = (IXmlEventValueObject)eventDataObject;
			var context = eventValueObject.Context;
			var containerNumbers = context.ContainerNumbers != null && context.ContainerNumbers.Any() ? context.ContainerNumbers : new List<ZString> { "" };
			foreach (var containerNumber in containerNumbers)
			{
				AddBestMatchForContainer(containerNumber, results);
			}

			return results.Any() ? results.ToArray() : null;
		}

		void AddBestMatchForContainer(ZString containerNumber, List<BusinessObject> bizOs)
		{
			var containerReferences = new ContainerStockReferences { ContainerNumber = containerNumber };
			var matcher = new ContainerStockMatcher(factory, containerReferences, new DummyLogger());
			var bestMatch = matcher.GetBestMatch();
			if (bestMatch != null)
			{
				bizOs.Add(bestMatch);
			}
		}

		bool IsEventSupported(UniversalEvent eventDataObject)
		{
			if (!eventDataObject.EventType.HasValue ||
				!SupportedEvents.Contains(eventDataObject.EventType.ToString()))
			{
				return false;
			}

			IXmlEventValueObject eventValueObject = eventDataObject;
			var eventContextValueTypes = eventValueObject.Context != null
				? eventValueObject.Context.Values.Select(val => val.Key.Type.ToString())
				: Enumerable.Empty<string>();

			return new HashSet<string>(MandatoryContextValueTypes).IsSubsetOf(eventContextValueTypes);
		}

		IEnumerable<string> SupportedEvents
		{
			get
			{
				yield return Events.FreightLoadedCode;      // "FLO"
				yield return Events.FreightUnloadedCode;    // "FUL"
				yield return Events.GateInCode;             // "GIN"
				yield return Events.GateOutCode;            // "GOU"
			}
		}

		IEnumerable<string> MandatoryContextValueTypes
		{
			get
			{
				yield return nameof(UniversalEvent.ContextTypes.ContainerNumber);
				yield return nameof(UniversalEvent.ContextTypes.DepotCode);
			}
		}
	}
}





