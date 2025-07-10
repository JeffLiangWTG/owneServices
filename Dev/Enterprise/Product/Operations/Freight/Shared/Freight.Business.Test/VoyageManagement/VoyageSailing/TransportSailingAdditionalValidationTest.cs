using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportSailingAdditionalValidationTest : BusinessObjectValidationTestCase
	{
		#region Depot Cut Off Date

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

			transport.Sailing.Validation.ValidateAll();
			AssertNoErrors(transport.Sailing.JX_DepotCutOffInfo);
			AssertNoErrors(transport.JW_DepotCutOffForBindingInfo);

			FreightConfigurationRegistry.Instance.ConsolCutOffDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), true);
			transport.Sailing.Validation.ValidateAll();
			AssertHasErrors(transport.Sailing.JX_DepotCutOffInfo);
			AssertHasErrors(transport.JW_DepotCutOffForBindingInfo);
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

			transport.Sailing.Validation.ValidateAll();
			AssertNoErrors(transport.Sailing.JX_DepotCutOffInfo);
			AssertNoErrors(transport.JW_DepotCutOffForBindingInfo);

			FreightConfigurationRegistry.Instance.ConsolCutOffDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), true);
			transport.Sailing.Validation.ValidateAll();
			AssertHasErrors(transport.Sailing.JX_DepotCutOffInfo);
			AssertHasErrors(transport.JW_DepotCutOffForBindingInfo);
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

			transport.Sailing.Validation.ValidateAll();
			AssertNoErrors(transport.Sailing.JX_DepotCutOffInfo);
			AssertNoErrors(transport.JW_DepotCutOffForBindingInfo);

			FreightConfigurationRegistry.Instance.ConsolCutOffDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), true);
			transport.Sailing.Validation.ValidateAll();
			AssertNoErrors(transport.Sailing.JX_DepotCutOffInfo);
			AssertNoErrors(transport.JW_DepotCutOffForBindingInfo);
		}

		#endregion
	}
}
