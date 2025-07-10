using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class OrgSupplierBuyerLinkExtensionMethods
	{
		public static ReconIssues GetReconIssueCalculated(this OrgSupplierBuyerLink link)
		{
			var result = ReconIssues.None;
			if (link != null)
			{
				var addInfo = link.GetAddInfo();
				if (addInfo != null)
				{
					result = ReconIssueCodeList.GetReconIssuesValue(addInfo.ZO_OtherReconIndicator);
				}
			}
			return result;
		}
	}
}
