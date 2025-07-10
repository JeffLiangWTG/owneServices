using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	partial class SundryChargesTest
	{
		public void TestDocManagerInfo()
		{
			AssertEquals(Constants.DocManagerCodes.AgencySundryCharges, ((IDocManagerSupport)Sundry).DocManagerInfo.DocManagerCode);
		}

		public void TestDocumentSupporter()
		{
			AssertType(typeof(SundryChargesDocumentSupporter), ((IDocumentSupportable)Sundry).DocumentSupporter);
		}

		public void TestEDocsProviderSupporter()
		{
			AssertType(typeof(JobInvoicingEDocsProviderSupporter), ((IEDocsProvider)Sundry).GetEDocsProviderSupporter());
		}
	}
}
