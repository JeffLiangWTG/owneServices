using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IRatingContract : IBusiness
	{
		ZGuid PK { get; }
		ZString RCT_ContractNumber { get; set; }
		ZBool RCT_IsActive { get; set; }
		ZGuid RCT_OH { get; set; }
		ZGuid RCT_GC { get; set; }
		ZDate RCT_StartDate { get; set; }
		ZDate RCT_EndDate { get; set; }
		ZString RCT_ContractType { get; set; }
		ZString RCT_TransportMode { get; set; }
		ZString RCT_AutoratingDateFiltering { get; set; }
		ZString RCT_ContainerType { get; set; }
		ZBool RCT_AllowHazardousCommodities { get; set; }
		ZString RCT_GS_NKContractOwner { get; set; }
		IOrgHeader ServiceProvider { get; }
		IRatingContractAllocationLineCollection Allocations { get; }
		IRelatedNamedAccountsPivotCollection<IRatingContractNamedAccountPivot> NamedAccountPivots { get; }
		IRatingContractContainerDetentionCollection ContainerDetentions { get; }
		ICarrierContractQuantityUnitPair CarrierContractQuantities { get; }
		ICarrierContractQuantityUnitPair CarrierContractCapacityWithVariance { get; }
		ICarrierContractQuantityUnitPair CurrentContractUtilisation { get; }
		ICarrierContractQuantityUnitPair CurrentContractOutstandingCommitted { get; }
		ICarrierContractQuantityUnitPair CurrentContractOutstandingWithVariance { get; }
	}
}
