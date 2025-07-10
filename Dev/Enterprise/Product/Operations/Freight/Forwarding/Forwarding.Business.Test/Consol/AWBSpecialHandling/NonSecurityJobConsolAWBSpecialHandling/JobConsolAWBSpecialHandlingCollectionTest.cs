using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.AWB.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(NonSecurityJobConsolAWBSpecialHandlingCollection))]
	sealed class JobConsolAWBSpecialHandlingCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMaxCountValidation()
		{
			var collection = (ISupportMaxCountValidation)GetCollectionToTest();
			AssertEquals("Max count should be 9", 9, collection.MaxCountValidator.MaxCount);
		}

		public void TestAdditionalFilter()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var specialHandling1 = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			specialHandling1.JKH_JK_Consol = consol.PK;
			specialHandling1.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements;

			var specialHandling2 = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			specialHandling2.JKH_JK_Consol = consol.PK;
			specialHandling2.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;

			var specialHandling3 = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			specialHandling3.JKH_JK_Consol = consol.PK;
			specialHandling3.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;

			var specialHandling4 = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			specialHandling4.JKH_JK_Consol = consol.PK;
			specialHandling4.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

			var specialHandling5 = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			specialHandling5.JKH_JK_Consol = consol.PK;
			specialHandling5.JKH_Code = string.Empty;

			var specialHandling6 = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			specialHandling6.JKH_JK_Consol = consol.PK;
			specialHandling6.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround;

			var specialHandling7 = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			specialHandling7.JKH_JK_Consol = consol.PK;
			specialHandling7.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CarbonDioxideSolidDryIce;

			var collection = new NonSecurityJobConsolAWBSpecialHandlingCollection(consol);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { specialHandling6, specialHandling7 }, collection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NonSecurityJobConsolAWBSpecialHandlingCollection(Factory.New<ForwardingConsol>());
		}
	}
}
