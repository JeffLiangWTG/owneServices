
namespace Enterprise.Freight.Forwarding.Business
{
	public static class DeliveryDueDateCalculatorFactory
	{
		public static DeliveryDueDateCalculator GetDeliveryDueDateCalculatorByDeliveryMode(DeliveryDueDateCalculationContext context)
		{
			var isDTC = context.IsDTC;

			switch (context.HBLDeliveryMode)
			{
				case Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR:
					return isDTC ? new DoorToDTCDeliveryDueDateCalculator(context) : new DoorToDoorDeliveryDueDateCalculator(context);
				case Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS:
					return isDTC ? new DoorToDTCDeliveryDueDateCalculator(context) : new DoorToCFSDeliveryDueDateCalculator(context);
				case Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR:
					return isDTC ? new CFSToDTCDeliveryDueDateCalculator(context) : new CFSToDoorDeliveryDueDateCalculator(context);
				case Core.Constants.HBLDeliveryModes.Codes.CFS_CFS:
					return isDTC ? new CFSToDTCDeliveryDueDateCalculator(context) : new CFSToCFSDeliveryDueDateCalculator(context);
				case Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT:
					return new AirportToAirportDeliveryDueDateCalculator(context);
				case Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT:
					return new DoorToAirportDeliveryDueDateCalculator(context);
				case Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT:
					return new CFSToAirportDeliveryDueDateCalculator(context);
				case Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR:
					return new AirportToDoorDeliveryDueDateCalculator(context);
				case Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS:
					return new AirportToCFSDeliveryDueDateCalculator(context);
				default:
					return null;
			}
		}
	}
}
