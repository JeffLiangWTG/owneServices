using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Integration
{
	public interface ICCACommonAssignmentValidationData : IFactoryProvider
	{
		ZString Name { get; }

		IOrgHeader ContractServiceProvider { get; }

		IRatingContract CarrierContract { get; }

		IRatingContractAllocationLine AllocationRoute { get; }

		ZDateTime ETD { get; }

		ZString LoadPort { get; }

		ZString DischargePort { get; }

		ZString VoyageFlight { get; }

		ZString Vessel { get; }

		IEnumerable<IForwardingContainer> Containers { get; }

		ZString UniqueConsignRef { get; }

		ZString TransportMode { get; }
	}
}
