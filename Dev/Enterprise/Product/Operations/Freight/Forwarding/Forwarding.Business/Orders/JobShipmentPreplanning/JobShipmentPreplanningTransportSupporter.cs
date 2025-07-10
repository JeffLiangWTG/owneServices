using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public sealed class JobShipmentPreplanningTransportSupporter : TransportSupporter<JobShipmentPreplanning>
	{
		public JobShipmentPreplanningTransportSupporter(JobShipmentPreplanning parent)
			: base(parent) { }

		public override ZString BillOfLading
		{
			get { return Parent.EF_MasterBill; }
		}

		public override ZString ConsignmentRef
		{
			get { return Parent.EF_PreshipID; }
		}

		public override ZString ContainerMode
		{
			get { return ""; }
		}

		public override ZString Description
		{
			get { return Parent.EF_PreshipID; }
		}

		public override ZGuid ShippingLine
		{
			get { return ZGuid.Empty; }
			set { }
		}

		public override ZString TransportMode
		{
			get { return ""; }
		}

		public override JobConsolTransportValidation GetNewTransportValidator(Transport transport)
		{
			return new PreAdviceTransportValidation(transport);
		}

		public override void SetConsignmentRefIfNotSet()
		{
			base.SetConsignmentRefIfNotSet();
			Parent.SetPreshipIDIfNeeded();
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceOrders; }
		}
	}
}
