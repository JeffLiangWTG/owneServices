using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportVoyDestinationAdditionalValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckJB_RL_NKPortOfDischarge

		public void TestCheckJB_RL_NKPortOfDischarge()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			var transport = consol.Transports[0];

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL 111";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "111S";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSMV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			var errorMessage = "A routing leg cannot discharge at the consol's first load.";

			sailing.Destination.Validation.ValidateAll();
			AssertHasErrorContaining(sailing.Destination.JB_RL_NKPortOfDischargeInfo, errorMessage);
			AssertHasErrorContaining(transport.JW_RL_NKDiscPortForBindingInfo, errorMessage);

			sailing.Destination.JB_RL_NKPortOfDischarge = "USLAX";
			sailing.Destination.Validation.ValidateAll();
			AssertNoErrorContaining(sailing.Destination.JB_RL_NKPortOfDischargeInfo, errorMessage);
			AssertNoErrorContaining(transport.JW_RL_NKDiscPortForBindingInfo, errorMessage);
		}

		#endregion

		#region TestCheckJB_E_ARV

		public void TestCheckJB_E_ARV()
		{
			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_LegOrder = 1;
			transport2.JW_LegOrder = 2;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL 111";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "111S";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USLAX";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHA";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing.PK;

			transport2.JW_ETD = ZDateTime.Now.AddDays(10).AddHours(-6);
			transport2.JW_ETA = ZDateTime.Now.AddDays(20);
			transport1.Sailing.Destination.JB_E_ARV = ZDateTime.Now;
			transport1.Sailing.Destination.JB_A_ARV = ZDateTime.Now.AddDays(10);
			AssertNoErrors("ETA is less than a day after the next transport's ETD", transport1.Sailing.Destination.JB_E_ARVInfo);

			transport1.Sailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(12);
			AssertHasError("ETA is more than a day after the next transport's ETD", transport1.Sailing.Destination.JB_E_ARVInfo, legOutOfOrderMessage);
		}

		#endregion

		#region TestCheckJB_A_ARV

		public void TestCheckJB_A_ARV()
		{
			var consol = Factory.New<CommonConsol>();

			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL 111";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "111S";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";

			voyage.GenerateSailings();
			var sailing1 = voyage.Sailings[0];
			var sailing2 = voyage.Sailings[1];

			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1.PK;
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;
			transport1.JW_LegOrder = 1;
			transport2.JW_LegOrder = 2;

			AssertATAOrderValidated(transport1, transport2);

			transport2.JW_LegOrder = 1;
			AssertATAOrderNotValidated(transport1, transport2);

			transport1.JW_LegOrder = 2;
			AssertATAOrderValidated(transport2, transport1);
		}

		void AssertATAOrderValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;

			transport2.Sailing.Origin.JA_A_DEP = ZDateTime.Empty;
			transport1.Sailing.Destination.JB_A_ARV = now;
			AssertNoErrors(transport1.Sailing.Destination.JB_A_ARVInfo);

			transport2.Sailing.Origin.JA_A_DEP = now.AddDays(-1);
			transport1.Sailing.Destination.Validation.ValidateJB_A_ARV();
			AssertHasError(transport1.Sailing.Destination.JB_A_ARVInfo, legOutOfOrderMessage);

			transport2.Sailing.Origin.JA_A_DEP = now.AddDays(1);
			transport1.Sailing.Destination.Validation.ValidateJB_A_ARV();
			AssertNoErrors(transport1.Sailing.Destination.JB_A_ARVInfo);

			transport1.Sailing.Destination.JB_A_ARV = ZDateTime.Empty;
			transport2.Sailing.Origin.JA_A_DEP = ZDateTime.Empty;
		}

		void AssertATAOrderNotValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;

			transport1.Sailing.Destination.JB_A_ARV = now.AddDays(1);
			transport2.Sailing.Origin.JA_A_DEP = now;
			AssertNoErrors(transport2.JW_ATDInfo);

			transport1.Sailing.Destination.JB_A_ARV = ZDateTime.Empty;
			transport2.Sailing.Origin.JA_A_DEP = ZDateTime.Empty;

			transport2.Sailing.Destination.JB_A_ARV = now.AddDays(1);
			transport1.Sailing.Origin.JA_A_DEP = now;
			AssertNoErrors(transport1.JW_ATDInfo);

			transport2.Sailing.Destination.JB_A_ARV = ZDateTime.Empty;
			transport1.Sailing.Origin.JA_A_DEP = ZDateTime.Empty;
		}

		#endregion

		const string legOutOfOrderMessage = "The date order does not reflect the leg order.";
	}
}
