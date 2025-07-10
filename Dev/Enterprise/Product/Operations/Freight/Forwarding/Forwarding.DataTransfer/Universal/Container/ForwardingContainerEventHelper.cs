using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;
using EventReferenceParameterTypes = Enterprise.Core.Constants.EventReferenceParameterTypes;
using Facilities = CargoWise.EventReference.Constants.Facilities.Code;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class ForwardingContainerEventHelper
	{
		public static bool IsExportPickupOrDeliveryEvent(ZString eventCode, ZString eventReference)
		{
			return IsPickupOrDeliveryEventMatched(eventCode, eventReference, exportContainerPickupOrDeliveryEvents.Value);
		}

		public static bool IsImportPickupOrDeliveryEvent(ZString eventCode, ZString eventReference)
		{
			return IsPickupOrDeliveryEventMatched(eventCode, eventReference, importContainerPickupOrDeliveryEvents.Value);
		}

		#region Implementation

		static string EventKey(string eventCode, string facility, string containerType)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}", eventCode, facility, containerType);
		}

		static bool IsPickupOrDeliveryEventMatched(ZString eventCode, ZString eventReference, HashSet<string> eventKeys)
		{
			if (eventCode == Events.PickedUpCode || eventCode == Events.DeliveredCode)
			{
				var parameters = StmALog.GetParametersFromReference(eventReference) as IDictionary<string, string>;

				string facility;
				parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, out facility);

				string containerType;
				parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out containerType);

				string eventKey = EventKey(eventCode, facility, containerType);

				return eventKeys.Contains(eventKey);
			}

			return false;
		}

		[ThreadSafe]
		static readonly Lazy<HashSet<string>> exportContainerPickupOrDeliveryEvents = new Lazy<HashSet<string>>
		(
			() => GetExportContainerPickupOrDeliveryEvents()
		);

		[ThreadSafe]
		static readonly Lazy<HashSet<string>> importContainerPickupOrDeliveryEvents = new Lazy<HashSet<string>>
		(
			() => GetImportContainerPickupOrDeliveryEvent()
		);

		static HashSet<string> GetExportContainerPickupOrDeliveryEvents()
		{
			return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				// Empty container is released from a container yard
				{ EventKey(Events.PickedUpCode,     Facilities.ContainerYard,   EventReferenceParameterTypes.EmptyContainer) },
				{ EventKey(Events.PickedUpCode,     Facilities.ContainerYard,   "") },

				// Empty container is delivered to consignor/cfs depot
				{ EventKey(Events.DeliveredCode,    Facilities.Consignor,   EventReferenceParameterTypes.EmptyContainer) },
				{ EventKey(Events.DeliveredCode,    Facilities.Consignor,   "") },
				{ EventKey(Events.DeliveredCode,    Facilities.Depot,       EventReferenceParameterTypes.EmptyContainer) },
				{ EventKey(Events.DeliveredCode,    "",                     EventReferenceParameterTypes.EmptyContainer) },

				// Full container is picked up from consignor/cfs depot
				{ EventKey(Events.PickedUpCode,     Facilities.Consignor,   EventReferenceParameterTypes.FullContainer) },
				{ EventKey(Events.PickedUpCode,     Facilities.Consignor,   "") },
				{ EventKey(Events.PickedUpCode,     Facilities.Depot,       EventReferenceParameterTypes.FullContainer) },
				{ EventKey(Events.PickedUpCode,     "",                     EventReferenceParameterTypes.FullContainer) },

				// Full container is delivered to a terminal/wharf
				{ EventKey(Events.DeliveredCode,    Facilities.Terminal,   EventReferenceParameterTypes.FullContainer) },
				{ EventKey(Events.DeliveredCode,    Facilities.Terminal,   "") },
			};
		}

		static HashSet<string> GetImportContainerPickupOrDeliveryEvent()
		{
			return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				// Full container is picked up from a terminal/wharf
				{ EventKey(Events.PickedUpCode,     Facilities.Terminal,   EventReferenceParameterTypes.FullContainer) },
				{ EventKey(Events.PickedUpCode,     Facilities.Terminal,   "") },

				// Full container is delivered to consignee/cfs depot
				{ EventKey(Events.DeliveredCode,    Facilities.Consignee,   EventReferenceParameterTypes.FullContainer) },
				{ EventKey(Events.DeliveredCode,    Facilities.Consignee,   "") },
				{ EventKey(Events.DeliveredCode,    Facilities.Depot,       EventReferenceParameterTypes.FullContainer) },
				{ EventKey(Events.DeliveredCode,    "",                     EventReferenceParameterTypes.FullContainer) },

				// Empty container is picked up from consignee/cfs depot
				{ EventKey(Events.PickedUpCode,     Facilities.Consignee,   EventReferenceParameterTypes.EmptyContainer) },
				{ EventKey(Events.PickedUpCode,     Facilities.Consignee,   "") },
				{ EventKey(Events.PickedUpCode,     Facilities.Depot,       EventReferenceParameterTypes.EmptyContainer) },
				{ EventKey(Events.PickedUpCode,     "",                     EventReferenceParameterTypes.EmptyContainer) },

				// Empty container is returned to container yard
				{ EventKey(Events.DeliveredCode,    Facilities.ContainerYard,   EventReferenceParameterTypes.EmptyContainer) },
				{ EventKey(Events.DeliveredCode,    Facilities.ContainerYard,   "") },
			};
		}

		#endregion
	}
}
