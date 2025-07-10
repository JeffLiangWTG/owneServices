using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDAdHocServiceOrderProcessTaskCollection : ProcessTaskCollection
	{
		public CYDAdHocServiceOrderProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new CYDAdHocServiceOrder Parent => (CYDAdHocServiceOrder)base.Parent;

		public new CYDAdHocServiceOrderProcessTask this[int index] => (CYDAdHocServiceOrderProcessTask)Elements[index];

		public new CYDAdHocServiceOrderProcessTask AddNew() => (CYDAdHocServiceOrderProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new CYDAdHocServiceOrderProcessTaskCollection(Parent);
	}
}
