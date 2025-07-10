using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business.Testing
{
	partial class VoyageAccountTest
	{
		public void TestDocManager()
		{
			IEDocsProvider provider = Account1;
			AssertEquals(Constants.DocManagerCodes.AgencyVoyageAccounting, provider.DocManagerInfo.DocManagerCode);
		}

		public void TestDocumentSupporter()
		{
			AssertType(typeof(VoyageAccountDocumentSupporter), ((IDocumentSupportable)Account1).DocumentSupporter);
		}
	}
}
