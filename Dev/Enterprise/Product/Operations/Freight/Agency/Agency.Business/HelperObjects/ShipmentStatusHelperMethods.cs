using System;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Agency.Business
{
	public static class ShipmentStatusHelperMethods
	{
		public static bool CountsTowardsAllocations(string status)
		{
			return (GetStatusFlags(status) & StatusFlags.CountsTowardsAllocations) != 0;
		}

		public static bool IsBookingStage(string status)
		{
			return (GetStatusFlags(status) & StatusFlags.IsBookingStage) != 0;
		}

		public static bool IsBillOfLadingStage(string status)
		{
			return (GetStatusFlags(status) & StatusFlags.IsBillOfLadingStage) != 0;
		}

		public static string[] GetBookingStageStatus()
		{
			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
			{
				return new string[]
				{
					ShipmentStatusList.Codes.ElectronicBooking,
					ShipmentStatusList.Codes.EBookingCancellationRequest,
					ShipmentStatusList.Codes.Booked,
					ShipmentStatusList.Codes.BookingCancelled,
					ShipmentStatusList.Codes.BookingRejected,
					ShipmentStatusList.Codes.WebBooking,
					ShipmentStatusList.Codes.WaitListed
				};
			}
			else
			{
				return new string[]
				{
					ShipmentStatusList.Codes.ElectronicBooking,
					ShipmentStatusList.Codes.Booked,
					ShipmentStatusList.Codes.WebBooking,
					ShipmentStatusList.Codes.WaitListed
				};
			}
		}

		public static string[] GetBillOfLadingStageStatus()
		{
			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
			{
				return new string[]
				{
					ShipmentStatusList.Codes.ElectronicShippingInstruction,
					ShipmentStatusList.Codes.Confirmed,
					ShipmentStatusList.Codes.SIRejected,
					ShipmentStatusList.Codes.WebFwdInstruction
				};
			}
			else
			{
				return new string[]
				{
					ShipmentStatusList.Codes.ElectronicShippingInstruction,
					ShipmentStatusList.Codes.Confirmed,
					ShipmentStatusList.Codes.WebFwdInstruction
				};
			}
		}

		[Flags]
		enum StatusFlags
		{
			None = 0x00,
			CountsTowardsAllocations = 0x01,
			IsBookingStage = 0x02,
			IsBillOfLadingStage = 0x04,
		}

		static StatusFlags GetStatusFlags(string status)
		{
			switch (status)
			{
				case ShipmentStatusList.Codes.Confirmed:
				case ShipmentStatusList.Codes.WebFwdInstruction:
				case ShipmentStatusList.Codes.ElectronicShippingInstruction:
				case ShipmentStatusList.Codes.SIRejected:
					return StatusFlags.CountsTowardsAllocations | StatusFlags.IsBillOfLadingStage;

				case ShipmentStatusList.Codes.Booked:
					return StatusFlags.CountsTowardsAllocations | StatusFlags.IsBookingStage;

				case ShipmentStatusList.Codes.WebBooking:
				case ShipmentStatusList.Codes.WaitListed:
				case ShipmentStatusList.Codes.ElectronicBooking:
				case ShipmentStatusList.Codes.EBookingCancellationRequest:
				case ShipmentStatusList.Codes.BookingCancelled:
				case ShipmentStatusList.Codes.BookingRejected:
					return StatusFlags.IsBookingStage;

				default:
					return StatusFlags.None;
			}
		}
	}
}
