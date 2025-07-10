using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReceiveAdviceProcessTaskCollection : ProcessTaskCollection
	{
		public CYDReceiveAdviceProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new CYDReceiveAdvice Parent => (CYDReceiveAdvice)base.Parent;

		public new CYDReceiveAdviceProcessTask this[int index] => (CYDReceiveAdviceProcessTask)Elements[index];

		public new CYDReceiveAdviceProcessTask AddNew() => (CYDReceiveAdviceProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new CYDReceiveAdviceProcessTaskCollection(Parent);
	}
}
