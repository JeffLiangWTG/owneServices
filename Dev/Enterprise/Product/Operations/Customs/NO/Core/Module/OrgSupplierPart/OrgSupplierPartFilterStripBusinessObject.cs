using Enterprise.Customs.Business;
using Enterprise.Customs.NO.Business;

namespace Enterprise.Customs.NO.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Norway);
	}
}
