using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business
{
	public class ContainerPenaltyMatchFilter : IContainerPenaltyMatchFilter
	{
		public IOrgHeader Carrier { get; set; }
		public IOrgHeader Client { get; set; }
		public IOrgHeader CTO { get; set; }
		public IGlbCompany Company { get; set; }

		public ZString OriginPort { get; set; }
		public ZString DetentionPort { get; set; }
		public ZString ContainerClass { get; set; }
		public ZString Direction { get; set; }
		public ZString CreditorType { get; set; }
		public ZString ProcessType { get; set; }

		public ICommonContainer Container { get; set; }

		#region Shipments

		public IReadOnlyCollection<ICommonShipment> Shipments { get; set; }
		public GetClientsDelegate GetClientsFunction { get; set; }

		#endregion

		public ContainerPenaltyMatchFilter Clone()
		{
			return new ContainerPenaltyMatchFilter()
			{
				Container = Container,
				Carrier = Carrier,
				Client = Client,
				CTO = CTO,
				OriginPort = OriginPort,
				DetentionPort = DetentionPort,
				ContainerClass = ContainerClass,
				Direction = Direction,
				CreditorType = CreditorType,
				ProcessType = ProcessType,
				Shipments = Shipments,
				GetClientsFunction = GetClientsFunction,
			};
		}
	}
}
