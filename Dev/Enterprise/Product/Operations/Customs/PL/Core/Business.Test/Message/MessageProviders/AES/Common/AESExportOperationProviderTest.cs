using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESExportOperationProviderTest : AESExportOperationProviderTestBase<AESExportOperationProvider, IExportOperation>
{
	protected override AESExportOperationProvider GetProvider() => new AESExportOperationProvider(SendingObject);
}
