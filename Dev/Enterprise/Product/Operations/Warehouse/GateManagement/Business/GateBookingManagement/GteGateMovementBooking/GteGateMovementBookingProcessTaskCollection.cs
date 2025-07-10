using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteGateMovementBookingProcessTaskCollection : ProcessTaskCollection
	{
		public GteGateMovementBookingProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new GteGateMovementBooking Parent => (GteGateMovementBooking)base.Parent;

		public new GteGateMovementBookingProcessTask this[int index] => (GteGateMovementBookingProcessTask)Elements[index];

		public new GteGateMovementBookingProcessTask AddNew() => (GteGateMovementBookingProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new GteGateMovementBookingProcessTaskCollection(Parent);
	}
}
