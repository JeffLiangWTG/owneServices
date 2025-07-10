using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierBuyerLinkToleranceCollection : ActiveBusinessObjectCollection<OrgSupplierBuyerLinkTolerance>
	{
		public OrgSupplierBuyerLinkToleranceCollection(OrgSupplierBuyerLink orgSupplierBuyerLink)
			: base(orgSupplierBuyerLink.Factory, orgSupplierBuyerLink, null, OrgSupplierBuyerLinkToleranceSchema.OLT_OL_SupplierBuyerLink)
		{
		}
	}
}
