using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class RoutingRequestConsolGeneratorValidationForSailingsTest : BusinessObjectValidationTestCase
	{
		public void TestRoutingRequestConsolGeneratorValidation_AirlinePrefix_ForDifferentCarriers()
		{
			var message = "There are multiple airline schedules selected.";

			Generator.AirlinePrefix = "SQ";
			AssertNoWarning(generator.AirlinePrefixInfo, message);

			var sailings = new JobSailingCollection(Factory);
			sailings.Add(CreateSailing("SQ22"));
			sailings.Add(CreateSailing("CX33"));
			var selection2 = new MultiDaysSelection(sailings, Factory);
			var generator2 = new RoutingRequestConsolGenerator(selection2);
			generator2.AirlinePrefix = "CX";
			AssertHasWarning(generator2.AirlinePrefixInfo, message);
		}

		public void TestRoutingRequestConsolGeneratorValidation_AirlinePrefix()
		{
			var notFoundMessage = "The Airline Prefix does not match any airlines.";
			var notUniqueMessage = "The Airline Prefix is not unique, so airline details cannot be imported into the created consol(s).";

			var airlines = Factory.Load<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "ZZ"));
			foreach (var airline in airlines)
			{
				airline.Delete();
			}

			Generator.AirlinePrefix = "ZZ";
			AssertHasError(generator.AirlinePrefixInfo, notFoundMessage);

			var airline1 = Factory.NewWithValidTestData<RefAirline>();
			airline1.RM_TwoCharacterCode = "ZZ";
			var airline2 = Factory.NewWithValidTestData<RefAirline>();
			airline2.RM_TwoCharacterCode = "ZZ";

			Generator.AirlinePrefix = "ZZ";
			AssertHasError(generator.AirlinePrefixInfo, notUniqueMessage);
		}

		public void TestRoutingRequestConsolGeneratorValidation_ServiceLevel()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "SQ").PK;

			var serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "XXX";
			serviceLevel1.PL_CarrierServiceLevelDescription = "XXX Description";
			var serviceLevel2 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "EXP";
			serviceLevel2.PL_CarrierServiceLevelDescription = "EXP Description";

			Factory.Save();

			Generator.ServiceLevel = "YYY";
			AssertHasErrors(Generator.ServiceLevelInfo);

			Generator.ServiceLevel = "EXP";
			AssertNoErrors(Generator.ServiceLevelInfo);

			Generator.ServiceLevel = "XXX";
			AssertNoErrors(Generator.ServiceLevelInfo);

			Generator.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			AssertNoErrors(Generator.ServiceLevelInfo);
		}

		#region Implementation

		JobSailing CreateSailing(ZString voyageFlight)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = voyageFlight;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "HKHKG";
			destination.JB_E_ARV = ZDate.Today.AddDays(1);

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;

			return sailing;
		}

		RoutingRequestConsolGenerator Generator
		{
			get { return generator ?? (generator = new RoutingRequestConsolGenerator(MultiDaysSelectionForTest)); }
		}

		RoutingRequestConsolGenerator generator;

		MultiDaysSelection MultiDaysSelectionForTest
		{
			get
			{
				var sailings = new JobSailingCollection(Factory);
				sailings.Add(CreateSailing("SQ22"));
				sailings.Add(CreateSailing("SQ33"));
				return new MultiDaysSelection(sailings, Factory);
			}
		}

		#endregion
	}
}
