using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Freight.LocalCartage.Business.GPS
{
	internal class Leg
	{
		public Leg(CommonCartageLeg leg)
		{
			Pickup = new LegSegment(leg, LegSegmentType.PickUp);
			WaitPoint = new LegSegment(leg, LegSegmentType.WaitPoint);
			Delivery = new LegSegment(leg, LegSegmentType.Delivery);
			Sequence = leg.JU_RunSheetSequence;
		}

		public readonly ZInt Sequence;
		public readonly LegSegment Pickup;
		public readonly LegSegment WaitPoint;
		public readonly LegSegment Delivery;

		public ZBool IsComplete
		{
			get { return !OrderedGeofenceAddresses().Any(a => !a.HasTimeInAndOut); }
		}

		public IEnumerable<LegSegment> OrderedGeofenceAddresses()
		{
			if (Pickup.HasValidAddress)
			{
				yield return Pickup;
			}

			if (WaitPoint.HasValidAddress)
			{
				yield return WaitPoint;
			}

			if (Delivery.HasValidAddress)
			{
				yield return Delivery;
			}
		}
	}
}
