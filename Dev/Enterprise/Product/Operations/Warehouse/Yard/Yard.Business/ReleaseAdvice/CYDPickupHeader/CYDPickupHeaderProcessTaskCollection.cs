using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPickupHeaderProcessTaskCollection : ProcessTaskCollection
	{
		public CYDPickupHeaderProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new CYDPickupHeader Parent => (CYDPickupHeader)base.Parent;

		public new CYDPickupHeaderProcessTask this[int index] => (CYDPickupHeaderProcessTask)Elements[index];

		public new CYDPickupHeaderProcessTask AddNew() => (CYDPickupHeaderProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new CYDPickupHeaderProcessTaskCollection(Parent);
	}
}
