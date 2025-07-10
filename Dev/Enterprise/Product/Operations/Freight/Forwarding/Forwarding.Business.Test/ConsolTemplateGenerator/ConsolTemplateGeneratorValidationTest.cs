using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolTemplateGeneratorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestConsolTemplateGeneratorValidation_ServiceLevel()
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

			var generator = new ConsolTemplateGenerator(MultiDaysSelectionForTest);

			generator.ServiceLevel = "YYY";
			AssertHasErrors(generator.ServiceLevelInfo);

			generator.ServiceLevel = "EXP";
			AssertNoErrors(generator.ServiceLevelInfo);

			generator.ServiceLevel = "XXX";
			AssertNoErrors(generator.ServiceLevelInfo);

			generator.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			AssertNoErrors(generator.ServiceLevelInfo);
		}

		#region Implementation

		MultiDaysSelection MultiDaysSelectionForTest
		{
			get
			{
				var sailings = new JobSailingCollection(Factory);
				sailings.Add(CreateSailing());

				return new MultiDaysSelection(sailings, Factory);
			}
		}

		JobSailing CreateSailing()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "SQ22";

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

		#endregion
	}
}
