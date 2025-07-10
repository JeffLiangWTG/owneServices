using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HTransportContractDocumentTest : TestCaseWithFactory
	{
		public void TestID()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_BillNumber = "AAA1234";
			ITransportContractDocument transportContractDocument = new N5101HTransportContractDocument(bill);
			AssertEquals("AAA1234", transportContractDocument.ID);
		}
	}
}
