using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDDeliveryHeaderProcessTaskCollection : ProcessTaskCollection
	{
		public CYDDeliveryHeaderProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new CYDDeliveryHeader Parent => (CYDDeliveryHeader)base.Parent;

		public new CYDDeliveryHeaderProcessTask this[int index] => (CYDDeliveryHeaderProcessTask)Elements[index];

		public new CYDDeliveryHeaderProcessTask AddNew() => (CYDDeliveryHeaderProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new CYDDeliveryHeaderProcessTaskCollection(Parent);
	}
}
