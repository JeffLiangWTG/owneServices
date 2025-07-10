using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteVehicleMovementProcessTaskCollection : ProcessTaskCollection
	{
		public GteVehicleMovementProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new GteVehicleMovement Parent => (GteVehicleMovement)base.Parent;

		public new GteVehicleMovementProcessTask this[int index] => (GteVehicleMovementProcessTask)Elements[index];

		public new GteVehicleMovementProcessTask AddNew() => (GteVehicleMovementProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new GteVehicleMovementProcessTaskCollection(Parent);
	}
}
