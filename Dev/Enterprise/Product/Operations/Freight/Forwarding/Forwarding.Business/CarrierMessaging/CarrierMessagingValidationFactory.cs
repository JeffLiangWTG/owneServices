using CargoWise.Common;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class CarrierMessagingValidationFactory
	{
		public static CarrierMessagingValidation GetValidation(ForwardingConsol consol)
		{
			Argument.NotNull(consol, "consol");

			if (consol.IsSuitableForForwardAirMessage())
			{
				return new ForwardAirCarrierMessagingValidation(consol);
			}

			return new NotImplementedCarrierMessagingValidation(consol);
		}
	}
}
