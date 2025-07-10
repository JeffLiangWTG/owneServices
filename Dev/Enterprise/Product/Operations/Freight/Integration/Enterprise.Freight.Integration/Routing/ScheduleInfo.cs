using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public struct ScheduleInfo
	{
		public ScheduleInfo(ZString carrier, int flightNumber, ZString origin, ZDate departureDate, ZString destination, ZDate arrivalDate, ZString aircraftType = default)
		{
			Carrier = carrier;
			FlightNumber = flightNumber;
			Origin = origin;
			Destination = destination;
			DepartureDate = departureDate;
			ArrivalDate = arrivalDate;
			AircraftType = aircraftType;
		}

		public ZString Carrier { get; }
		public int FlightNumber { get; }

		public ZString Origin { get; }
		public ZString Destination { get; }

		public ZDate DepartureDate { get; }
		public ZDate ArrivalDate { get; }

		public ZString AircraftType { get; }

		public static ScheduleInfo Empty => new ScheduleInfo(ZString.Empty, default, ZString.Empty, ZDate.Empty, ZString.Empty, ZDate.Empty);

		public bool IsEmpty
		{
			get
			{
				return Carrier.IsEmpty
					&& ArrivalDate.IsEmpty
					&& DepartureDate.IsEmpty
					&& Origin.IsEmpty
					&& Destination.IsEmpty
					&& AircraftType.IsEmpty
					&& FlightNumber == default;
			}
		}

		#region Overrides

		public override bool Equals(object obj)
		{
			if (obj is ScheduleInfo info)
			{
				return info == this;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return Origin.GetHashCode()
				^ Destination.GetHashCode()
				^ Carrier.GetHashCode()
				^ DepartureDate.GetHashCode()
				^ ArrivalDate.GetHashCode()
				^ FlightNumber.GetHashCode()
				^ AircraftType.GetHashCode();
		}

		public static bool operator ==(ScheduleInfo left, ScheduleInfo right)
		{
			return left.Origin == right.Origin
				&& left.Destination == right.Destination
				&& left.Carrier == right.Carrier
				&& left.DepartureDate == right.DepartureDate
				&& left.ArrivalDate == right.ArrivalDate
				&& left.FlightNumber == right.FlightNumber
				&& left.AircraftType == right.AircraftType;
		}

		public static bool operator !=(ScheduleInfo left, ScheduleInfo right)
		{
			return !(left == right);
		}

		#endregion
	}
}
