using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitProcessTaskCollection : ProcessTaskCollection
	{
		public CYDTransportationUnitProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new CYDTransportationUnit Parent => (CYDTransportationUnit)base.Parent;

		public new CYDTransportationUnitProcessTask this[int index] => (CYDTransportationUnitProcessTask)Elements[index];

		public new CYDTransportationUnitProcessTask AddNew() => (CYDTransportationUnitProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new CYDTransportationUnitProcessTaskCollection(Parent);
	}
}
