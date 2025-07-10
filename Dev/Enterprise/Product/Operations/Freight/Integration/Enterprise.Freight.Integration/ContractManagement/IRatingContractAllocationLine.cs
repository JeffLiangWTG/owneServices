using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IRatingContractAllocationLine : IBusiness
	{
		ZGuid PK { get; }
		ZGuid RCA_RCT_RatingContract { get; set; }
		ZGuid RCA_RC_ContainerType { get; set; }
		ZGuid RCA_JX_SailingSchedule { get; set; }
		ZDate RCA_StartDate { get; set; }
		ZDate RCA_ExpiryDate { get; set; }
		ZDate StartDateWithContractFallback { get; }
		ZDate ExpiryDateWithContractFallback { get; }
		ZDateTime LinkedScheduleETD { get; }
		ZDateTime LinkedScheduleSTD { get; }
		ZDateTime LinkedScheduleETA { get; }
		ZDateTime LinkedScheduleSTA { get; }
		ZString RCA_LoadLocation { get; set; }
		ZString RCA_DischargeLocation { get; set; }
		ZString RCA_Calc_LoadLocation { get; }
		ZString RCA_Calc_DischargeLocation { get; }
		ZString RCA_PlaceOfReceipt { get; set; }
		ZString RCA_PlaceOfDelivery { get; set; }
		ZShort RCA_AllocatedQuantity { get; set; }
		ZString RCA_AllocatedUQ { get; set; }
		ZString RCA_AllocationLineID { get; set; }
		ZString RCA_VoyageNumber { get; set; }
		ZString RCA_RV_NKVessel { get; set; }
		ZString RCA_StorageOrFreightRateClass { get; set; }
		ZString RCA_ServiceLoop { get; set; }
		ZString RCA_ContainerOwner { get; set; }
		ZDecimal RCA_ContainerWeightLimit { get; set; }
		ZString RCA_ContainerWeightLimitUQ { get; set; }
		ZString RCA_ContainerWeightLimitType { get; set; }
		ZString RCA_Calc_VoyageNumber { get; }
		ZString RCA_Calc_VesselName { get; }
		ZString RCA_Calc_ServiceString { get; }
		ZString ParentContractNumber { get; }
		ZString NamedAccountsFormatted { get; }
		ZString LinkedSchedule { get; }
		ZDecimal RCA_BookingVariance { get; set; }
		ZDecimal Utilization { get; }
		ZDecimal CapacityWithVariance { get; }
		ZDecimal OutstandingCommitted { get; }
		ZDecimal OutstandingWithVariance { get; }
		ZBool RCA_AllowRelatedPorts { get; set; }
		ZBool RCA_AllowGatewayConsolOnly { get; set; }
		ZBool RCA_AllowGroupageOnly { get; set; }
		ZBool RCA_HasBookingLimit { get; set; }
		ZBool LinkedScheduleETDUpdated { get; }
		ZGuid RCA_RCA_ParentAllocationRoute { get; }
		IRatingContract Contract { get; }
		IJobSailing JobSailing { get; }
		IRefContainer RefContainerType { get; }
		IRelatedNamedAccountsPivotCollection<IRatingContractNamedAccountPivot> NamedAccountPivots { get; }
		IRelatedAgentsPivotCollection<IAllocationRouteAgentPivot> AgentPivots { get; }
		IRatingContractAllocationLine ParentAllocationRoute { get; }
		IAllocationDistributionCollection AllocationDistributions { get; }
	}
}
