using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardUnitStateProcessTaskCollection : ProcessTaskCollection
	{
		public CYDYardUnitStateProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new CYDYardUnitState Parent => (CYDYardUnitState)base.Parent;

		public new CYDYardUnitStateProcessTask this[int index] => (CYDYardUnitStateProcessTask)Elements[index];

		public new CYDYardUnitStateProcessTask AddNew() => (CYDYardUnitStateProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new CYDYardUnitStateProcessTaskCollection(Parent);
	}
}
