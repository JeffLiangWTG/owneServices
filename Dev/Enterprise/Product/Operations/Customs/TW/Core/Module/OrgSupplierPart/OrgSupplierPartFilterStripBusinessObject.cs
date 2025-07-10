using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Taiwan);
	}
}
