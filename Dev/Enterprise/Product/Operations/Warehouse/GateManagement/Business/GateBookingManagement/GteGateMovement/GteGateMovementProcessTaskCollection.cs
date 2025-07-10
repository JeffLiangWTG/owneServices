using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteGateMovementProcessTaskCollection : ProcessTaskCollection
	{
		public GteGateMovementProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new GteGateMovement Parent => (GteGateMovement)base.Parent;

		public new GteGateMovementProcessTask this[int index] => (GteGateMovementProcessTask)Elements[index];

		public new GteGateMovementProcessTask AddNew() => (GteGateMovementProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new GteGateMovementProcessTaskCollection(Parent);
	}
}
