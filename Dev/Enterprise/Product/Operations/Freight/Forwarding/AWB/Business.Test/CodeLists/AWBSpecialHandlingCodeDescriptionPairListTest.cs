using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class AWBSpecialHandlingCodeDescriptionPairListTest : TestCaseWithFactory
	{
		public void TestIsCargoSecurityStatusCode()
		{
			var list = new AWBSpecialHandlingCodeDescriptionPairList();
			var securityStatusCodes = new[]
			{
				AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly
			};

			foreach (var code in list.GetAllCodes())
			{
				AssertEquals(securityStatusCodes.Any(x => x == code), AWBSpecialHandlingCodeDescriptionPairList.IsCargoSecurityStatusCode(code));
			}

			Assert(!AWBSpecialHandlingCodeDescriptionPairList.IsCargoSecurityStatusCode(string.Empty));
		}
	}
}
