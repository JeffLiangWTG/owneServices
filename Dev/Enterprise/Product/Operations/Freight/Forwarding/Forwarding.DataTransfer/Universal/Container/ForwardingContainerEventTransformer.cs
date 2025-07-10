using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EventReference;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class ForwardingContainerEventTransformer
	{
		public static EventValue Transform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent = null, ForwardingContainer parentContainer = null)
		{
			Argument.NotNull(sourceEventValue, nameof(sourceEventValue));

			var evnt = sourceEventValue.EventType;
			var reference = sourceEventValue.Reference;
			IDictionary<string, string> parameters;

			if (sourceEventValue.Parameters.Count == 0)
			{
				parameters = StmALog.GetParametersFromReference(sourceEventValue.Reference);
			}
			else
			{
				parameters = sourceEventValue.Parameters.ToDictionary(s => s.Key, s => s.Value);
			}

			if (parameters != null)
			{
				string facility;
				parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Facility, out facility);

				var transformationKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", sourceEventValue.Code, facility);

				Event transformedEvent;
				if (containerTransformationDictionary.TryGetValue(transformationKey, out transformedEvent))
				{
					evnt = transformedEvent;
				}

				string type;
				parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Type, out type);

				if (string.IsNullOrEmpty(type))
				{
					UpdateEventTypeParameter(evnt, facility, parameters);
				}

				reference = UpdateEventLocationIfNecessary(evnt, sourceUniversalEvent, parentContainer, reference, parameters);
			}

			return new EventValue(evnt,
				sourceEventValue.IsEstimate,
				sourceEventValue.DeferFiringWorkflow,
				sourceEventValue.EventTime,
				reference,
				parameters);
		}

		static void UpdateEventTypeParameter(Event eventValue, string facility, IDictionary<string, string> parameters)
		{
			Argument.NotNull(eventValue, nameof(eventValue));

			if (facility == Constants.Facilities.Code.Consignor && eventValue.Code == Events.PickedUpCode
				|| facility == Constants.Facilities.Code.Consignee && eventValue.Code == Events.DeliveredCode)
			{
				parameters[Constants.EventReferenceParameters.Codes.Type] = Enterprise.Core.Constants.EventReferenceParameterTypes.FullContainer;
			}

			if (facility == Constants.Facilities.Code.Consignor && eventValue.Code == Events.DeliveredCode
				|| facility == Constants.Facilities.Code.Consignee && eventValue.Code == Events.PickedUpCode)
			{
				parameters[Constants.EventReferenceParameters.Codes.Type] = Enterprise.Core.Constants.EventReferenceParameterTypes.EmptyContainer;
			}
		}

		static string UpdateEventLocationIfNecessary(Event eventValue, UniversalEvent sourceUniversalEvent, ForwardingContainer parentContainer, string slReference, IDictionary<string, string> parameters)
		{
			if (sourceUniversalEvent != null && parentContainer != null && EventHasTransportBookingDataSource(sourceUniversalEvent) && !CanTransportBookingProvideTrustworthyLocation)
			{
				var eventLocation = TryGetEventLocationFromContainer(eventValue, parentContainer, parameters);
				if (!string.IsNullOrEmpty(eventLocation))
				{
					return UpdateEventLocation(eventLocation, slReference, parameters);
				}
			}

			return slReference;
		}

		static bool EventHasTransportBookingDataSource(UniversalEvent sourceUniversalEvent)
		{
			var dataSourceCollection = sourceUniversalEvent?.DataContext?.DataSourceCollection;
			if (dataSourceCollection == null)
			{
				return false;
			}

			var transportBookingDataContexts = new[]
			{
				nameof(DataContextType.TransportBooking),
				nameof(DataContextType.TransportBookingConfirmation),
				nameof(DataContextType.TransportBookingConsolidation)
			};

			return dataSourceCollection.Any(dataSource => transportBookingDataContexts.Contains(dataSource.Type?.ToString() ?? string.Empty));
		}

		static bool CanTransportBookingProvideTrustworthyLocation
		{
			get { return false; }
		}

		static string TryGetEventLocationFromContainer(Event eventValue, ForwardingContainer parentContainer, IDictionary<string, string> parameters)
		{
			if (parameters != null)
			{
				string facility;
				parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Facility, out facility);

				if (!string.IsNullOrEmpty(facility) && facility.Equals(Constants.Facilities.Code.Terminal, StringComparison.OrdinalIgnoreCase))
				{
					return parentContainer.GetEventLocationForTerminal(eventValue.Code);
				}

				if (!string.IsNullOrEmpty(facility) && facility.Equals(Constants.Facilities.Code.ContainerYard, StringComparison.OrdinalIgnoreCase))
				{
					return parentContainer.GetEventLocationForContainerYard(eventValue.Code);
				}
			}

			return string.Empty;
		}

		static string UpdateEventLocation(string updatedLocation, string slReference, IDictionary<string, string> parameters)
		{
			if (parameters != null)
			{
				var freeText = StmALog.GetFreeTextFromReference(slReference);

				string location;
				if (parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Location, out location))
				{
					if (location.Equals(updatedLocation, StringComparison.OrdinalIgnoreCase))
					{
						return slReference;
					}

					freeText = string.IsNullOrEmpty(freeText)
						? location
						: string.Concat(location, ",", freeText);
				}

				parameters[Constants.EventReferenceParameters.Codes.Location] = updatedLocation;

				return StmALog.GenerateEventReferenceToFitInReferenceMaxLength(freeText, parameters);
			}
			else
			{
				return slReference;
			}
		}

		static readonly ImmutableDictionary<string, Event> containerTransformationDictionary = new Dictionary<string, Event>()
		{
			{ string.Format(CultureInfo.InvariantCulture, "{0}|{1}", Events.PickedUpCode, Constants.Facilities.Code.ContainerYard), Events.GateOut },
			{ string.Format(CultureInfo.InvariantCulture, "{0}|{1}", Events.DeliveredCode, Constants.Facilities.Code.Terminal), Events.GateIn },
			{ string.Format(CultureInfo.InvariantCulture, "{0}|{1}", Events.PickedUpCode, Constants.Facilities.Code.Terminal), Events.GateOut },
			{ string.Format(CultureInfo.InvariantCulture, "{0}|{1}", Events.DeliveredCode, Constants.Facilities.Code.ContainerYard), Events.GateIn }
		}.ToImmutableDictionary();
	}
}
