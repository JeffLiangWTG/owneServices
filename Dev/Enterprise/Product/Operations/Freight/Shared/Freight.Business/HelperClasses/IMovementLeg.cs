using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IMovementLeg
	{
		ZDateTime DepartureDate { get; }
		ZDateTime ArrivalDate { get; }
		ZString Load { get; }
		ZString Discharge { get; }
		ZString TransportMode { get; }
	}

	public static class MovementLegExtensions
	{
		public static bool IsPortToPort(this IMovementLeg leg)
		{
			return leg.HasBothPorts() && !leg.IsWithinPort();
		}

		public static bool IsWithinPort(this IMovementLeg leg)
		{
			return leg.HasBothPorts() && leg.Load == leg.Discharge;
		}

		public static bool HasBothPorts(this IMovementLeg leg)
		{
			if (leg == null)
			{
				return false;
			}

			return !leg.Load.IsEmpty && !leg.Discharge.IsEmpty;
		}

		public static bool HasAtLeastOnePort(this IMovementLeg leg)
		{
			if (leg == null)
			{
				return false;
			}

			return !leg.Load.IsEmpty || !leg.Discharge.IsEmpty;
		}

		public static ZString LoadForSorting(this IMovementLeg leg)
		{
			if (!leg.HasAtLeastOnePort())
			{
				return ZString.Empty;
			}

			return leg.Load.IsEmpty ? leg.Discharge : leg.Load;
		}

		public static ZString DischargeForSorting(this IMovementLeg leg)
		{
			if (!leg.HasAtLeastOnePort())
			{
				return ZString.Empty;
			}

			return leg.Discharge.IsEmpty ? leg.Load : leg.Discharge;
		}
	}
}
