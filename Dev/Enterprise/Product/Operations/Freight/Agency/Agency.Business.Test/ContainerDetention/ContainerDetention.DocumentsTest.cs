using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business.Testing
{
	partial class ContainerDetentionTest
	{
		public void TestDocumentSupporter()
		{
			AssertType(typeof(ContainerDetentionDocumentSupporter), Detention.DocumentSupporter);
		}

		public void TestDocManagerSupport()
		{
			AssertType(typeof(DocManagerInfo), ((IDocManagerSupport)Detention).DocManagerInfo);
			AssertEquals(Constants.DocManagerCodes.DetentionInvoice, ((IDocManagerSupport)Detention).DocManagerInfo.DocManagerCode);
		}
	}
}
