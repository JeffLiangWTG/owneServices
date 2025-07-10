using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC515ExportOperationTest : AESExportOperationProviderTestBase<CC515ExportOperationProvider, ICC515CExportOperation>
{
	public void TestReferenceNumber() => AssertEquals("ReferenceNumberTest", GetProvider().ReferenceNumber);

	protected override CC515ExportOperationProvider GetProvider() => new CC515ExportOperationProvider(SendingObject);
}
