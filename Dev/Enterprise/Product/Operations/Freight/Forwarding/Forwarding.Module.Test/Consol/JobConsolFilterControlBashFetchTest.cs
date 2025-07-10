using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Module.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using IFilterControl = Enterprise.Integration.ZArchitecture.IFilterControl;
using IGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class JobConsolFilterControlBashFetchTest : FilterControlBashFetchHintTest<ForwardingModuleConsol>
	{
		#region BashFetchTest

		public void TestBashFetchForView_JK_Calc_AirBookingStatus()
		{
			// JobConsolTransport: 1

			BashFetchForView("JK_Calc_AirBookingStatus", 1);
		}

		public void TestBashFetchForView_JK_SystemCreateUser()
		{
			BashFetchForView("JK_SystemCreateUser", 0);
		}

		public void TestBashFetchForView_JK_SystemCreateBranch()
		{
			BashFetchForView("JK_SystemCreateBranch", 0);
		}

		public void TestBashFetchForView_JK_SystemCreateDepartment()
		{
			BashFetchForView("JK_SystemCreateDepartment", 0);
		}

		public void TestBashFetchForView_JK_SystemCreateTimeUtc()
		{
			BashFetchForView("JK_SystemCreateTimeUtc", 0);
		}

		public void TestBashFetchForView_JK_SystemLastEditUser()
		{
			BashFetchForView("JK_SystemLastEditUser", 0);
		}

		public void TestBashFetchForView_JK_SystemLastEditTimeUtc()
		{
			BashFetchForView("JK_SystemLastEditTimeUtc", 0);
		}

		public void TestBashFetchForView_JK_UniqueConsignRef()
		{
			BashFetchForView("JK_UniqueConsignRef", 0);
		}

		public void TestBashFetchForView_JK_AgentType()
		{
			BashFetchForView("JK_AgentType", 0);
		}

		public void TestBashFetchForView_JK_TransportMode()
		{
			BashFetchForView("JK_TransportMode", 0);
		}

		public void TestBashFetchForView_JK_ConsolMode()
		{
			BashFetchForView("JK_ConsolMode", 0);
		}

		public void TestBashFetchForView_JK_Phase()
		{
			BashFetchForView("JK_Phase", 0);
		}

		public void TestBashFetchForView_JK_IsNeutralMaster()
		{
			BashFetchForView("JK_IsNeutralMaster", 0);
		}

		public void TestBashFetchForView_JK_MasterBillNum()
		{
			BashFetchForView("JK_MasterBillNum", 0);
		}

		public void TestBashFetchForView_JK_RL_NKLoadPort()
		{
			BashFetchForView("JK_RL_NKLoadPort", 0);
		}

		public void TestBashFetchForView_JK_RL_NKDischargePort()
		{
			BashFetchForView("JK_RL_NKDischargePort", 0);
		}

		public void TestBashFetchForView_JK_SendingForwarderHandlingType()
		{
			BashFetchForView("JK_SendingForwarderHandlingType", 0);
		}

		public void TestBashFetchForView_JK_ReceivingForwarderHandlingType()
		{
			BashFetchForView("JK_ReceivingForwarderHandlingType", 0);
		}

		public void TestBashFetchForView_JK_RequiredTemperatureMinimum()
		{
			BashFetchForView("JK_RequiredTemperatureMinimum", 0);
		}

		public void TestBashFetchForView_JK_RequiredTemperatureMaximum()
		{
			BashFetchForView("JK_RequiredTemperatureMaximum", 0);
		}

		public void TestBashFetchForView_JK_RequiredTemperatureUnit()
		{
			BashFetchForView("JK_RequiredTemperatureUnit", 0);
		}

		public void TestBashFetchForView_JK_CarrierContractNumber()
		{
			BashFetchForView("JK_CarrierContractNumber", 0);
		}

		public void TestBashFetchForView_JK_JX_JA_RL_NKPortOfLoading()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_JX_JA_RL_NKPortOfLoading", 9);
		}

		public void TestBashFetchForView_JK_JX_JB_RL_NKPortOfDischarge()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_JX_JB_RL_NKPortOfDischarge", 9);
		}

		public void TestBashFetchForView_JK_Calc_SendingAgentCode()
		{
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("JK_Calc_SendingAgentCode", 2);
		}

		public void TestBashFetchForView_JK_Calc_ReceivingAgentCode()
		{
			BashFetchForView("JK_Calc_ReceivingAgentCode", 0);
		}

		public void TestBashFetchForView_JK_ScreeningStatus()
		{
			BashFetchForView("JK_ScreeningStatus", 0);
		}

		public void TestBashFetchForView_JK_JX_JA_E_DEP()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_JX_JA_E_DEP", 9);
		}

		public void TestBashFetchForView_JK_JX_JB_E_ARV()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_JX_JB_E_ARV", 9);
		}

		public void TestBashFetchForView_JK_JX_JA_A_DEP()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_JX_JA_A_DEP", 9);
		}

		public void TestBashFetchForView_JK_JX_JB_A_ARV()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_JX_JB_A_ARV", 9);
		}

		public void TestBashFetchForView_JK_JX_JV_NKVessel()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_JX_JV_NKVessel", 9);
		}

		public void TestBashFetchForView_JK_JX_JV_VoyageFlight()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_JX_JV_VoyageFlight", 9);
		}

		public void TestBashFetchForView_JK_JX_JV_AircraftType()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_JX_JV_AircraftType", 9);
		}

		public void TestBashFetchForView_JK_PrepaidCollect()
		{
			BashFetchForView("JK_PrepaidCollect", 0);
		}

		public void TestBashFetchForView_ShippingLinePK()
		{
			BashFetchForView("ShippingLinePK", 0);
		}

		public void TestBashFetchForView_JK_AWBServiceLevel()
		{
			BashFetchForView("JK_AWBServiceLevel", 0);
		}

		public void TestBashFetchForView_JK_BookingReference()
		{
			BashFetchForView("JK_BookingReference", 0);
		}

		public void TestBashFetchForView_JK_AgentsReference()
		{
			BashFetchForView("JK_AgentsReference", 0);
		}

		public void TestBashFetchForView_CreditorPK()
		{
			BashFetchForView("CreditorPK", 0);
		}

		public void TestBashFetchForView_JK_ConsolChargeable()
		{
			// JobConShipLink: 13
			// JobContainerPackPivot: 13
			// JobPackLines: 13
			// JobShipment: 12
			// JobContainer: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_ConsolChargeable", 54);
		}

		public void TestBashFetchForView_JK_ConsolChargeableUnit()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_ConsolChargeableUnit", 13);
		}

		public void TestBashFetchForView_JK_ConsolChargeableRate()
		{
			BashFetchForView("JK_ConsolChargeableRate", 0);
		}

		public void TestBashFetchForView_JK_Calc_TEUCount()
		{
			// JobContainer: 12
			// RefContainer: 1

			BashFetchForView("JK_Calc_TEUCount", 13);
		}

		public void TestBashFetchForView_JK_Calc_ContainerCount()
		{
			// JobContainer: 1

			BashFetchForView("JK_Calc_ContainerCount", 1);
		}

		public void TestBashFetchForView_JK_Calc_20GPCount()
		{
			// JobContainer: 12
			// RefContainer: 1

			BashFetchForView("JK_Calc_20GPCount", 13);
		}

		public void TestBashFetchForView_JK_Calc_20RECount()
		{
			// JobContainer: 12
			// RefContainer: 1

			BashFetchForView("JK_Calc_20RECount", 13);
		}

		public void TestBashFetchForView_JK_Calc_40GPCount()
		{
			// JobContainer: 12
			// RefContainer: 1

			BashFetchForView("JK_Calc_40GPCount", 13);
		}

		public void TestBashFetchForView_JK_Calc_40RECount()
		{
			// JobContainer: 12
			// RefContainer: 1

			BashFetchForView("JK_Calc_40RECount", 13);
		}

		public void TestBashFetchForView_JK_Calc_OtherContainerCount()
		{
			// JobContainer: 12
			// RefContainer: 1

			BashFetchForView("JK_Calc_OtherContainerCount", 13);
		}

		public void TestBashFetchForView_JK_TotalShipmentWeight()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_TotalShipmentWeight", 13);
		}

		public void TestBashFetchForView_JK_TotalShipmentWeightUnit()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_TotalShipmentWeightUnit", 13);
		}

		public void TestBashFetchForView_JK_TotalShipmentVolume()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_TotalShipmentVolume", 13);
		}

		public void TestBashFetchForView_JK_TotalShipmentVolumeUnit()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_TotalShipmentVolumeUnit", 13);
		}

		public void TestBashFetchForView_JK_CorrectedConsolWeight()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_CorrectedConsolWeight", 13);
		}

		public void TestBashFetchForView_JK_CorrectedConsolWeightUnit()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_CorrectedConsolWeightUnit", 13);
		}

		public void TestBashFetchForView_JK_CorrectedConsolVolume()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_CorrectedConsolVolume", 13);
		}

		public void TestBashFetchForView_JK_CorrectedConsolVolumeUnit()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_CorrectedConsolVolumeUnit", 13);
		}

		public void TestBashFetchForView_JK_TotalShipmentChargeable()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_TotalShipmentChargeable", 13);
		}

		public void TestBashFetchForView_JK_Calc_TotalShipmentChargeableUnit()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_Calc_TotalShipmentChargeableUnit", 13);
		}

		public void TestBashFetchForView_JK_DocsCutOff()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_DocsCutOff", 9);
		}

		public void TestBashFetchForView_JK_DepotReceivalCommences()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_DepotReceivalCommences", 9);
		}

		public void TestBashFetchForView_JK_CTOReceivalCommences()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_CTOReceivalCommences", 9);
		}

		public void TestBashFetchForView_JK_DepotCutOff()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_DepotCutOff", 9);
		}

		public void TestBashFetchForView_JK_CTOCutOff()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_CTOCutOff", 9);
		}

		public void TestBashFetchForView_JK_CTOAvailabilityDate()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_CTOAvailabilityDate", 9);
		}

		public void TestBashFetchForView_JK_DepotAvailabilityDate()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_DepotAvailabilityDate", 9);
		}

		public void TestBashFetchForView_JK_CTOStorageDate()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_CTOStorageDate", 9);
		}

		public void TestBashFetchForView_JK_DepotStorageDate()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_DepotStorageDate", 9);
		}

		public void TestBashFetchForView_JK_ConsolCutOffDateLocal()
		{
			BashFetchForView("JK_ConsolCutOffDateLocal", 0);
		}

		public void TestBashFetchForView_JK_CRN()
		{
			// CusEntryNum: 1

			BashFetchForView("JK_CRN", 1);
		}

		public void TestBashFetchForView_JK_EntryStatus()
		{
			BashFetchForView("JK_EntryStatus", 0);
		}

		public void TestBashFetchForView_JK_RoutingComplete()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_RoutingComplete", 9);
		}

		public void TestBashFetchForView_JK_VGMCutOff()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_VGMCutOff", 9);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_Description()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_Description", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_ActualDateForBinding()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_Description()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_Description", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_ScheduledDateForBinding()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding", 12);
		}

		public void TestBashFetchForView_JK_Calc_IsCargoOnly()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_Calc_IsCargoOnly", 9);
		}

		public void TestBashFetchForView_JK_IsHazardous()
		{
			BashFetchForView("JK_IsHazardous", 0);
		}

		public void TestBashFetchForView_JK_TotalShipmentActWeightCheck()
		{
			BashFetchForView("JK_TotalShipmentActWeightCheck", 0);
		}

		public void TestBashFetchForView_WeightVerificationUnit()
		{
			BashFetchForView("WeightVerificationUnit", 0);
		}

		public void TestBashFetchForView_JK_TotalShipmentActVolumeCheck()
		{
			BashFetchForView("JK_TotalShipmentActVolumeCheck", 0);
		}

		public void TestBashFetchForView_VolumeVerificationUnit()
		{
			BashFetchForView("VolumeVerificationUnit", 0);
		}

		public void TestBashFetchForView_JK_TotalShipmentQuantity()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_TotalShipmentQuantity", 13);
		}

		public void TestBashFetchForView_JK_TotalShipmentChargableCheck()
		{
			BashFetchForView("JK_TotalShipmentChargableCheck", 0);
		}

		public void TestBashFetchForView_JK_JX_JA_E_ARV()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_JX_JA_E_ARV", 9);
		}

		public void TestBashFetchForView_JK_JX_JA_A_ARV()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_JX_JA_A_ARV", 9);
		}

		public void TestBashFetchForView_Transports_DepartureTransport_JW_ETD()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("Transports+DepartureTransport+JW_ETD", 9);
		}

		public void TestBashFetchForView_Transports_DepartureTransport_JW_ATD()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("Transports+DepartureTransport+JW_ATD", 9);
		}

		public void TestBashFetchForView_Transports_ArrivalTransport_JW_ETA()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("Transports+ArrivalTransport+JW_ETA", 9);
		}

		public void TestBashFetchForView_Transports_ArrivalTransport_JW_ATA()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("Transports+ArrivalTransport+JW_ATA", 9);
		}

		public void TestBashFetchForView_Transports_DepartureTransport_JW_RL_NKLoadPort()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("Transports+DepartureTransport+JW_RL_NKLoadPort", 9);
		}

		public void TestBashFetchForView_Transports_ArrivalTransport_JW_RL_NKDiscPort()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConsolTransport: 1

			BashFetchForView("Transports+ArrivalTransport+JW_RL_NKDiscPort", 9);
		}

		public void TestBashFetchForView_JK_Calc_ContainerTypesSummary()
		{
			// JobContainer: 12
			// RefContainer: 1

			BashFetchForView("JK_Calc_ContainerTypesSummary", 13);
		}

		public void TestBashFetchForView_JK_Calc_ContainerStorageClassesSummary()
		{
			// JobContainer: 12
			// RefContainer: 1

			BashFetchForView("JK_Calc_ContainerStorageClassesSummary", 13);
		}

		public void TestBashFetchForView_JK_CoLoadMasterBill()
		{
			BashFetchForView("JK_CoLoadMasterBill", 0);
		}

		public void TestBashFetchForView_JK_CoLoadBookingReference()
		{
			BashFetchForView("JK_CoLoadBookingReference", 0);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyLastMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyLastMilestone_P9_Description()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_Description", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyLastMilestone_P9_ActualDateForBinding()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_ActualDateForBinding", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyNextMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyNextMilestone_P9_Description()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_Description", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyNextMilestone_P9_ScheduledDateForBinding()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_ScheduledDateForBinding", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Tasks_NextTask_P9_Description()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Tasks+NextTask+P9_Description", 12);
		}

		public void TestBashFetchForView_NotesChecker_HasSpecialInstructions()
		{
			// JobConShipLink: 1
			// JobContainer: 1
			// JobDeclaration: 1
			// JobDocumentData: 1
			// JobShipment: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// StmNote: 1

			BashFetchForView("NotesChecker.HasSpecialInstructions", 8);
		}

		public void TestBashFetchForView_NotesChecker_HasGoodsHandlingInstructions()
		{
			// JobConShipLink: 1
			// JobContainer: 1
			// JobDeclaration: 1
			// JobDocumentData: 1
			// JobShipment: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// StmNote: 1

			BashFetchForView("NotesChecker.HasGoodsHandlingInstructions", 8);
		}

		public void TestBashFetchForView_Density_WeightUtilisationPercentage()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("Density.WeightUtilisationPercentage", 13);
		}

		public void TestBashFetchForView_Density_VolumeUtilisationPercentage()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("Density.VolumeUtilisationPercentage", 13);
		}

		public void TestBashFetchForView_JK_Calc_CostFreePercentage()
		{
			// JobConShipLink: 13
			// JobContainerPackPivot: 13
			// JobPackLines: 13
			// JobShipment: 12
			// JobContainer: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_Calc_CostFreePercentage", 54);
		}

		public void TestBashFetchForView_JK_Calc_FreeSpace()
		{
			// JobConShipLink: 13
			// JobContainerPackPivot: 13
			// JobPackLines: 13
			// JobShipment: 12
			// JobContainer: 2
			// JobConsolTransport: 1

			BashFetchForView("JK_Calc_FreeSpace", 54);
		}

		public void TestBashFetchForView_JK_CostFreeUnitForBinding()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("JK_CostFreeUnitForBinding", 13);
		}

		public void TestBashFetchForView_Density_ExcessVolumeWeight()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("Density.ExcessVolumeWeight", 13);
		}

		public void TestBashFetchForView_Density_ExcessVolumeWeightUnit()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("Density.ExcessVolumeWeightUnit", 13);
		}

		public void TestBashFetchForView_Density_IsActualExcess()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("Density.IsActualExcess", 13);
		}

		public void TestBashFetchForView_Density_VolumeRatio()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("Density.VolumeRatio", 13);
		}

		public void TestBashFetchForView_Density_DensityFactor()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("Density.DensityFactor", 13);
		}

		public void TestBashFetchForView_Density_DensityRemark()
		{
			// JobShipment: 12
			// JobConShipLink: 1

			BashFetchForView("Density.DensityRemark", 13);
		}

		public void TestBashFetchForView_NumbersAsString()
		{
			// CusEntryNum: 1

			BashFetchForView("NumbersAsString", 1);
		}

		public void TestBashFetchForView_Transports_MostInterestingTransport_OnlineScheduleStatusDescription()
		{
			BashFetchForView("Transports+MostInterestingTransport+OnlineScheduleStatusDescription", 1);
		}

		public void TestBashFetchForView_JK_Calc_DGClass()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("JK_Calc_DGClass", 26);
		}

		public void TestBashFetchForView_JK_Calc_DGSubstance()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("JK_Calc_DGSubstance", 26);
		}

		public void TestBashFetchForView_Job_Branch_GB_Code()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// JobHeader: 1
			// UNDGDataItems: 1

			BashFetchForView("Job+Branch+GB_Code", 26);
		}

		public void TestBashFetchForView_Job_Department_GE_Code()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("Job+Department+GE_Code", 26);
		}

		public void TestBashFetchForView_Job_JH_Status()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("Job+JH_Status", 26);
		}
		public void TestBashFetchForView_Job_JH_HoldReason()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("Job+JH_HoldReason", 26);
		}

		public void TestBashFetchForView_Job_JH_GS_NKRepOps()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("Job+JH_GS_NKRepOps", 26);
		}

		public void TestBashFetchForView_JK_TotalLoadingMeters()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("JK_TotalLoadingMeters", 26);
		}

		public void TestBashFetchForView_NotifyParty_OH_Code()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("NotifyParty+OH_Code", 26);
		}

		public void TestBashFetchForView_NotifyParty_OH_FullName()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("NotifyParty+OH_FullName", 26);
		}

		public void TestBashFetchForView_NotifyParty2_OH_Code()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("NotifyParty2+OH_Code", 26);
		}

		public void TestBashFetchForView_NotifyParty2_OH_FullName()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("NotifyParty2+OH_FullName", 26);
		}

		public void TestBashFetchForView_NotifyParty3_OH_Code()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("NotifyParty3+OH_Code", 26);
		}

		public void TestBashFetchForView_NotifyParty3_OH_FullName()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("NotifyParty3+OH_FullName", 26);
		}

		public void TestBashFetchForView_JK_Calc_PossibleOversize()
		{
			// JobConShipLink: 13
			// JobContainerPackPivot: 13
			// JobPackLines: 13
			// JobDeclaration: 12
			// JobDocsAndCartage: 12
			// JobShipment: 12
			// JobContainer: 2
			// JobConsolTransport : 1

			BashFetchForView("JK_Calc_PossibleOversize", 78);
		}

		public void TestBashFetchForView_JK_ReleaseType()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("JK_ReleaseType", 26);
		}

		public void TestBashFetchForView_Job_JH_GS_NKRepSales()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("Job+JH_GS_NKRepSales", 26);
		}

		public void TestBashFetchForView_JK_Calc_ActualVolumeWeight()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("JK_Calc_ActualVolumeWeight", 26);
		}

		public void TestBashFetchForView_ShipmentCount()
		{
			// JobPackLines: 12
			// JobShipment: 12
			// JobConShipLink: 1
			// UNDGDataItems: 1

			BashFetchForView("ShipmentCount", 26);
		}

		public void TestBashFetchForView_JK_RL_NKCarrierBookingOffice()
		{
			BashFetchForView("JK_RL_NKCarrierBookingOffice", 0);
		}

		public void TestBashFetchForView_JK_Calc_CarrierBookingLatestDate()
		{
			// StmALog: 12

			BashFetchForView("JK_Calc_CarrierBookingLatestDate", 12);
		}

		public void TestBashFetchForView_JK_Calc_CarrierBookingLatestStatus()
		{
			// StmALog: 12

			BashFetchForView("JK_Calc_CarrierBookingLatestStatus", 12);
		}

		public void TestBashFetchForView_JK_RCA_AllocationLine()
		{
			BashFetchForView("JK_RCA_AllocationLine", 0);
		}

		public void TestBashFetchForView_TotalCO2e()
		{
			// JobCO2e: 1

			BashFetchForView("TotalCO2e", 1);
		}

		public void TestBashFetchForView_TotalCO2eForSorting()
		{
			// JobCO2e: 1

			BashFetchForView("TotalCO2eForSorting", 1);
		}

		public void TestBashFetchForView_CO2eStatus()
		{
			// JobCO2e: 1

			BashFetchForView("CO2eStatus", 1);
		}

		public void TestBashFetchForView_JK_RS_NKGatewayServiceLevel()
		{
			// StmALog: 12

			BashFetchForView("JK_RS_NKGatewayServiceLevel", 12);
		}

		public void TestBashFetchForView_JK_Calc_BillOfLadingBillStatus()
		{
			// StmALog: 12

			BashFetchForView("JK_Calc_BillOfLadingBillStatus", 12);
		}

		public void TestBashFetchForView_JK_Calc_BillOfLadingBillDate()
		{
			// StmALog: 12

			BashFetchForView("JK_Calc_BillOfLadingBillDate", 12);
		}

		public void TestBashFetchForView_JK_ElectronicBillOfLadingType()
		{
			BashFetchForView("JK_ElectronicBillOfLadingType", 1);
		}

		public void TestBashFetchForView_JK_ElectronicBillOfLadingTerms()
		{
			BashFetchForView("JK_ElectronicBillOfLadingTerms", 1);
		}

		public void TestBashFetchForView_JK_ElectronicBillOfLadingReference()
		{
			BashFetchForView("JK_ElectronicBillOfLadingReference", 1);
		}

		#endregion

		protected override SchemaPKColumn PkColumn => JobConsolSchema.PK;

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			var refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var gatewayAgentPort = factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var notifyParty = factory.NewWithValidTestData<OrgHeader>();
			var notifyParty2 = factory.NewWithValidTestData<OrgHeader>();
			var notifyParty3 = factory.NewWithValidTestData<OrgHeader>();

			for (var i = 0; i < 12; i++)
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;

				var voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
				voyage.GenerateSailings();

				var consol = factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AUSYD";

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "TEST2017";
				container.JC_RC = refContainer.PK;

				var transport = consol.Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_JX = voyage.Sailings[0].PK;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "shipment" + i;

				var job = new JobHeader.Loader(shipment).TryCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;

				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
				var jobHeader = new JobHeader.Loader(consol).TryCreate();
				jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
				jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;

				var outerPackline = shipment.OuterPackLines.AddNew();
				outerPackline.SetContainer(consol, container);

				outerPackline.UNDGs.AddNew();

				var milestone = consol.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				milestone = consol.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Invalid);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				var task = consol.WorkflowItems.Tasks.AddNew();
				task.P9_Type = "INV";
				task.P9_Description = "Investigation";
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

				consol.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;
				consol.NotifyParty2DocumentaryAddress.OrganisationPK = notifyParty2.PK;
				consol.NotifyParty3DocumentaryAddress.OrganisationPK = notifyParty3.PK;

				result.Add(consol.PK);
			}

			factory.Save();

			return result.ToArray();
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var consols = Factory.New<ForwardingShipment>().Consols;
			var filterBo = new JobConsolFilterBusinessObject();
			return new JobConsolFilterControl(consols, filterBo);
		}

		protected override string[] GetExcludedColumnNamesForTestFetchHint()
		{
			using (var filterControl = GetNewFilterStripControl())
			{
				var columnStyles = filterControl.FilteredGrid.ColumnStyles;
				for (int i = columnStyles.Count - 1; i >= 0; i--)
				{
					var columnInfo = (ZGridColumnInfo)columnStyles[i];
					var excludedColumns = new string[]
					{
						"JK_SecurityStatus",
						"IsTemplate",
						"TemplateRecord+STR_IsActive",
						"TemplateRecord+STR_TemplateName",
						"FirstSeaTransport",
						"LastSeaTransport",
						"FirstSeaLegLoadPortForBinding",
						"FirstSeaLegLoadPortETDForBinding",
						"FirstSeaLegLoadPortATDForBinding",
						"LastSeaLegDischargePortForBinding",
						"LastSeaLegDischargePortETAForBinding",
						"LastSeaLegDischargePortATAForBinding",
						"EarliestCTOStorageStartForBinding",
						"EarliestEmptyRequiredByDateForBinding",
					};

					if (!excludedColumns.Contains(columnInfo.ColumnName))
					{
						columnStyles.RemoveAt(i);
					}
				}

				ObjectFactory.Get<Enterprise.Integration.Customs.CA.IConsolModuleColumnsAndFiltersProvider>().AddColumns((IFilterControl)filterControl);
				ObjectFactory.Get<Enterprise.Integration.Customs.IForwardingConsolModuleCustomColumnsAndFiltersProvider>().AddColumns((IGridControl)filterControl);

				var columnNames = columnStyles
					.Cast<ZGridColumnInfo>()
					.Select(c => c.ColumnName)
					.ToArray();

				return columnNames;
			}
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new ForwardingModuleConsolCollection(Factory);
		}
	}
}
