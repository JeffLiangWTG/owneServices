using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.MasterFiles;

namespace Enterprise.Customs.NZ.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory);
	}
}
