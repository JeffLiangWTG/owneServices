using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public class ContainerTrackingSubscriptionRequestedManager
	{
		public ContainerTrackingSubscriptionRequestedManager(IContainerTrackingProvider containerTrackingProvider)
		{
			this.containerTrackingProvider = Argument.NotNull(containerTrackingProvider, nameof(containerTrackingProvider));
			this.containers = containerTrackingProvider.Containers ?? Enumerable.Empty<ITrackableContainer>();
			ContainerWasRemoved = false;
		}

		public ContainerTrackingSubscriptionRequestedManager(ITrackableContainer container, IContainerTrackingProvider containerTrackingProvider)
		{
			this.containerTrackingProvider = Argument.NotNull(containerTrackingProvider, nameof(containerTrackingProvider));

			Argument.NotNull(container, nameof(container));
			this.containers = new[] { container };
			ContainerWasRemoved = false;
		}

		readonly IContainerTrackingProvider containerTrackingProvider;
		readonly IEnumerable<ITrackableContainer> containers;
		public bool ContainerWasRemoved { get; set; }

		public void UpdateIfNecessary()
		{
			if (FreightDataRegistry.Instance.ContainerAutomation.Value)
			{
				UpdateEventLog(containerTrackingProvider.Logs);
			}
		}

		public static ZString SubscriptionRequestedEventReference
		{
			get
			{
				var eventParameter = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
					Constants.EventReferenceParameterTypes.ContainerTracking);

				return StmALog.GenerateEventReference("", new[] { eventParameter });
			}
		}

		#region Implementation

		void UpdateEventLog(Logs logs)
		{
			var isCoLoadInformationFilled =
			containerTrackingProvider.SubscribeToContainersOnly && containerTrackingProvider.CoLoadWith != null &&
				(!containerTrackingProvider.CoLoadWithCarrierBookingReference.IsEmpty || !containerTrackingProvider.CoLoadWithMasterBillNumber.IsEmpty);

			var isMainCarrierInformationFilled = containerTrackingProvider.ShippingLine != null &&
				(!containerTrackingProvider.CarrierBookingReference.IsEmpty || !containerTrackingProvider.MasterBillNumber.IsEmpty);

			// CW1 doesn't send subscriptions for CoLoad consols when CBR is filled and MBN is not
			// For CoLoad we need to match BOL+container number. The reason is that carrier may have more than one container and BOL under one booking.
			// then they split the BL and assign to different containers.
			// that's why we create SBR from CoLoad consol when BOL+container number exist at early stages.
			var isCoLoadAndCbrOnlyFilled = containerTrackingProvider.SubscribeToContainersOnly &&
				!isCoLoadInformationFilled && isMainCarrierInformationFilled &&
				containerTrackingProvider.MasterBillNumber.IsEmpty;

			var isSea = containerTrackingProvider.TransportMode == Constants.TransportModes.Sea;
			var isRailOrRoad = containerTrackingProvider.TransportMode == Constants.TransportModes.Rail || containerTrackingProvider.TransportMode == Constants.TransportModes.Road;
			var isShippingLineOrNVOCCCarrier = containerTrackingProvider.ShippingLine != null
				&& (containerTrackingProvider.ShippingLine.OH_IsShippingLine || containerTrackingProvider.ShippingLine.OH_IsSeaWholesaler);

			var isContainerTrackingProviderApplicableForSubscription =
				(isSea || (isRailOrRoad && isShippingLineOrNVOCCCarrier))
				&& !isCoLoadAndCbrOnlyFilled
				&& (isCoLoadInformationFilled || isMainCarrierInformationFilled);

			if (isContainerTrackingProviderApplicableForSubscription)
			{
				UpdateEventLogIfRequired(logs);
			}
			else
			{
				CancelEventLogIfExists(logs);
			}
		}

		void UpdateEventLogIfRequired(Logs logs)
		{
			var updateRequired = GetExistingLog(logs) == null ||
				containerTrackingProvider.CarrierCodeHasChanges
				|| (containerTrackingProvider.SubscribeToContainersOnly && containerTrackingProvider.CoLoadHasChanges)
				// See the description about CBR and CoLoad consols above
				|| (!containerTrackingProvider.SubscribeToContainersOnly &&
					containerTrackingProvider.CarrierBookingReferenceHasChanges)
				|| containerTrackingProvider.MasterBillNumberHasChanges
				|| containerTrackingProvider.TransportModeHasChanges
				|| containerTrackingProvider.SubscribeToContainersOnlyHasChanges
				|| containerTrackingProvider.ContainerModeHasChanges
				|| HasContainerNumbersChanged
				|| ContainerWasRemoved
				|| containerTrackingProvider.RoutingLegsHaveChanges;

			if (updateRequired && !HasDehireEvent(logs))
			{
				logs.CreateRecreateOrUpdateEventLog(AutoEvents.SubscriptionRequested, EstimateActual.Actual, ZDateTimeOffset.Now, SubscriptionRequestedEventReference);
			}
		}

		bool HasDehireEvent(Logs logs)
		{
			return logs.MostRecentLogByEventTime(AutoEvents.Dehire, (log) => !log.SL_IsEstimate) != null;
		}

		bool HasContainerNumbersChanged
		{
			get { return containers.Any(c => c.ContainerNumberHasChanges); }
		}

		void CancelEventLogIfExists(Logs logs)
		{
			var existingLog = GetExistingLog(logs);
			if (existingLog != null)
			{
				existingLog.Cancel();
			}
		}

		StmALog GetExistingLog(Logs logs)
		{
			return logs.MostRecentLogByEventTime(AutoEvents.SubscriptionRequested, SubscriptionRequestedEventReference);
		}

		#endregion
	}
}
