using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReleaseAdviceProcessTaskCollection : ProcessTaskCollection
	{
		public CYDReleaseAdviceProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public new CYDReleaseAdvice Parent => (CYDReleaseAdvice)base.Parent;

		public new CYDReleaseAdviceProcessTask this[int index] => (CYDReleaseAdviceProcessTask)Elements[index];

		public new CYDReleaseAdviceProcessTask AddNew() => (CYDReleaseAdviceProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new CYDReleaseAdviceProcessTaskCollection(Parent);
	}
}
