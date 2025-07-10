using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IAllocationRouteAssignable
	{
		BusinessObjectFactory Factory { get; }

		ZString HumanReadableName { get; }
		ZString CarrierContractNumber { get; }
		ZString AllocationRouteID { get; }
		ZString ServiceProvider { get; }

		void UpdateCarrierContractAndAllocationDetails(IRatingContractAllocationLine route);
		bool HasCarrierOrRouteDifferentToOverride(IRatingContractAllocationLine route);
		bool IsAssignedAllocationRoute(IRatingContractAllocationLine route);
	}
}
