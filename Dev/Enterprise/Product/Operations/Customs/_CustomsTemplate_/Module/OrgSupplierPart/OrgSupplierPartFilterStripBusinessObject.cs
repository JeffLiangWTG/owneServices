using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.Customs.Business;

namespace Enterprise.Customs._CustomsTemplate_.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes._TemplateCountryName_);
	}
}
