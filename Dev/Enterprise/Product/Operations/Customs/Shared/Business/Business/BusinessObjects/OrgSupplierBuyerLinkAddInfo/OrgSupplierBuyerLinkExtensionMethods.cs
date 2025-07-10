using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public static class OrgSupplierBuyerLinkExtensionMethods
	{
		public static OrgSupplierBuyerLinkAddInfo GetAddInfo(this OrgSupplierBuyerLink link)
		{
			OrgSupplierBuyerLinkAddInfo result = null;
			if (link != null)
			{
				result = (OrgSupplierBuyerLinkAddInfo)link.AddInfo;
			}
			return result;
		}
	}
}
