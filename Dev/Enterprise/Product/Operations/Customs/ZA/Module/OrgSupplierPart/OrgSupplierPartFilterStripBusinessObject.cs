using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business;

namespace Enterprise.Customs.ZA.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory);
	}
}
