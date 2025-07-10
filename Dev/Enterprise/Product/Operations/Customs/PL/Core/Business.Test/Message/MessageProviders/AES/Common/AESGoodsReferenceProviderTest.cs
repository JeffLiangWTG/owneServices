using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESGoodsReferenceProviderTest : DataProviderTestCase<AESGoodsReferenceProvider>
{
	public void TestSequenceNumber() => AssertEquals(1, Provider.SequenceNumber);

	public void TestDeclarationGoodsItemNumber() => AssertEquals(5, Provider.DeclarationGoodsItemNumber);

	protected override AESGoodsReferenceProvider GetProvider() => new AESGoodsReferenceProvider(1, 5);
}
