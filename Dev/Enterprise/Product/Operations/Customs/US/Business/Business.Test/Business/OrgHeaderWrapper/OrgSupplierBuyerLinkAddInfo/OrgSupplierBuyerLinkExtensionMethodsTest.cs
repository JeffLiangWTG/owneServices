using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class OrgSupplierBuyerLinkExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetReconIssueCalculated()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			var addInfo = link.GetAddInfo();
			AssertEquals("Recon Issue", ReconIssues.None, link.GetReconIssueCalculated());

			addInfo.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.Value9802Recon;
			AssertEquals("Recon Issue", ReconIssues._98 | ReconIssues.VL, link.GetReconIssueCalculated());
		}
	}
}
