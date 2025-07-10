
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderHeaderProcessTaskCollection : ProcessTaskCollection
	{
		public MNRWorkOrderHeaderProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new MNRWorkOrderHeader Parent => (MNRWorkOrderHeader)base.Parent;

		public new MNRWorkOrderHeaderProcessTask this[int index] => (MNRWorkOrderHeaderProcessTask)Elements[index];

		public new MNRWorkOrderHeaderProcessTask AddNew() => (MNRWorkOrderHeaderProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new MNRWorkOrderHeaderProcessTaskCollection(Parent);
	}
}
