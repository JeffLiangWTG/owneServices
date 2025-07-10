using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class NonSecurityJobConsolAWBSpecialHandlingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSpecialHandlingCodeDescriptionList()
		{
			var handling = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			var allSpecialHandlingCodes = new AWBSpecialHandlingCodeDescriptionPairList();
			allSpecialHandlingCodes.SortByDescription();
			var expectedCodes = allSpecialHandlingCodes.GetAllCodes().Where(x => !AWBSpecialHandlingCodeDescriptionPairList.IsCargoSecurityStatusCode(x));

			AssertContainsExactElementsInExactOrder(expectedCodes, handling.Lookups.SpecialHandlingCodeDescriptionList.GetAllCodes());
		}

		public void TestNoExceptionIfOriginOrDestinationPortsAreNull()
		{
			AssertNoExceptionThrown(() => {
				var handling = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
				var allSpecialHandlingCodes = new AWBSpecialHandlingCodeDescriptionPairList();
				var expectedCodes = allSpecialHandlingCodes.GetAllCodes().Where(x => !AWBSpecialHandlingCodeDescriptionPairList.IsCargoSecurityStatusCode(x));
				var consol = Factory.New<ForwardingConsol>();
				handling.JKH_JK_Consol = consol.PK;

				AssertContainsExactElementsInExactOrder(expectedCodes, handling.Lookups.SpecialHandlingCodeDescriptionList.GetAllCodes());
			});
		}

		public void TestIfAirlineSpecialHandlingIsInsertedToList()
		{
			var refAirline = Factory.New<RefAirline>();
			refAirline.RM_AirlinePrefix = "Emk";
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "Emk";

			var originPort = Factory.New<RefUNLOCO>();
			originPort.Code = "port1";
			var destinationPort = Factory.New<RefUNLOCO>();
			destinationPort.Code = "port2";

			var consol = Factory.New<ForwardingConsol>();     
			consol.JK_TransportMode = "Air";
			consol.JK_RL_NKLoadPort = originPort.Code;
			consol.JK_RL_NKDischargePort = destinationPort.Code;
			consol.MasterBillAirlinePrefix = refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode;

			var refHandling = Factory.New<RefAirlineSpecialHandlingCode>();
			refHandling.RHC_Code = "MMT";
			refHandling.RHC_Description = "MMT Description";
			refHandling.RHC_OriginPortOrCountry = originPort.Code;
			refHandling.RHC_DestinationPortOrCountry = destinationPort.Code;
			refHandling.RHC_RM_Airline = refAirline.PK;

			var consoleSpecialHandling = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			consoleSpecialHandling.JKH_JK_Consol = consol.PK;

			var specialHandlingCodes = consoleSpecialHandling.Lookups.SpecialHandlingCodeDescriptionList;

			Assert(specialHandlingCodes.ContainsCode("MMT"));
		}

		public void TestSpecialHandlingCode_DoesNotContainAirlineSpecificCode_WhenOriginAndDestinationIsNotMatched()
		{
			var refAirline = Factory.New<RefAirline>();
			refAirline.RM_AirlinePrefix = "Emk";
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "Emk";

			var fakePort = Factory.New<RefUNLOCO>();
			fakePort.Code = "fake";

			var originPort = Factory.New<RefUNLOCO>();
			originPort.Code = "port1";
			var destinationPort = Factory.New<RefUNLOCO>();
			destinationPort.Code = "port2";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "Air";
			consol.JK_RL_NKLoadPort = originPort.Code;
			consol.JK_RL_NKDischargePort = destinationPort.Code;
			consol.MasterBillAirlinePrefix = refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode;

			var refHandling = Factory.New<RefAirlineSpecialHandlingCode>();
			refHandling.RHC_Code = "MMT";
			refHandling.RHC_Description = "MMT Description";
			refHandling.RHC_OriginPortOrCountry = originPort.Code;
			refHandling.RHC_DestinationPortOrCountry = destinationPort.Code;
			refHandling.RHC_RM_Airline = refAirline.PK;

			var consoleSpecialHandling = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			consoleSpecialHandling.JKH_JK_Consol = consol.PK;

			var specialHandlingCodes = consoleSpecialHandling.Lookups.SpecialHandlingCodeDescriptionList;

			Assert(specialHandlingCodes.ContainsCode("MMT"));

			consol.JK_RL_NKLoadPort = fakePort.Code;
			specialHandlingCodes = consoleSpecialHandling.Lookups.SpecialHandlingCodeDescriptionList;
			Assert(!specialHandlingCodes.ContainsCode("MMT"));

			consol.JK_RL_NKLoadPort = originPort.Code;
			specialHandlingCodes = consoleSpecialHandling.Lookups.SpecialHandlingCodeDescriptionList;
			Assert(specialHandlingCodes.ContainsCode("MMT"));

			consol.JK_RL_NKDischargePort = fakePort.Code;
			specialHandlingCodes = consoleSpecialHandling.Lookups.SpecialHandlingCodeDescriptionList;
			Assert(!specialHandlingCodes.ContainsCode("MMT"));
		}

		public void TestSpecialHandlingCode_ContainsAirlineSpecificCode_WhenOriginOrDestinationIsEmptyInAirlineSpecificCode()
		{
			var refAirline = Factory.New<RefAirline>();
			refAirline.RM_AirlinePrefix = "Emk";
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "Emk";

			var fakePort = Factory.New<RefUNLOCO>();
			fakePort.Code = "fake";

			var originPort = Factory.New<RefUNLOCO>();
			originPort.Code = "port1";
			var destinationPort = Factory.New<RefUNLOCO>();
			destinationPort.Code = "port2";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "Air";
			consol.JK_RL_NKLoadPort = originPort.Code;
			consol.JK_RL_NKDischargePort = destinationPort.Code;
			consol.MasterBillAirlinePrefix = refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode;

			var refHandling = Factory.New<RefAirlineSpecialHandlingCode>();
			refHandling.RHC_Code = "MMT";
			refHandling.RHC_Description = "MMT Description";
			refHandling.RHC_OriginPortOrCountry = ZString.Empty;
			refHandling.RHC_DestinationPortOrCountry = destinationPort.Code;
			refHandling.RHC_RM_Airline = refAirline.PK;

			var consoleSpecialHandling = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			consoleSpecialHandling.JKH_JK_Consol = consol.PK;

			var specialHandlingCodes = consoleSpecialHandling.Lookups.SpecialHandlingCodeDescriptionList;

			Assert(specialHandlingCodes.ContainsCode("MMT"));

			refHandling.RHC_OriginPortOrCountry = originPort.Code;
			refHandling.RHC_DestinationPortOrCountry = ZString.Empty;

			specialHandlingCodes = consoleSpecialHandling.Lookups.SpecialHandlingCodeDescriptionList;

			Assert(specialHandlingCodes.ContainsCode("MMT"));
		}

		public void TestSpecialHandlingCode_ContainsAirlineSpecificCode_WhenCountryIsMatched()
		{
			var refAirline = Factory.New<RefAirline>();
			refAirline.RM_AirlinePrefix = "Emk";
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "Emk";

			var country = Factory.New<RefCountry>();
			country.Code = "AU";

			var originPort = Factory.New<RefUNLOCO>();
			originPort.Code = "port1";
			var destinationPort = Factory.New<RefUNLOCO>();
			destinationPort.Code = "port2";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "Air";
			consol.JK_RL_NKLoadPort = originPort.Code;
			consol.JK_RL_NKDischargePort = destinationPort.Code;
			consol.MasterBillAirlinePrefix = refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode;

			var refHandling = Factory.New<RefAirlineSpecialHandlingCode>();
			refHandling.RHC_Code = "MMT";
			refHandling.RHC_Description = "MMT Description";
			refHandling.RHC_OriginPortOrCountry = country.Code;
			refHandling.RHC_DestinationPortOrCountry = destinationPort.Code;
			refHandling.RHC_RM_Airline = refAirline.PK;

			var consoleSpecialHandling = Factory.New<NonSecurityJobConsolAWBSpecialHandling>();
			consoleSpecialHandling.JKH_JK_Consol = consol.PK;

			var specialHandlingCodes = consoleSpecialHandling.Lookups.SpecialHandlingCodeDescriptionList;

			Assert(!specialHandlingCodes.ContainsCode("MMT"));
		}
	}
}
