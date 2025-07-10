using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class OrgSupplierPartLookups : Customs.Business.OrgSupplierPartLookups
	{
		public OrgSupplierPartLookups(OrgSupplierPart part)
			: base(part)
		{
		}

		public BaseClassificationCollection<CusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory);
	}
}
