using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveConsignmentTransportSupporter : TransportSupporter<WhsItemReceiveConsignment>
	{
		public WhsItemReceiveConsignmentTransportSupporter(WhsItemReceiveConsignment whsItemReceiveConsignment)
			: base(whsItemReceiveConsignment)
		{
		}

		public override ZGuid ShippingLine { get => ZGuid.Empty; set => _ = value; }

		public override ZString Description => string.Empty;

		public override ZString ConsignmentRef => string.Empty;

		public override ZString TransportMode => Parent.WRC_TransportMode;

		public override ZString ContainerMode => string.Empty;

		public override ZString BillOfLading => string.Empty;

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceTransitWarehouse;
	}
}
