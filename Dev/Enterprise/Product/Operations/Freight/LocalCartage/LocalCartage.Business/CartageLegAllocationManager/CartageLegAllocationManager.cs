using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business.GPS;
using Enterprise.GPS.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageLegAllocationManager : NonPersistentBusinessObject
	{
		public CartageLegAllocationManager(CommonCartageLeg cartageLeg)
			: base(cartageLeg.Factory)
		{
			this.cartageLeg = cartageLeg;
		}

		readonly CommonCartageLeg cartageLeg;

		public CommonCartageLeg CartageLeg { get { return cartageLeg; } }

		public ZDateTime PickupSelectedIn
		{
			get { return pickupSelectedIn; }
			private set
			{
				pickupSelectedIn = value;
				PickupSelectedInInfo.RefreshBinding();
			}
		}
		ZDateTime pickupSelectedIn;

		public ZPropertyInfo PickupSelectedInInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(PickupSelectedIn));
			}
		}

		public ZDateTime PickupSelectedOut
		{
			get { return pickupSelectedOut; }
			private set
			{
				pickupSelectedOut = value;
				PickupSelectedOutInfo.RefreshBinding();
			}
		}
		ZDateTime pickupSelectedOut;

		public ZPropertyInfo PickupSelectedOutInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(PickupSelectedOut));
			}
		}

		public ZDateTime WaitingPointSelectedIn
		{
			get { return waitingPointSelectedIn; }
			private set
			{
				waitingPointSelectedIn = value;
				WaitingPointSelectedInInfo.RefreshBinding();
			}
		}
		ZDateTime waitingPointSelectedIn;

		public ZPropertyInfo WaitingPointSelectedInInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(WaitingPointSelectedIn));
			}
		}

		public ZDateTime WaitingPointSelectedOut
		{
			get { return waitingPointSelectedOut; }
			private set
			{
				waitingPointSelectedOut = value;
				WaitingPointSelectedOutInfo.RefreshBinding();
			}
		}
		ZDateTime waitingPointSelectedOut;

		public ZPropertyInfo WaitingPointSelectedOutInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(WaitingPointSelectedOut));
			}
		}

		public ZDateTime DeliverySelectedIn
		{
			get { return deliverySelectedIn; }
			private set
			{
				deliverySelectedIn = value;
				DeliverySelectedInInfo.RefreshBinding();
			}
		}
		ZDateTime deliverySelectedIn;

		public ZPropertyInfo DeliverySelectedInInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(DeliverySelectedIn));
			}
		}

		public ZDateTime DeliverySelectedOut
		{
			get { return deliverySelectedOut; }
			private set
			{
				deliverySelectedOut = value;
				DeliverySelectedOutInfo.RefreshBinding();
			}
		}
		ZDateTime deliverySelectedOut;

		public ZPropertyInfo DeliverySelectedOutInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(DeliverySelectedOut));
			}
		}

		Leg Leg
		{
			get { return leg ?? (leg = new Leg(CartageLeg)); }
		}
		Leg leg;

		string PickupAddress
		{
			get { return pickupAddress ?? (pickupAddress = Leg.Pickup.AddressCode); }
		}
		string pickupAddress;

		string WaitingPointAddress
		{
			get { return waitingPointAddress ?? (waitingPointAddress = CartageLeg.HasWaitPoint ? Leg.WaitPoint.AddressCode : null); }
		}
		string waitingPointAddress;

		string DeliveryAddress
		{
			get { return deliveryAddress ?? (deliveryAddress = Leg.Delivery.AddressCode); }
		}
		string deliveryAddress;

		GPSEventsHelper GPSEventsHelper
		{
			get { return gpsEventsHelper ?? (gpsEventsHelper = new GPSEventsHelper(CartageLeg.WorkSheet)); }
		}
		GPSEventsHelper gpsEventsHelper;

		public GPSEventAllocationCollection GPSEventAllocations
		{
			get
			{
				if (gpsEventAllocations == null)
				{
					var validAddresses = new List<string>();
					validAddresses.Add(PickupAddress);
					if (CartageLeg.HasWaitPoint)
					{
						validAddresses.Add(WaitingPointAddress);
					}
					validAddresses.Add(DeliveryAddress);

					GPSEventsHelper.PopulateEventLeg(CartageLeg.WorkSheet);

					var gpsEvents = CartageLeg.WorkSheet.GPSEvents.Cast<GPSEvent>().Where(g => validAddresses.Contains(g.EventShortInfo));

					gpsEventAllocations = new GPSEventAllocationCollection(gpsEvents, this);

					SelectDefault(gpsEventAllocations);

					gpsEventAllocations.Sort("GPSEvent+EventTime");
				}

				return gpsEventAllocations;
			}
		}
		GPSEventAllocationCollection gpsEventAllocations;

		void SelectDefault(GPSEventAllocationCollection gpsEventAllocs)
		{
			Argument.NotNull(gpsEventAllocs, "gpsEventAllocations", "gpsEventAllocations should not be null.");

			var allocations = gpsEventAllocs.Cast<GPSEventAllocation>().Where(a => a.GPSEvent.EventLeg == null).OrderBy(a => a.GPSEvent.EventTime);

			var pickupInHasToBeSelected = IsAddressInList(PickupAddress, InOut.In, allocations);
			var pickupOutHasToBeSelected = IsAddressInList(PickupAddress, InOut.Out, allocations);
			var waitPointInHasToBeSelected = CartageLeg.HasWaitPoint && IsAddressInList(WaitingPointAddress, InOut.In, allocations);
			var waitPointOutHasToBeSelected = CartageLeg.HasWaitPoint && IsAddressInList(WaitingPointAddress, InOut.Out, allocations);
			var deliveryInHasToBeSelected = IsAddressInList(DeliveryAddress, InOut.In, allocations);
			var deliveryOutHasToBeSelected = IsAddressInList(DeliveryAddress, InOut.Out, allocations);

			foreach (GPSEventAllocation gpsEventAllocation in allocations)
			{
				if (pickupInHasToBeSelected)
				{
					pickupInHasToBeSelected = !SelectIfValidForAddressAndDirection(gpsEventAllocation, PickupAddress, InOut.In);
				}
				else if (pickupOutHasToBeSelected)
				{
					pickupOutHasToBeSelected = !SelectIfValidForAddressAndDirection(gpsEventAllocation, PickupAddress, InOut.Out);
				}
				else if (CartageLeg.HasWaitPoint && waitPointInHasToBeSelected)
				{
					waitPointInHasToBeSelected = !SelectIfValidForAddressAndDirection(gpsEventAllocation, WaitingPointAddress, InOut.In);
				}
				else if (CartageLeg.HasWaitPoint && waitPointOutHasToBeSelected)
				{
					waitPointOutHasToBeSelected = !SelectIfValidForAddressAndDirection(gpsEventAllocation, WaitingPointAddress, InOut.Out);
				}
				else if (deliveryInHasToBeSelected)
				{
					deliveryInHasToBeSelected = !SelectIfValidForAddressAndDirection(gpsEventAllocation, DeliveryAddress, InOut.In);
				}
				else if (deliveryOutHasToBeSelected)
				{
					deliveryOutHasToBeSelected = !SelectIfValidForAddressAndDirection(gpsEventAllocation, DeliveryAddress, InOut.Out);
					if (!deliveryOutHasToBeSelected)
					{
						break;
					}
				}
			}
		}

		bool IsAddressInList(string address, InOut inOut, IOrderedEnumerable<GPSEventAllocation> gpsEventAllocs)
		{
			return gpsEventAllocs.Any(g => IsValidAddressAndDirection(g, address, inOut));
		}

		bool SelectIfValidForAddressAndDirection(GPSEventAllocation gpsEventAllocation, string address, InOut inOut)
		{
			return gpsEventAllocation.Selected = IsValidAddressAndDirection(gpsEventAllocation, address, inOut);
		}

		bool IsValidAddressAndDirection(GPSEventAllocation gpsEventAllocation, LegSegment legSegment, InOut inOut)
		{
			Argument.NotNull(legSegment, "legSegment", "legSegment should not be null.");

			string address = null;
			if (legSegment.Type == LegSegmentType.PickUp)
			{
				address = PickupAddress;
			}
			else if (legSegment.Type == LegSegmentType.WaitPoint)
			{
				address = WaitingPointAddress;
			}
			else if (legSegment.Type == LegSegmentType.Delivery)
			{
				address = DeliveryAddress;
			}

			return address != null && IsValidAddressAndDirection(gpsEventAllocation, address, inOut);
		}

		internal bool IsValidAddressAndDirection(GPSEventAllocation gpsEventAllocation, string address, InOut inOut)
		{
			Argument.NotNull(gpsEventAllocation, "gpsEventAllocation", "gpsEventAllocation should not be null.");
			Argument.NotNull(address, "address", "address should not be null.");

			var validActivityTypes = new List<string>();
			if (inOut == InOut.In)
			{
				validActivityTypes.Add(GPSConstants.GPSInOutActivityType.Codes.GIN);
			}
			else
			{
				validActivityTypes.Add(GPSConstants.GPSInOutActivityType.Codes.GOT);
			}

			return (gpsEventAllocation.GPSEvent != null) && validActivityTypes.Contains(gpsEventAllocation.GPSEvent.EventTypeCode) && gpsEventAllocation.GPSEvent.EventShortInfo.Equals(address);
		}

		internal ZString GetSegmentTypeDescriptionFromAddress(string address)
		{
			var type = ZString.Empty;

			if (address == PickupAddress)
			{
				type = Leg.Pickup.TypeDescription;
			}
			else if (address == WaitingPointAddress)
			{
				type = Leg.WaitPoint.TypeDescription;
			}
			else if (address == DeliveryAddress)
			{
				type = Leg.Delivery.TypeDescription;
			}

			return type;
		}

		public void SetupAllSelectedGpsEvent()
		{
			GPSEvent pickupInGpsEvent = null;
			GPSEvent pickupOutGpsEvent = null;
			GPSEvent waitingPointInGpsEvent = null;
			GPSEvent waitingPointOutGpsEvent = null;
			GPSEvent deliveryInGpsEvent = null;
			GPSEvent deliveryOutGpsEvent = null;

			SelectedGPSEventsForLegSegments(out pickupInGpsEvent, out pickupOutGpsEvent, out waitingPointInGpsEvent, out waitingPointOutGpsEvent, out deliveryInGpsEvent, out deliveryOutGpsEvent);

			SetSelectedTimes(pickupInGpsEvent, pickupOutGpsEvent, waitingPointInGpsEvent, waitingPointOutGpsEvent, deliveryInGpsEvent, deliveryOutGpsEvent);
		}

		void SetSelectedTimes(GPSEvent pickupInGpsEvent, GPSEvent pickupOutGpsEvent, GPSEvent waitingPointInGpsEvent, GPSEvent waitingPointOutGpsEvent, GPSEvent deliveryInGpsEvent, GPSEvent deliveryOutGpsEvent)
		{
			PickupSelectedIn = pickupInGpsEvent != null ? pickupInGpsEvent.EventTime : ZDateTime.Empty;
			PickupSelectedOut = pickupOutGpsEvent != null ? pickupOutGpsEvent.EventTime : ZDateTime.Empty;
			WaitingPointSelectedIn = waitingPointInGpsEvent != null ? waitingPointInGpsEvent.EventTime : ZDateTime.Empty;
			WaitingPointSelectedOut = waitingPointOutGpsEvent != null ? waitingPointOutGpsEvent.EventTime : ZDateTime.Empty;
			DeliverySelectedIn = deliveryInGpsEvent != null ? deliveryInGpsEvent.EventTime : ZDateTime.Empty;
			DeliverySelectedOut = deliveryOutGpsEvent != null ? deliveryOutGpsEvent.EventTime : ZDateTime.Empty;
		}

		public bool AllocateAndSetTimeAsSelected()
		{
			GPSEvent pickupInGpsEvent = null;
			GPSEvent pickupOutGpsEvent = null;
			GPSEvent waitingPointInGpsEvent = null;
			GPSEvent waitingPointOutGpsEvent = null;
			GPSEvent deliveryInGpsEvent = null;
			GPSEvent deliveryOutGpsEvent = null;

			var result = SelectedGPSEventsForLegSegments(out pickupInGpsEvent, out pickupOutGpsEvent, out waitingPointInGpsEvent, out waitingPointOutGpsEvent, out deliveryInGpsEvent, out deliveryOutGpsEvent);

			if (result)
			{
				if (pickupInGpsEvent != null)
				{
					AllocateGPSEventToLegSegment(pickupInGpsEvent, Leg.Pickup, InOut.In);
				}

				if (pickupOutGpsEvent != null)
				{
					AllocateGPSEventToLegSegment(pickupOutGpsEvent, Leg.Pickup, InOut.Out);
				}

				if (CartageLeg.HasWaitPoint)
				{
					if (waitingPointInGpsEvent != null)
					{
						AllocateGPSEventToLegSegment(waitingPointInGpsEvent, Leg.WaitPoint, InOut.In);
					}

					if (waitingPointOutGpsEvent != null)
					{
						AllocateGPSEventToLegSegment(waitingPointOutGpsEvent, Leg.WaitPoint, InOut.Out);
					}
				}
				if (deliveryInGpsEvent != null)
				{
					AllocateGPSEventToLegSegment(deliveryInGpsEvent, Leg.Delivery, InOut.In);
				}

				if (deliveryOutGpsEvent != null)
				{
					AllocateGPSEventToLegSegment(deliveryOutGpsEvent, Leg.Delivery, InOut.Out);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "5#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
		public bool SelectedGPSEventsForLegSegments(out GPSEvent pickupInGpsEvent,
													out GPSEvent pickupOutGpsEvent,
													out GPSEvent waitingPointInGpsEvent,
													out GPSEvent waitingPointOutGpsEvent,
													out GPSEvent deliveryInGpsEvent,
													out GPSEvent deliveryOutGpsEvent)
		{
			bool result = true;

			var selectedEvents = GPSEventAllocations.Cast<GPSEventAllocation>().Where(g => g.Selected).OrderBy(g => g.GPSEvent == null ? ZDateTime.Empty : g.GPSEvent.EventTime);

			result &= SelectedGPSEventForLegSegment(selectedEvents, Leg.Pickup, InOut.In, out pickupInGpsEvent);
			result &= SelectedGPSEventForLegSegment(selectedEvents, Leg.Pickup, InOut.Out, out pickupOutGpsEvent);

			if (CartageLeg.HasWaitPoint)
			{
				result &= SelectedGPSEventForLegSegment(selectedEvents, Leg.WaitPoint, InOut.In, out waitingPointInGpsEvent);
				result &= SelectedGPSEventForLegSegment(selectedEvents, Leg.WaitPoint, InOut.Out, out waitingPointOutGpsEvent);
			}
			else
			{
				waitingPointInGpsEvent = null;
				waitingPointOutGpsEvent = null;
			}

			result &= SelectedGPSEventForLegSegment(selectedEvents, Leg.Delivery, InOut.In, out deliveryInGpsEvent);
			result &= SelectedGPSEventForLegSegment(selectedEvents, Leg.Delivery, InOut.Out, out deliveryOutGpsEvent);

			return result;
		}

		void AllocateGPSEventToLegSegment(GPSEvent gpsEvent, LegSegment legSegment, InOut inOut)
		{
			if (inOut == InOut.In)
			{
				legSegment.TimeIn = gpsEvent.EventTime;
			}
			else
			{
				legSegment.TimeOut = gpsEvent.EventTime;
			}

			// Note: 
			// 1. We can override a existing segment allocation (so one event cannot have many segment)
			// 2. A same segment can have many events for history (so one segment can have many events even for the same direction (IN or OUT))
			var gpsClientActivity = Factory.Load<GPSSupporterActivity>(gpsEvent.EventPK);
			gpsClientActivity.EN_JU = legSegment.Leg.PK;
		}

		bool SelectedGPSEventForLegSegment(IOrderedEnumerable<GPSEventAllocation> selectedEvents, LegSegment legSegment, InOut inOut, out GPSEvent gpsEvent)
		{
			Argument.NotNull(selectedEvents, "selectedEvents", "selectedEvents should not be null.");

			var result = false;
			gpsEvent = null;

			var validGpsEventAllocations = selectedEvents.Where(g => IsValidAddressAndDirection(g, legSegment, inOut));

			var validGpsEventAllocationsCount = validGpsEventAllocations.Count();
			if (validGpsEventAllocationsCount == 1)
			{
				var gpsEventAllocation = validGpsEventAllocations.First();
				gpsEvent = gpsEventAllocation.GPSEvent;
				result = true;
			}
			else if (validGpsEventAllocationsCount == 0)
			{
				result = true;
			}

			return result;
		}
	}
}
