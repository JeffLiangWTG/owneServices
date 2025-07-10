using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteBookingProcessTaskCollection : ProcessTaskCollection
	{
		public GteBookingProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new GteBooking Parent => (GteBooking)base.Parent;

		public new GteBookingProcessTask this[int index] => (GteBookingProcessTask)Elements[index];

		public new GteBookingProcessTask AddNew() => (GteBookingProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new GteBookingProcessTaskCollection(Parent);
	}
}
