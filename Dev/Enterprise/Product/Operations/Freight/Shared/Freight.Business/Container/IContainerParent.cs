using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface IContainerParent : ITransportParent
	{
		ZString TransportMode { get; }
		OrgHeader ShippingLine { get; }
		RefUNLOCO LoadPort { get; }
		RefUNLOCO DischargePort { get; }

		IEnumerable<CommonContainer> Containers { get; }
		IEnumerable<JobDocsAndCartage> DocsAndCartage(CommonContainer container);

		#region Addresses

		OrgAddress ShippingLineAddress { get; }
		OrgAddress DeparturePackCFSTransportAddress { get; }
		OrgAddress ArrivalUnpackCFSTransportAddress { get; }
		OrgAddress DepartureCTOAddress { get; }
		OrgAddress ArrivalCTOAddress { get; }
		OrgAddress SendingForwarderAddress { get; }
		OrgAddress ReceivingForwarderAddress { get; }
		OrgAddress ContainerYardEmptyPickupAddress { get; }
		OrgAddress ContainerYardEmptyReturnAddress { get; }
		OrgAddress DepartureCFSAddress { get; }
		OrgAddress ArrivalCFSAddress { get; }

		#endregion
	}
}
