namespace Enterprise.Freight.Integration
{
	public interface IETAProvider
	{
		IDeliveryDueDateCalculationResult CalculateTimeOfArrivalToAirport();
	}

	public static class ETAProviderConstants
	{
		public static string SuccessETALastLeg
		{
			get => Res.GetString("fb526737-237a-4101-90db-455b19ca5029", "ETA of last transport leg has been selected as [Delivery Due Date].");
		}

		public static string ErrorNoShipment
		{
			get => Res.GetString("7ff45503-2586-4ecf-9d4a-c42157f772e7", "No shipment was supplied.");
		}

		public static string ErrorNoConsol
		{
			get => Res.GetString("b327cc9c-89ac-49ec-b54c-2c67e6b3af97", "Shipment has not been attached to a consolidation.");
		}

		public static string ErrorMultipleConsols
		{
			get => Res.GetString("56afa4c9-3927-447d-8415-274ca748abb5", "Shipment has been attached to more than one consolidation.");
		}

		public static string ErrorNotDirectAgent
		{
			get => Res.GetString("f63c1a67-3d88-408f-b553-ac1373cbe199", "Attached consolidation's type is not Direct.");
		}

		public static string ErrorETABlank
		{
			get => Res.GetString("a8838993-433e-432f-86ab-247b64f6eefc", "ETA of the last transport leg is blank.");
		}

		public static string ErrorNoMatchingTransportLeg
		{
			get => Res.GetString("2955f999-f9e5-49ac-b238-6c394057168a", "No matching transport leg found.");
		}
	}
}
