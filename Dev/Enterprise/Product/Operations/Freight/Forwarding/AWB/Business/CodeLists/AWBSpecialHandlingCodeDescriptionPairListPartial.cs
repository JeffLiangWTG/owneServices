namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public partial class AWBSpecialHandlingCodeDescriptionPairList
	{
		public static bool IsCargoSecurityStatusCode(string code)
		{
			return code == Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements
				|| code == Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft
				|| code == Codes.CargoSecureForPassengerAndAllCargoAircraft
				|| code == Codes.CargoSecureForAllCargoAircraftOnly;
		}
	}
}
