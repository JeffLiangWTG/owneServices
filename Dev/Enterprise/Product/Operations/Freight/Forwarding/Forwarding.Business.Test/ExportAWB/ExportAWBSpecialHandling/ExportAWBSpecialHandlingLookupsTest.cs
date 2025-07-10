using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ExportAWBSpecialHandlingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNoExceptionIfOriginOrDestinationPortsAreNull()
		{
			AssertNoExceptionThrown(() => {
				var handling = Factory.New<ExportAWBSpecialHandling>();
				var lookup = new ExportAWBSpecialHandlingLookups(handling);
				AssertNotNull("SpecialHandlingCodeDescriptionList should not be null", lookup.SpecialHandlingCodeDescriptionList);
			});
		}

		public void TestSpecialHandlingCodeForAWBHeaderIsAddedToTheList()
		{
			var lookup = CreateConsolAndGetAWBSpecialHandlingLookup("100", "AUSYD", "CNSHA");
			var refHandling = CreateRefAirLineSpecialHandlingCode("100", "AUSYD", "CNSHA");

			Assert(lookup.SpecialHandlingCodeDescriptionList.ContainsCode("MMT"));
		}

		public void TestSpecialHandlingCodeForAWBHeaderChangesIfOriginDesPortIsChanged()
		{
			var fakePort = Factory.NewWithValidTestData<RefUNLOCO>();
			fakePort.Code = "FK1";
			fakePort.RL_IATA = "F1";

			var lookup = CreateConsolAndGetAWBSpecialHandlingLookup("100", "AUSYD", "CNSHA");
			var refHandling = CreateRefAirLineSpecialHandlingCode("100", "AUSYD", "CNSHA");

			Assert(lookup.SpecialHandlingCodeDescriptionList.ContainsCode("MMT"));

			var originCode = refHandling.RHC_OriginPortOrCountry;

			refHandling.RHC_OriginPortOrCountry = fakePort.Code;
			Assert(!lookup.SpecialHandlingCodeDescriptionList.ContainsCode("MMT"));

			refHandling.RHC_OriginPortOrCountry = originCode;
			Assert(lookup.SpecialHandlingCodeDescriptionList.ContainsCode("MMT"));

			refHandling.RHC_DestinationPortOrCountry = fakePort.Code;
			Assert(!lookup.SpecialHandlingCodeDescriptionList.ContainsCode("MMT"));
		}

		public void TestRemoveSCOFromAWBSpecialHandlingCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
			{
				var lookup = CreateConsolAndGetAWBSpecialHandlingLookup("100", "CHCHP", "CNSHA");
				Assert(!lookup.SpecialHandlingCodeDescriptionList.ContainsCode(AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly));
			}
		}

		#region Helper Methods

		RefAirlineSpecialHandlingCode CreateRefAirLineSpecialHandlingCode(ZString airlinePrefix, ZString origin, ZString destination)
		{
			var refAirline = Factory.NewWithValidTestData<RefAirline>();
			refAirline.RM_AirlinePrefix = airlinePrefix;
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = airlinePrefix;
			refAirline.RM_TwoCharacterCode = "FK";

			var specialHandlingCode = Factory.NewWithValidTestData<RefAirlineSpecialHandlingCode>();
			specialHandlingCode.RHC_Code = "MMT";
			specialHandlingCode.RHC_Description = "MMT Description";
			specialHandlingCode.RHC_OriginPortOrCountry = origin;
			specialHandlingCode.RHC_DestinationPortOrCountry = destination;
			specialHandlingCode.RHC_RM_Airline = refAirline.PK;

			return specialHandlingCode;
		}

		ExportAWBSpecialHandlingLookups CreateConsolAndGetAWBSpecialHandlingLookup(ZString airlinePrefix, ZString origin, ZString destination)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			var awbSpecialHandling = Factory.New<ExportAWBSpecialHandling>();
			awbSpecialHandling.EP_EH = consol.AWBHeader.PK;

			consol.MasterBillAirlinePrefix = airlinePrefix;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;

			var lookup = new ExportAWBSpecialHandlingLookups(awbSpecialHandling);
			return lookup;
		}

		#endregion
	}
}
