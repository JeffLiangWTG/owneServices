using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class JobConsolAWBSpecialHandlingTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var specialHandlingItem = consol.AWBSpecialHandlingItems.AddNew();
			var decider = new JobConsolAWBSpecialHandlingTypeDecider();

			AssertEquals(typeof(SecurityJobConsolAWBSpecialHandling), decider.GetTypeForLoad(((INeedRow)specialHandlingItem).Row, Factory));

			specialHandlingItem.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements;
			AssertEquals(typeof(SecurityJobConsolAWBSpecialHandling), decider.GetTypeForLoad(((INeedRow)specialHandlingItem).Row, Factory));

			specialHandlingItem.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			AssertEquals(typeof(SecurityJobConsolAWBSpecialHandling), decider.GetTypeForLoad(((INeedRow)specialHandlingItem).Row, Factory));

			specialHandlingItem.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertEquals(typeof(SecurityJobConsolAWBSpecialHandling), decider.GetTypeForLoad(((INeedRow)specialHandlingItem).Row, Factory));

			specialHandlingItem.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			AssertEquals(typeof(SecurityJobConsolAWBSpecialHandling), decider.GetTypeForLoad(((INeedRow)specialHandlingItem).Row, Factory));

			specialHandlingItem.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround;
			AssertEquals(typeof(NonSecurityJobConsolAWBSpecialHandling), decider.GetTypeForLoad(((INeedRow)specialHandlingItem).Row, Factory));
		}
	}
}
