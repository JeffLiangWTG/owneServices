using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportVoyOriginAdditionalValidationTest : BusinessObjectValidationTestCase
	{
		#region Terminal Cut Off Date

		public void TestLCL_CutOffDateMandatory()
		{
			FreightConfigurationRegistry.Instance.ConsolCutOffDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), false);

			var consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_RL_NKDischargePort = "INBOM";

			var transport = consol.Transports[0];

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL 111";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "AIR";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "111S";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSMV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			transport.Sailing.Origin.Validation.ValidateAll();
			AssertNoErrors(transport.Sailing.Origin.JA_CutOffInfo);
			AssertNoErrors(transport.JW_TerminalCutOffForBindingInfo);

			FreightConfigurationRegistry.Instance.ConsolCutOffDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), true);
			transport.Sailing.Origin.Validation.ValidateAll();
			AssertNoErrors(transport.Sailing.Origin.JA_CutOffInfo);
			AssertNoErrors(transport.JW_TerminalCutOffForBindingInfo);
		}

		public void TestAIR_CutOffDateMandatory()
		{
			FreightConfigurationRegistry.Instance.ConsolCutOffDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), false);

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_RL_NKDischargePort = "INBOM";

			var transport = consol.Transports[0];

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL 111";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "AIR";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "111S";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSMV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			transport.Sailing.Origin.Validation.ValidateAll();
			AssertNoErrors(transport.Sailing.Origin.JA_CutOffInfo);
			AssertNoErrors(transport.JW_TerminalCutOffForBindingInfo);

			FreightConfigurationRegistry.Instance.ConsolCutOffDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), true);
			transport.Sailing.Origin.Validation.ValidateAll();
			AssertNoErrors(transport.Sailing.Origin.JA_CutOffInfo);
			AssertNoErrors(transport.JW_TerminalCutOffForBindingInfo);
		}

		public void TestFCL_CutOffDateMandatory()
		{
			FreightConfigurationRegistry.Instance.ConsolCutOffDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), false);

			var consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_RL_NKDischargePort = "INBOM";

			var transport = consol.Transports[0];

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL 111";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "AIR";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "111S";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSMV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			transport.Sailing.Origin.Validation.ValidateAll();
			AssertNoErrors(transport.Sailing.Origin.JA_CutOffInfo);
			AssertNoErrors(transport.JW_TerminalCutOffForBindingInfo);

			FreightConfigurationRegistry.Instance.ConsolCutOffDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), true);
			transport.Sailing.Origin.Validation.ValidateAll();
			AssertHasErrors(transport.Sailing.Origin.JA_CutOffInfo);
			AssertHasErrors(transport.JW_TerminalCutOffForBindingInfo);
		}

		#endregion

		#region TestCheckJA_RL_NKPortOfLoading

		public void TestCheckJA_RL_NKPortOfLoading()
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
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USLAX";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHA";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			var errorMessage = "A routing leg cannot load at the consol's final discharge.";

			sailing.Origin.Validation.ValidateAll();
			AssertHasErrorContaining(sailing.Origin.JA_RL_NKPortOfLoadingInfo, errorMessage);
			AssertHasErrorContaining(transport.JW_RL_NKLoadPortForBindingInfo, errorMessage);

			sailing.Origin.JA_RL_NKPortOfLoading = "AUSYD";
			sailing.Origin.Validation.ValidateAll();
			AssertNoErrorContaining(sailing.Origin.JA_RL_NKPortOfLoadingInfo, errorMessage);
			AssertNoErrorContaining(transport.JW_RL_NKLoadPortForBindingInfo, errorMessage);
		}

		#endregion

		#region TestCheckJA_E_DEP

		public void TestCheckJA_E_DEP()
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
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing.PK;

			var etd = ZDateTime.Now;
			transport1.JW_ETD = etd;
			transport1.JW_ETA = etd.AddDays(4);
			transport2.Sailing.Origin.JA_E_DEP = etd.AddDays(4).AddHours(-6);
			transport2.Sailing.Origin.JA_A_DEP = etd.AddDays(8);

			AssertNoErrors("ETD is less than a day before the previous transport's ETA", transport2.Sailing.Origin.JA_E_DEPInfo);

			transport2.Sailing.Origin.JA_E_DEP = etd.AddDays(2);
			AssertHasError("ETD is more than a day before the previous transport's ETA", transport2.Sailing.Origin.JA_E_DEPInfo, legOutOfOrderMessage);
		}

		#endregion

		#region TestCheckJA_A_DEP

		public void TestCheckJA_A_DEP()
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

			AssertATDOrderValidated(transport1, transport2);

			transport2.JW_LegOrder = 1;
			AssertATDOrderNotValidated(transport1, transport2);

			transport1.JW_LegOrder = 2;
			AssertATDOrderValidated(transport2, transport1);
		}

		void AssertATDOrderValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;
			transport1.Sailing.Destination.JB_A_ARV = ZDateTime.Empty;
			transport2.Sailing.Origin.JA_A_DEP = now;
			AssertNoErrors(transport2.Sailing.Origin.JA_A_DEPInfo);

			transport1.Sailing.Destination.JB_A_ARV = now.AddDays(1);
			transport2.Sailing.Origin.Validation.ValidateJA_A_DEP();
			AssertHasError(transport1.Sailing.Destination.JB_A_ARVInfo, legOutOfOrderMessage);
			AssertHasError(transport2.Sailing.Origin.JA_A_DEPInfo, legOutOfOrderMessage);

			transport1.Sailing.Destination.JB_A_ARV = now.AddDays(-1);
			transport2.Sailing.Origin.Validation.ValidateJA_A_DEP();
			AssertNoErrors(transport2.Sailing.Origin.JA_A_DEPInfo);
			AssertNoErrors(transport1.Sailing.Destination.JB_A_ARVInfo);

			transport1.Sailing.Destination.JB_A_ARV = ZDateTime.Empty;
			transport2.Sailing.Origin.JA_A_DEP = ZDateTime.Empty;
		}

		void AssertATDOrderNotValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;

			transport1.Sailing.Destination.JB_A_ARV = now.AddDays(1);
			transport2.Sailing.Origin.JA_A_DEP = now;
			AssertNoErrors(transport2.Sailing.Origin.JA_A_DEPInfo);

			transport1.Sailing.Destination.JB_A_ARV = ZDateTime.Empty;
			transport2.Sailing.Origin.JA_A_DEP = ZDateTime.Empty;

			transport2.Sailing.Destination.JB_A_ARV = now.AddDays(1);
			transport1.Sailing.Origin.JA_A_DEP = now;
			AssertNoErrors(transport1.Sailing.Origin.JA_A_DEPInfo);

			transport2.Sailing.Destination.JB_A_ARV = ZDateTime.Empty;
			transport1.Sailing.Origin.JA_A_DEP = ZDateTime.Empty;
		}

		#endregion

		#region TestValidationIsNotRunWhenTransportIsDeleted

		public void TestValidationIsNotRunWhenTransportIsDeleted()
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
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USLAX";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHA";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			sailing.Origin.Validation.ValidateAll();
			var validation = sailing.Origin.AdditionalValidation;

			transport.Delete();
			AssertNoExceptionThrown(validation.ValidateAll);
		}

		#endregion

		const string legOutOfOrderMessage = "The date order does not reflect the leg order.";
	}
}
