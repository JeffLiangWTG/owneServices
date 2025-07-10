using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVOriginLoadList : IBusiness
	{
		ZGuid PK { get; }

		IOrgHeader Carrier { get; }
		IRefContainer ContainerType { get; }
		IRefUNLOCO Destination { get; }
		IOrgAddress DestinationDepot { get; }
		ZAddress HVL_OA_DestinationDepot_ZAddress { get; }
		ZAddress HVL_OA_OriginDepot_ZAddress { get; }
		IRefUNLOCO Origin { get; }
		IOrgAddress OriginCTOAddress { get; }
		ZAddress OriginCTO_ZAddress { get; }
		IOrgAddress OriginDepot { get; }
		IOrgHeader Owner { get; }
		IRefServiceLevel ServiceLevel { get; }
		ZString HumanReadableNameWithoutId { get; }
		ZGuid OriginCTO { get; }
		ZString WorkflowType { get; }
		IProcessTaskCollection WorkflowItems { get; }

		ZString HVL_ContainerNumber { get; set; }
		ZDateTime HVL_E_Arv { get; set; }
		ZDateTime HVL_E_Dep { get; set; }
		ZString HVL_HouseBillNumber { get; set; }
		ZString HVL_MasterBillNumber { get; set; }
		ZGuid HVL_OA_DestinationDepot { get; set; }
		ZGuid HVL_OA_OriginDepot { get; set; }
		ZGuid HVL_OH_Carrier { get; set; }
		ZGuid HVL_OH_Owner { get; set; }
		ZGuid HVL_RC_ContainerType { get; set; }
		ZString HVL_RL_NKDestination { get; set; }
		ZString HVL_RL_NKOrigin { get; set; }
		ZBool HVL_IsMasterHouse { get; set; }
		ZString HVL_RS_NKServiceLevel { get; set; }
		ZString HVL_VesselName { get; set; }
		ZString HVL_Status { get; set; }
		ZDateTime HVL_SystemCreateTimeUtc { get; set; }
		ZString HVL_SystemCreateUser { get; set; }
		ZDateTime HVL_SystemLastEditTimeUtc { get; set; }
		ZString HVL_SystemLastEditUser { get; set; }
		ZString HVL_TransportMode { get; set; }
		ZString HVL_UniqueReference { get; set; }
		ZString HVL_VoyageFlight { get; set; }

		IHVLVOuterPackageCollection OuterPackages { get; }

		Logs Logs { get; }
		Notes Notes { get; }
	}
}
